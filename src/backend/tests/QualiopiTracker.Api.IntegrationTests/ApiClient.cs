namespace QualiopiTracker.Api.IntegrationTests;

internal static class ApiClient
{
    public const string DemoPassword = "TestDemo123!";

    public static async Task<HttpClient> LoggedInClientAsync(
        this WebApplicationFactory<Program> factory,
        string email,
        string password = DemoPassword)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);

        return client;
    }

    public static async Task<HttpClient> SignedUpClientAsync(this WebApplicationFactory<Program> factory, string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new SignUpRequest(
            "Test Provider", null, ["TrainingCourses"], "Test Lead", email, DemoPassword, DemoPassword));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);

        return client;
    }

    /// <summary>A validly signed token whose subject is missing or chosen by the test.</summary>
    public static string TokenFor(this WebApplicationFactory<Program> factory, string? subject)
    {
        var settings = factory.Services.GetRequiredService<JwtSettings>();
        var claims = new Dictionary<string, object> { [JwtClaims.Email] = "ghost@example.com" };
        if (subject is not null)
        {
            claims[JwtClaims.Subject] = subject;
        }

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = claims,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(settings.SigningKey), SecurityAlgorithms.HmacSha256),
        });
    }
}
