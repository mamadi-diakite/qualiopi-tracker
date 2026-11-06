namespace QualiopiTracker.Api.IntegrationTests;

public sealed class FrameworkEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    [Fact]
    public async Task Members_see_the_framework_with_the_demo_applicability()
    {
        var client = await factory.LoggedInClientAsync("hugo.lambert@example.com");

        var criteria = await client.GetFromJsonAsync<List<CriterionResponse>>("/api/framework");

        Assert.Equal(7, criteria?.Count);
        var indicators = criteria!.SelectMany(c => c.Indicators).ToList();
        Assert.Equal(32, indicators.Count);
        Assert.DoesNotContain(indicators, i => i.Status == "OutOfScope");
        var subcontracting = indicators.Single(i => i.Number == 27);
        Assert.Equal("NotApplicable", subcontracting.Status);
        Assert.StartsWith("We deliver every course", subcontracting.NotApplicableReason);
        Assert.StartsWith("Results are published", indicators.Single(i => i.Number == 2).Notes);
    }

    [Fact]
    public async Task Apprenticeship_indicators_are_out_of_scope_for_a_skills_assessment_provider()
    {
        var client = await factory.LoggedInClientAsync("julien.faure@example.com");

        var criteria = await client.GetFromJsonAsync<List<CriterionResponse>>("/api/framework");

        Assert.Equal(
            [14, 15, 20, 29],
            criteria!.SelectMany(c => c.Indicators).Where(i => i.Status == "OutOfScope").Select(i => i.Number));
    }

    [Fact]
    public async Task Gets_one_indicator()
    {
        var client = await factory.LoggedInClientAsync("julien.faure@example.com");

        var indicator = await client.GetFromJsonAsync<IndicatorResponse>("/api/framework/indicators/4");

        Assert.Equal("Needs analysis", indicator?.Title);
        Assert.Equal(2, indicator?.CriterionNumber);
        Assert.Equal("Applicable", indicator?.Status);
        Assert.StartsWith("Each assessment starts", indicator?.Notes);
    }

    [Fact]
    public async Task Unknown_indicator_returns_404()
    {
        var client = await factory.LoggedInClientAsync("hugo.lambert@example.com");

        var response = await client.GetAsync("/api/framework/indicators/33");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("This indicator does not exist.", problem?.Title);
    }

    [Fact]
    public async Task Quality_lead_marks_an_indicator_not_applicable_then_applicable_again()
    {
        var client = await factory.SignedUpClientAsync("framework.lead@example.com");

        var marked = await client.PutAsJsonAsync(
            "/api/framework/indicators/28", new UpdateIndicatorRequest("No work placements.", "Owner: me."));

        Assert.Equal(HttpStatusCode.OK, marked.StatusCode);
        var indicator = await marked.Content.ReadFromJsonAsync<IndicatorResponse>();
        Assert.Equal("NotApplicable", indicator?.Status);
        Assert.Equal("No work placements.", indicator?.NotApplicableReason);

        var cleared = await client.PutAsJsonAsync("/api/framework/indicators/28", new UpdateIndicatorRequest(null, "Owner: me."));
        Assert.Equal("Applicable", (await cleared.Content.ReadFromJsonAsync<IndicatorResponse>())?.Status);
        var reloaded = await client.GetFromJsonAsync<IndicatorResponse>("/api/framework/indicators/28");
        Assert.Equal("Owner: me.", reloaded?.Notes);
    }

    [Fact]
    public async Task Out_of_scope_indicator_cannot_be_marked_not_applicable()
    {
        var client = await factory.SignedUpClientAsync("scope.lead@example.com");

        var response = await client.PutAsJsonAsync(
            "/api/framework/indicators/14", new UpdateIndicatorRequest("No apprentices.", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["notApplicableReason"], problem!.Errors.Keys);
    }

    [Fact]
    public async Task Members_cannot_change_indicators()
    {
        var client = await factory.LoggedInClientAsync("sarah.benali@example.com");

        var response = await client.PutAsJsonAsync("/api/framework/indicators/1", new UpdateIndicatorRequest(null, "Hello"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Only the quality lead can change indicators.", problem?.Title);
    }

    [Fact]
    public async Task Framework_needs_a_known_user()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/framework")).StatusCode);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.TokenFor(subject: null));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/framework")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/framework/indicators/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync(
            "/api/framework/indicators/1", new UpdateIndicatorRequest(null, null))).StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", factory.TokenFor(subject: Guid.NewGuid().ToString()));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/framework")).StatusCode);
    }
}
