namespace QualiopiTracker.Application.Tests;

public sealed class ProviderProfileServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryUserRepository _users = new();
    private readonly ProviderProfileService _service;
    private readonly Provider _provider;
    private readonly User _lead;
    private readonly User _member;

    public ProviderProfileServiceTests()
    {
        _service = new ProviderProfileService(_users);
        _provider = new Provider(
            "Horizon Skills Academy", "12345678901",
            ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, Now);
        _lead = new User(_provider.Id, "claire.morel@example.com", "Claire Morel", "hash", UserRole.QualityLead, Now);
        _member = new User(_provider.Id, "hugo.lambert@example.com", "Hugo Lambert", "hash", UserRole.Member, Now);
        _users.Providers.Add(_provider);
        _users.Users.AddRange([_lead, _member]);
    }

    [Fact]
    public async Task Every_member_sees_the_profile()
    {
        var result = await _service.GetAsync(_member.Id);

        Assert.Equal(ProviderProfileStatus.Ok, result.Status);
        Assert.Equal(_provider.Id, result.Profile?.Id);
        Assert.Equal("Horizon Skills Academy", result.Profile?.Name);
        Assert.Equal("12345678901", result.Profile?.RegistrationNumber);
        Assert.Equal(["TrainingCourses", "Apprenticeship"], result.Profile?.ActivityCategories);
    }

    [Fact]
    public async Task Unknown_user_has_no_profile()
    {
        Assert.Equal(ProviderProfileStatus.UserNotFound, (await _service.GetAsync(Guid.NewGuid())).Status);
    }

    [Fact]
    public async Task User_whose_provider_is_gone_has_no_profile()
    {
        _users.Providers.Clear();

        Assert.Equal(ProviderProfileStatus.UserNotFound, (await _service.GetAsync(_lead.Id)).Status);
        Assert.Equal(ProviderProfileStatus.UserNotFound, (await _service.UpdateAsync(_lead.Id, Update())).Status);
    }

    [Fact]
    public async Task Quality_lead_updates_the_profile()
    {
        var result = await _service.UpdateAsync(
            _lead.Id, new UpdateProviderProfileRequest(" Horizon Academy ", " ", ["SkillsAssessments"]));

        Assert.Equal(ProviderProfileStatus.Ok, result.Status);
        Assert.Equal("Horizon Academy", result.Profile?.Name);
        Assert.Null(result.Profile?.RegistrationNumber);
        Assert.Equal(["SkillsAssessments"], result.Profile?.ActivityCategories);
        Assert.Equal("Horizon Academy", _provider.Name);
        Assert.Null(_provider.RegistrationNumber);
        Assert.Equal(ActivityCategories.SkillsAssessments, _provider.ActivityCategories);
        Assert.Equal(1, _users.SaveCount);
    }

    [Fact]
    public async Task Members_cannot_update_the_profile()
    {
        var result = await _service.UpdateAsync(_member.Id, Update());

        Assert.Equal(ProviderProfileStatus.NotQualityLead, result.Status);
        AssertUnchanged();
    }

    [Fact]
    public async Task Unknown_user_cannot_update_the_profile()
    {
        Assert.Equal(ProviderProfileStatus.UserNotFound, (await _service.UpdateAsync(Guid.NewGuid(), Update())).Status);
        AssertUnchanged();
    }

    [Fact]
    public async Task Update_reports_every_invalid_field()
    {
        var result = await _service.UpdateAsync(_lead.Id, new UpdateProviderProfileRequest(" ", "123", []));

        Assert.Equal(ProviderProfileStatus.Invalid, result.Status);
        Assert.Null(result.Profile);
        Assert.Equal(["activityCategories", "name", "registrationNumber"], result.Errors.Keys.Order());
        Assert.Equal(["Provider name is required."], result.Errors["name"]);
        AssertUnchanged();
    }

    [Fact]
    public async Task Update_limits_the_name_length()
    {
        var result = await _service.UpdateAsync(
            _lead.Id, Update() with { Name = new string('a', Provider.NameMaxLength + 1) });

        Assert.Equal(["Use at most 150 characters."], result.Errors["name"]);
        AssertUnchanged();
    }

    [Fact]
    public async Task Update_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(_lead.Id, null!));
    }

    private static UpdateProviderProfileRequest Update() =>
        new("Horizon Academy", "10987654321", ["TrainingCourses"]);

    private void AssertUnchanged()
    {
        Assert.Equal("Horizon Skills Academy", _provider.Name);
        Assert.Equal(ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, _provider.ActivityCategories);
        Assert.Equal(0, _users.SaveCount);
    }
}
