namespace QualiopiTracker.Application.Tests;

public sealed class FrameworkServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryProviderIndicatorRepository _settings = new();
    private readonly FrameworkService _service;
    private readonly Provider _provider;
    private readonly User _lead;
    private readonly User _member;

    public FrameworkServiceTests()
    {
        _service = new FrameworkService(_users, _settings, new FixedTimeProvider(Now));
        _provider = new Provider("Horizon Skills Academy", null, ActivityCategories.TrainingCourses, Now.UtcDateTime);
        _lead = new User(_provider.Id, "claire.morel@example.com", "Claire", "hash", UserRole.QualityLead, Now.UtcDateTime);
        _member = new User(_provider.Id, "hugo.lambert@example.com", "Hugo", "hash", UserRole.Member, Now.UtcDateTime);
        _users.Providers.Add(_provider);
        _users.Users.AddRange([_lead, _member]);
    }

    [Fact]
    public async Task Framework_lists_every_criterion_with_its_indicators()
    {
        var result = await _service.GetFrameworkAsync(_member.Id);

        Assert.Equal(FrameworkStatus.Ok, result.Status);
        var criteria = result.Value!;
        Assert.Equal(Enumerable.Range(1, 7), criteria.Select(c => c.Number));
        Assert.Equal("Information on the offer", criteria[0].Title);
        Assert.Equal([1, 2, 3], criteria[0].Indicators.Select(i => i.Number));
        Assert.Equal(32, criteria.Sum(c => c.Indicators.Count));
        Assert.All(criteria, c => Assert.All(c.Indicators, i => Assert.Equal(c.Number, i.CriterionNumber)));
    }

    [Fact]
    public async Task Applicability_follows_the_provider_categories_and_reasons()
    {
        _settings.Settings.Add(Setting(27, "We never subcontract.", "Checked yearly."));
        _settings.Settings.Add(Setting(14, "Kept from an apprenticeship period.", null));
        var otherProvider = Guid.NewGuid();
        var foreign = new ProviderIndicator(otherProvider, 1, Now.UtcDateTime);
        foreign.Update("Not ours", null, Now.UtcDateTime);
        _settings.Settings.Add(foreign);

        var indicators = (await _service.GetFrameworkAsync(_lead.Id)).Value!.SelectMany(c => c.Indicators).ToList();

        var subcontracting = indicators.Single(i => i.Number == 27);
        Assert.Equal("NotApplicable", subcontracting.Status);
        Assert.Equal("We never subcontract.", subcontracting.NotApplicableReason);
        Assert.Equal("Checked yearly.", subcontracting.Notes);
        Assert.Equal("OutOfScope", indicators.Single(i => i.Number == 14).Status);
        Assert.Equal(["Apprenticeship"], indicators.Single(i => i.Number == 14).AppliesTo);
        Assert.Equal("Applicable", indicators.Single(i => i.Number == 1).Status);
        Assert.Null(indicators.Single(i => i.Number == 1).NotApplicableReason);
        Assert.Equal(
            ["TrainingCourses", "SkillsAssessments", "PriorExperienceAccreditation", "Apprenticeship"],
            indicators.Single(i => i.Number == 1).AppliesTo);
        Assert.Equal(4, indicators.Count(i => i.Status == "OutOfScope"));
    }

    [Fact]
    public async Task Unknown_user_or_provider_gets_nothing()
    {
        Assert.Equal(FrameworkStatus.UserNotFound, (await _service.GetFrameworkAsync(Guid.NewGuid())).Status);
        Assert.Equal(FrameworkStatus.UserNotFound, (await _service.GetIndicatorAsync(Guid.NewGuid(), 1)).Status);
        Assert.Equal(FrameworkStatus.UserNotFound, (await _service.UpdateIndicatorAsync(Guid.NewGuid(), 1, Update())).Status);

        _users.Providers.Clear();

        Assert.Equal(FrameworkStatus.UserNotFound, (await _service.GetFrameworkAsync(_lead.Id)).Status);
        Assert.Equal(FrameworkStatus.UserNotFound, (await _service.UpdateIndicatorAsync(_lead.Id, 1, Update())).Status);
    }

    [Fact]
    public async Task Gets_one_indicator_with_its_notes()
    {
        _settings.Settings.Add(Setting(2, null, "Published every January."));

        var result = await _service.GetIndicatorAsync(_member.Id, 2);

        Assert.Equal(FrameworkStatus.Ok, result.Status);
        Assert.Equal(
            new IndicatorResponse(2, 1, "Results figures", result.Value!.AppliesTo, "Applicable", null, "Published every January."),
            result.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(33)]
    public async Task Unknown_indicator_is_not_found(int number)
    {
        Assert.Equal(FrameworkStatus.IndicatorNotFound, (await _service.GetIndicatorAsync(_lead.Id, number)).Status);
        Assert.Equal(FrameworkStatus.IndicatorNotFound, (await _service.UpdateIndicatorAsync(_lead.Id, number, Update())).Status);
    }

    [Fact]
    public async Task Quality_lead_marks_an_indicator_as_not_applicable_with_notes()
    {
        var result = await _service.UpdateIndicatorAsync(
            _lead.Id, 27, new UpdateIndicatorRequest(" We never subcontract. ", " Owner: Claire. "));

        Assert.Equal(FrameworkStatus.Ok, result.Status);
        Assert.Equal("NotApplicable", result.Value?.Status);
        Assert.Equal("We never subcontract.", result.Value?.NotApplicableReason);
        Assert.Equal("Owner: Claire.", result.Value?.Notes);
        var setting = Assert.Single(_settings.Settings);
        Assert.Equal(_provider.Id, setting.ProviderId);
        Assert.Equal(Now.UtcDateTime, setting.UpdatedAtUtc);
        Assert.Equal(1, _settings.SaveCount);
    }

    [Fact]
    public async Task A_blank_reason_makes_the_indicator_applicable_again()
    {
        _settings.Settings.Add(Setting(27, "We never subcontract.", "Notes"));

        var result = await _service.UpdateIndicatorAsync(_lead.Id, 27, new UpdateIndicatorRequest(" ", "Notes"));

        Assert.Equal("Applicable", result.Value?.Status);
        Assert.Null(result.Value?.NotApplicableReason);
        Assert.Single(_settings.Settings);
        Assert.Equal(1, _settings.SaveCount);
    }

    [Fact]
    public async Task Notes_can_be_added_to_an_out_of_scope_indicator()
    {
        var result = await _service.UpdateIndicatorAsync(_lead.Id, 14, new UpdateIndicatorRequest(null, "For later."));

        Assert.Equal("OutOfScope", result.Value?.Status);
        Assert.Equal("For later.", result.Value?.Notes);
    }

    [Fact]
    public async Task An_out_of_scope_indicator_cannot_be_marked_not_applicable()
    {
        var result = await _service.UpdateIndicatorAsync(_lead.Id, 14, new UpdateIndicatorRequest("No apprentices.", null));

        AssertInvalid(result, "notApplicableReason");
        Assert.Equal(
            ["This indicator is outside your activity categories, so it already does not apply."],
            result.Errors["notApplicableReason"]);
    }

    [Fact]
    public async Task Update_limits_the_lengths()
    {
        var result = await _service.UpdateIndicatorAsync(_lead.Id, 27, new UpdateIndicatorRequest(
            new string('a', ProviderIndicator.NotApplicableReasonMaxLength + 1),
            new string('b', ProviderIndicator.NotesMaxLength + 1)));

        AssertInvalid(result, "notApplicableReason");
        Assert.Equal(["notApplicableReason", "notes"], result.Errors.Keys.Order());
    }

    [Fact]
    public async Task Members_cannot_change_indicators()
    {
        var result = await _service.UpdateIndicatorAsync(_member.Id, 27, Update());

        Assert.Equal(FrameworkStatus.NotQualityLead, result.Status);
        Assert.Empty(_settings.Settings);
        Assert.Equal(0, _settings.SaveCount);
    }

    [Fact]
    public async Task Update_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateIndicatorAsync(_lead.Id, 1, null!));
    }

    private static UpdateIndicatorRequest Update() => new("We never subcontract.", null);

    private ProviderIndicator Setting(int number, string? reason, string? notes)
    {
        var setting = new ProviderIndicator(_provider.Id, number, Now.UtcDateTime);
        setting.Update(reason, notes, Now.UtcDateTime);
        return setting;
    }

    private void AssertInvalid(FrameworkResult<IndicatorResponse> result, string field)
    {
        Assert.Equal(FrameworkStatus.Invalid, result.Status);
        Assert.Null(result.Value);
        Assert.Contains(field, result.Errors.Keys);
        Assert.Empty(_settings.Settings);
        Assert.Equal(0, _settings.SaveCount);
    }
}
