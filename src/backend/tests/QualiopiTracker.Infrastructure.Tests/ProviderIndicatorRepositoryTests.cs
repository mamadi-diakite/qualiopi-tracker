namespace QualiopiTracker.Infrastructure.Tests;

public sealed class ProviderIndicatorRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly Provider _provider = new("Horizon Skills Academy", null, ActivityCategories.All, Now);
    private readonly Provider _other = new("Pathway Career Assessments", null, ActivityCategories.All, Now);

    public ProviderIndicatorRepositoryTests()
    {
        using var db = _database.CreateContext();
        db.Providers.AddRange(_provider, _other);
        db.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Adds_lists_finds_and_updates_settings_of_one_provider()
    {
        await using (var db = _database.CreateContext())
        {
            var repository = new ProviderIndicatorRepository(db);
            var subcontracting = new ProviderIndicator(_provider.Id, 27, Now);
            subcontracting.Update("We never subcontract.", null, Now);
            repository.Add(subcontracting);
            repository.Add(new ProviderIndicator(_provider.Id, 2, Now));
            repository.Add(new ProviderIndicator(_other.Id, 27, Now));
            await repository.SaveChangesAsync(default);
        }

        await using (var db = _database.CreateContext())
        {
            var repository = new ProviderIndicatorRepository(db);

            Assert.Equal([2, 27], (await repository.ListAsync(_provider.Id, default)).Select(s => s.IndicatorNumber).Order());
            var found = await repository.FindAsync(_provider.Id, 27, default);
            Assert.Equal("We never subcontract.", found?.NotApplicableReason);
            Assert.Equal(DateTimeKind.Utc, found?.UpdatedAtUtc.Kind);
            Assert.Null(await repository.FindAsync(_provider.Id, 3, default));

            found!.Update(null, "Now with notes.", Now.AddDays(1));
            await repository.SaveChangesAsync(default);
        }

        await using var check = _database.CreateContext();
        var saved = await check.ProviderIndicators.SingleAsync(p => p.ProviderId == _provider.Id && p.IndicatorNumber == 27);
        Assert.Null(saved.NotApplicableReason);
        Assert.Equal("Now with notes.", saved.Notes);
    }

    [Fact]
    public async Task One_setting_per_provider_and_indicator()
    {
        await using var db = _database.CreateContext();
        db.ProviderIndicators.Add(new ProviderIndicator(_provider.Id, 1, Now));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.ProviderIndicators.Add(new ProviderIndicator(_provider.Id, 1, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Settings_need_an_existing_provider()
    {
        await using var db = _database.CreateContext();
        db.ProviderIndicators.Add(new ProviderIndicator(Guid.NewGuid(), 1, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
