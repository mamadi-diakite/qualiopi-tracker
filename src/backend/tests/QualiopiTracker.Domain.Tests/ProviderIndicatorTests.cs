namespace QualiopiTracker.Domain.Tests;

public sealed class ProviderIndicatorTests
{
    private static readonly Guid ProviderId = Guid.NewGuid();

    [Fact]
    public void Starts_without_reason_or_notes()
    {
        var setting = new ProviderIndicator(ProviderId, 27, TestDates.Now);

        Assert.Equal(ProviderId, setting.ProviderId);
        Assert.Equal(27, setting.IndicatorNumber);
        Assert.Null(setting.NotApplicableReason);
        Assert.Null(setting.Notes);
        Assert.Equal(TestDates.Now, setting.UpdatedAtUtc);
    }

    [Fact]
    public void Update_trims_and_stores_the_values()
    {
        var setting = new ProviderIndicator(ProviderId, 27, TestDates.Now);
        var later = TestDates.Now.AddHours(1);

        setting.Update(" We never subcontract. ", " Checked every year. ", later);

        Assert.Equal("We never subcontract.", setting.NotApplicableReason);
        Assert.Equal("Checked every year.", setting.Notes);
        Assert.Equal(later, setting.UpdatedAtUtc);
    }

    [Fact]
    public void Blank_values_clear_the_reason_and_the_notes()
    {
        var setting = new ProviderIndicator(ProviderId, 27, TestDates.Now);
        setting.Update("We never subcontract.", "Notes", TestDates.Now);

        setting.Update(" ", null, TestDates.Now);

        Assert.Null(setting.NotApplicableReason);
        Assert.Null(setting.Notes);
    }

    [Fact]
    public void Rejected_update_keeps_the_values()
    {
        var setting = new ProviderIndicator(ProviderId, 27, TestDates.Now);
        setting.Update("Reason", "Notes", TestDates.Now);

        Assert.Equal("notes", Assert.Throws<ArgumentException>(() => setting.Update(
            "Other", new string('a', ProviderIndicator.NotesMaxLength + 1), TestDates.Now)).ParamName);
        Assert.Equal("notApplicableReason", Assert.Throws<ArgumentException>(() => setting.Update(
            new string('a', ProviderIndicator.NotApplicableReasonMaxLength + 1), null, TestDates.Now)).ParamName);
        Assert.Equal("updatedAtUtc", Assert.Throws<ArgumentException>(() => setting.Update(
            "Other", null, DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);

        Assert.Equal("Reason", setting.NotApplicableReason);
        Assert.Equal("Notes", setting.Notes);
    }

    [Fact]
    public void Rejects_invalid_values()
    {
        Assert.Equal("providerId", Assert.Throws<ArgumentException>(
            () => new ProviderIndicator(Guid.Empty, 1, TestDates.Now)).ParamName);
        Assert.Equal("indicatorNumber", Assert.Throws<ArgumentOutOfRangeException>(
            () => new ProviderIndicator(ProviderId, 33, TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new ProviderIndicator(ProviderId, 1, DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Unspecified))).ParamName);
    }
}
