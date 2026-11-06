namespace QualiopiTracker.Api.IntegrationTests;

public sealed class ProviderEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    [Fact]
    public async Task Members_see_their_provider_profile()
    {
        var client = await factory.LoggedInClientAsync("hugo.lambert@example.com");

        var profile = await client.GetFromJsonAsync<ProviderProfileResponse>("/api/provider");

        Assert.Equal("Horizon Skills Academy", profile?.Name);
        Assert.Equal("12345678901", profile?.RegistrationNumber);
        Assert.Equal(["TrainingCourses", "Apprenticeship"], profile?.ActivityCategories);
    }

    [Fact]
    public async Task Quality_lead_updates_the_profile()
    {
        var client = await factory.SignedUpClientAsync("profile.lead@example.com");

        var response = await client.PutAsJsonAsync("/api/provider", new UpdateProviderProfileRequest(
            "Renamed Provider", "10987654321", ["SkillsAssessments", "Apprenticeship"]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProviderProfileResponse>();
        Assert.Equal("Renamed Provider", profile?.Name);
        Assert.Equal("10987654321", profile?.RegistrationNumber);
        Assert.Equal(["SkillsAssessments", "Apprenticeship"], profile?.ActivityCategories);
        var reloaded = await client.GetFromJsonAsync<ProviderProfileResponse>("/api/provider");
        Assert.Equal("Renamed Provider", reloaded?.Name);
    }

    [Fact]
    public async Task Invalid_profile_returns_a_validation_problem()
    {
        var client = await factory.SignedUpClientAsync("invalid.profile@example.com");

        var response = await client.PutAsJsonAsync(
            "/api/provider", new UpdateProviderProfileRequest(" ", null, ["Webinars"]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["activityCategories", "name"], problem!.Errors.Keys.Order());
    }

    [Fact]
    public async Task Members_cannot_edit_the_profile()
    {
        var client = await factory.LoggedInClientAsync("sarah.benali@example.com");

        var response = await client.PutAsJsonAsync(
            "/api/provider", new UpdateProviderProfileRequest("Taken Over", null, ["TrainingCourses"]));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Only the quality lead can edit the provider profile.", problem?.Title);
    }

    [Fact]
    public async Task Profile_needs_a_known_user()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/provider")).StatusCode);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.TokenFor(subject: null));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/provider")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync(
            "/api/provider", new UpdateProviderProfileRequest("Name", null, ["TrainingCourses"]))).StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", factory.TokenFor(subject: Guid.NewGuid().ToString()));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/provider")).StatusCode);
    }
}
