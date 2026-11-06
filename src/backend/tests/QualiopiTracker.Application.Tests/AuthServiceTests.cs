namespace QualiopiTracker.Application.Tests;

public sealed class AuthServiceTests
{
    private const string Password = "TestDemo123!";
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly AuthService _service;
    private readonly User _claire;

    public AuthServiceTests()
    {
        _service = new AuthService(_users, _hasher, new FakeTokenIssuer(), new FixedTimeProvider(Now));
        var provider = new Provider("Horizon Skills Academy", null, ActivityCategories.TrainingCourses, Now.UtcDateTime);
        _claire = new User(
            provider.Id, "claire.morel@example.com", "Claire Morel", "hashed:" + Password, UserRole.QualityLead, Now.UtcDateTime);
        _users.Providers.Add(provider);
        _users.Users.Add(_claire);
    }

    [Fact]
    public async Task Sign_up_creates_a_provider_with_its_quality_lead_and_opens_a_session()
    {
        var result = await _service.SignUpAsync(new SignUpRequest(
            " Pathway Career Assessments ", " 10987654321 ", ["SkillsAssessments", "PriorExperienceAccreditation"],
            " Julien Faure ", " Julien.Faure@Example.com ", "NewPass456", "NewPass456"));

        Assert.Empty(result.Errors);
        var provider = Assert.Single(_users.Providers, p => p.Name == "Pathway Career Assessments");
        Assert.Equal("10987654321", provider.RegistrationNumber);
        Assert.Equal(
            ActivityCategories.SkillsAssessments | ActivityCategories.PriorExperienceAccreditation,
            provider.ActivityCategories);
        Assert.Equal(Now.UtcDateTime, provider.CreatedAtUtc);
        var user = Assert.Single(_users.Users, u => u.Email == "julien.faure@example.com");
        Assert.Equal("Julien Faure", user.DisplayName);
        Assert.Equal(UserRole.QualityLead, user.Role);
        Assert.Equal(provider.Id, user.ProviderId);
        Assert.Equal("hashed:NewPass456", user.PasswordHash);
        Assert.Equal(Now.UtcDateTime, user.CreatedAtUtc);
        Assert.Equal(1, _users.SaveCount);
        Assert.Equal($"token-for-{user.Id}", result.Response?.AccessToken);
        Assert.Equal(
            new CurrentUserResponse(user.Id, user.Email, "Julien Faure", "QualityLead", provider.Id),
            result.Response?.User);
    }

