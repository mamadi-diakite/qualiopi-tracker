namespace QualiopiTracker.Api.IntegrationTests;

public sealed class AuthEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    private const string DemoPassword = ApiClient.DemoPassword;

    [Fact]
    public async Task Sign_up_creates_a_provider_and_its_quality_lead_and_returns_a_session()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new SignUpRequest(
            "Bright Path Training", "10987654321", ["TrainingCourses", "PriorExperienceAccreditation"],
            "Lucie Garnier", "lucie.garnier@example.com", "NewPass456", "NewPass456"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/auth/me", response.Headers.Location?.AbsolutePath);
        var session = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.Equal("lucie.garnier@example.com", session?.User.Email);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        var me = await client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");
        Assert.Equal("Lucie Garnier", me?.DisplayName);
        Assert.Equal("QualityLead", me?.Role);
        var profile = await client.GetFromJsonAsync<ProviderProfileResponse>("/api/provider");
        Assert.Equal(me?.ProviderId, profile?.Id);
        Assert.Equal("Bright Path Training", profile?.Name);
        Assert.Equal(["TrainingCourses", "PriorExperienceAccreditation"], profile?.ActivityCategories);
    }

    [Fact]
    public async Task Sign_up_with_errors_returns_a_validation_problem()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/sign-up", new SignUpRequest(
            "", "12", [], " ", "claire.morel@example.com", "short", "other"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(
            ["activityCategories", "confirmPassword", "displayName", "email", "password", "providerName", "registrationNumber"],
            problem!.Errors.Keys.Order());
    }

    [Fact]
    public async Task Login_with_a_demo_account_returns_a_token()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new LoginRequest("Claire.Morel@Example.com", DemoPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body?.AccessToken));
        Assert.Equal("claire.morel@example.com", body!.User.Email);
        Assert.Equal("QualityLead", body.User.Role);
    }

    [Theory]
    [InlineData("claire.morel@example.com", "WrongPass123")]
    [InlineData("nobody@example.com", DemoPassword)]
    [InlineData("", "")]
    public async Task Login_with_bad_credentials_returns_401(string email, string password)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Email or password is not correct.", problem?.Title);
    }

    [Fact]
    public async Task Me_returns_the_logged_in_member()
    {
        var client = await factory.LoggedInClientAsync("hugo.lambert@example.com");

        var user = await client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");

        Assert.Equal("hugo.lambert@example.com", user?.Email);
        Assert.Equal("Member", user?.Role);
    }

    [Fact]
    public async Task Protected_endpoints_need_a_token()
    {
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/change-password", new ChangePasswordRequest(DemoPassword, "NewPass456", "NewPass456"))).StatusCode);
    }

    [Fact]
    public async Task A_token_without_a_user_id_is_refused()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.TokenFor(subject: null));

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/change-password", new ChangePasswordRequest(DemoPassword, "NewPass456", "NewPass456"))).StatusCode);
    }

    [Fact]
    public async Task A_token_of_a_deleted_user_is_refused()
    {
        const string email = "temporary@example.com";
        var client = await factory.SignedUpClientAsync(email);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<QualiopiTrackerDbContext>();
            db.Users.Remove(db.Users.Single(u => u.Email == email));
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/change-password", new ChangePasswordRequest(DemoPassword, "NewPass456", "NewPass456"))).StatusCode);
    }

    [Fact]
    public async Task Change_password_with_errors_returns_a_validation_problem()
    {
        var client = await factory.LoggedInClientAsync("sarah.benali@example.com");

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password", new ChangePasswordRequest("WrongPass789", "short", "other"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["confirmPassword", "currentPassword", "newPassword"], problem!.Errors.Keys.Order());
    }

    [Fact]
    public async Task Changed_password_replaces_the_old_one()
    {
        var client = await factory.SignedUpClientAsync("lea.marchand@example.com");

        var response = await client.PostAsJsonAsync(
            "/api/auth/change-password", new ChangePasswordRequest(DemoPassword, "NewPass456", "NewPass456"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("lea.marchand@example.com", DemoPassword))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await anonymous.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("lea.marchand@example.com", "NewPass456"))).StatusCode);
    }
}