    [Fact]
    public async Task Registration_number_is_optional_at_sign_up()
    {
        var result = await _service.SignUpAsync(SignUp(registrationNumber: null));

        Assert.Empty(result.Errors);
        Assert.Null(Assert.Single(_users.Providers, p => p.Name == "New Provider").RegistrationNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Sign_up_requires_a_provider_name(string providerName)
    {
        var result = await _service.SignUpAsync(SignUp(providerName: providerName));

        AssertSignUpInvalid(result, "providerName");
        Assert.Equal(["Provider name is required."], result.Errors["providerName"]);
    }

    [Fact]
    public async Task Sign_up_limits_the_provider_name_length()
    {
        var result = await _service.SignUpAsync(SignUp(providerName: new string('a', Provider.NameMaxLength + 1)));

        AssertSignUpInvalid(result, "providerName");
    }

    [Fact]
    public async Task Sign_up_checks_the_registration_number()
    {
        var result = await _service.SignUpAsync(SignUp(registrationNumber: "12-34"));

        AssertSignUpInvalid(result, "registrationNumber");
        Assert.Equal(["Registration number must be 11 digits."], result.Errors["registrationNumber"]);
    }

    [Fact]
    public async Task Sign_up_needs_at_least_one_activity_category()
    {
        var result = await _service.SignUpAsync(SignUp(categories: []));

        AssertSignUpInvalid(result, "activityCategories");
        Assert.Equal(["Choose at least one activity category."], result.Errors["activityCategories"]);
    }

    [Fact]
    public async Task Sign_up_rejects_an_unknown_activity_category()
    {
        var result = await _service.SignUpAsync(SignUp(categories: ["Webinars"]));

        AssertSignUpInvalid(result, "activityCategories");
        Assert.Equal(["Unknown activity category."], result.Errors["activityCategories"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Sign_up_requires_a_name(string displayName)
    {
        var result = await _service.SignUpAsync(SignUp(displayName: displayName));

        AssertSignUpInvalid(result, "displayName");
        Assert.Equal(["Name is required."], result.Errors["displayName"]);
    }

    [Fact]
    public async Task Sign_up_limits_the_name_length()
    {
        var result = await _service.SignUpAsync(SignUp(displayName: new string('a', User.DisplayNameMaxLength + 1)));

        AssertSignUpInvalid(result, "displayName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task Sign_up_requires_a_valid_email(string email)
    {
        var result = await _service.SignUpAsync(SignUp(email: email));

        AssertSignUpInvalid(result, "email");
        Assert.Equal(["Enter a valid email address."], result.Errors["email"]);
    }

    [Fact]
    public async Task Sign_up_refuses_an_email_already_used()
    {
        var result = await _service.SignUpAsync(SignUp(email: "CLAIRE.MOREL@example.com"));

        AssertSignUpInvalid(result, "email");
        Assert.Equal(["An account already uses this email."], result.Errors["email"]);
    }

    [Fact]
    public async Task Sign_up_applies_the_password_policy_and_confirmation()
    {
        var result = await _service.SignUpAsync(SignUp(password: "short", confirmPassword: "other"));

        AssertSignUpInvalid(result, "password");
        Assert.Equal(["confirmPassword", "password"], result.Errors.Keys.Order());
    }

    [Fact]
    public async Task Sign_up_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SignUpAsync(null!));
    }

    [Fact]
    public async Task Login_returns_a_token_and_the_user()
    {
        var response = await _service.LoginAsync(new LoginRequest(" Claire.Morel@Example.com ", Password));

        Assert.NotNull(response);
        Assert.Equal($"token-for-{_claire.Id}", response.AccessToken);
        Assert.Equal(FakeTokenIssuer.ExpiresAt, response.ExpiresAtUtc);
        Assert.Equal(
            new CurrentUserResponse(_claire.Id, "claire.morel@example.com", "Claire Morel", "QualityLead", _claire.ProviderId),
            response.User);
    }

    [Fact]
    public async Task Login_fails_with_a_wrong_password()
    {
        Assert.Null(await _service.LoginAsync(new LoginRequest("claire.morel@example.com", "wrong-password1")));
    }

    [Fact]
    public async Task Login_fails_with_an_unknown_email_and_still_hashes()
    {
        var response = await _service.LoginAsync(new LoginRequest("nobody@example.com", Password));

        Assert.Null(response);
        Assert.Equal(1, _hasher.HashCount);
    }

    [Theory]
    [InlineData("", Password)]
    [InlineData("  ", Password)]
    [InlineData("claire.morel@example.com", "")]
    public async Task Login_fails_with_blank_values(string email, string password)
    {
        Assert.Null(await _service.LoginAsync(new LoginRequest(email, password)));
        Assert.Equal(0, _hasher.HashCount);
    }

    [Fact]
    public async Task Login_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.LoginAsync(null!));
    }

    [Fact]
    public async Task Current_user_is_found_by_id()
    {
        var user = await _service.GetCurrentUserAsync(_claire.Id);

        Assert.Equal("claire.morel@example.com", user?.Email);
        Assert.Null(await _service.GetCurrentUserAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Change_password_stores_the_new_hash()
    {
        var result = await _service.ChangePasswordAsync(_claire.Id, new ChangePasswordRequest(Password, "NewPass456", "NewPass456"));

        Assert.Equal(ChangePasswordStatus.Changed, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("hashed:NewPass456", _claire.PasswordHash);
        Assert.Equal(1, _users.SaveCount);
    }

    [Fact]
    public async Task Change_password_for_an_unknown_user()
    {
        var result = await _service.ChangePasswordAsync(Guid.NewGuid(), new ChangePasswordRequest(Password, "NewPass456", "NewPass456"));

        Assert.Equal(ChangePasswordStatus.UserNotFound, result.Status);
        Assert.Equal(0, _users.SaveCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("WrongPass789")]
    public async Task Change_password_checks_the_current_password(string currentPassword)
    {
        var result = await _service.ChangePasswordAsync(
            _claire.Id, new ChangePasswordRequest(currentPassword, "NewPass456", "NewPass456"));

        AssertChangeInvalid(result, "currentPassword");
    }

    [Fact]
    public async Task Change_password_applies_the_password_policy()
    {
        var result = await _service.ChangePasswordAsync(_claire.Id, new ChangePasswordRequest(Password, "short", "short"));

        AssertChangeInvalid(result, "newPassword");
        Assert.Contains("Use at least 8 characters.", result.Errors["newPassword"]);
    }

    [Fact]
    public async Task New_password_must_differ_from_the_current_one()
    {
        var result = await _service.ChangePasswordAsync(_claire.Id, new ChangePasswordRequest(Password, Password, Password));

        AssertChangeInvalid(result, "newPassword");
        Assert.Equal(["New password must be different from the current one."], result.Errors["newPassword"]);
    }

    [Fact]
    public async Task Confirmation_must_match()
    {
        var result = await _service.ChangePasswordAsync(_claire.Id, new ChangePasswordRequest(Password, "NewPass456", "NewPass457"));

        AssertChangeInvalid(result, "confirmPassword");
    }

    [Fact]
    public async Task All_errors_are_reported_together()
    {
        var result = await _service.ChangePasswordAsync(_claire.Id, new ChangePasswordRequest("bad", "short", "other"));

        Assert.Equal(["confirmPassword", "currentPassword", "newPassword"], result.Errors.Keys.Order());
    }

    [Fact]
    public async Task Change_password_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ChangePasswordAsync(_claire.Id, null!));
    }

    private static SignUpRequest SignUp(
        string providerName = "New Provider",
        string? registrationNumber = "12345678901",
        string[]? categories = null,
        string displayName = "New Lead",
        string email = "new.lead@example.com",
        string password = "NewPass456",
        string? confirmPassword = null) =>
        new(providerName, registrationNumber, categories ?? ["TrainingCourses"], displayName, email, password,
            confirmPassword ?? password);

    private void AssertSignUpInvalid(SignUpResult result, string field)
    {
        Assert.Null(result.Response);
        Assert.Contains(field, result.Errors.Keys);
        Assert.Single(_users.Users);
        Assert.Single(_users.Providers);
        Assert.Equal(0, _users.SaveCount);
    }

    private void AssertChangeInvalid(ChangePasswordResult result, string field)
    {
        Assert.Equal(ChangePasswordStatus.Invalid, result.Status);
        Assert.Contains(field, result.Errors.Keys);
        Assert.Equal("hashed:" + Password, _claire.PasswordHash);
        Assert.Equal(0, _users.SaveCount);
    }
}
