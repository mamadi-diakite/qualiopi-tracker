namespace QualiopiTracker.Infrastructure.Tests;

public sealed class DemoDataSeederTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly IdentityPasswordHasher _hasher = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Seeds_the_demo_providers_with_one_quality_lead_each()
    {
        Assert.True(await SeedAsync());

        await using var db = _database.CreateContext();
        var providers = await db.Providers.OrderBy(p => p.Name).ToListAsync();
        var users = await db.Users.Include(u => u.Provider).ToListAsync();

        Assert.Equal(DemoDataSeeder.Providers.Select(p => p.Name).Order(), providers.Select(p => p.Name));
        var horizon = Assert.Single(providers, p => p.Name == "Horizon Skills Academy");
        Assert.Equal(ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, horizon.ActivityCategories);
        Assert.All(providers, p => Assert.Single(users, u => u.ProviderId == p.Id && u.Role == UserRole.QualityLead));
        Assert.Equal(2, users.Count(u => u.Role == UserRole.Member && u.ProviderId == horizon.Id));
        Assert.Equal(DemoDataSeeder.Providers.Sum(p => p.Users.Count), users.Count);
        Assert.All(users, u => Assert.EndsWith("@example.com", u.Email));
        Assert.All(users, u => Assert.Equal(Now, u.CreatedAtUtc));
    }

    [Fact]
    public async Task Seeds_indicator_notes_and_a_not_applicable_reason()
    {
        await SeedAsync();

        await using var db = _database.CreateContext();
        var horizon = await db.Providers.SingleAsync(p => p.Name == "Horizon Skills Academy");
        var settings = await db.ProviderIndicators.Where(p => p.ProviderId == horizon.Id)
            .OrderBy(p => p.IndicatorNumber).ToListAsync();

        Assert.Equal([2, 27], settings.Select(s => s.IndicatorNumber));
        Assert.Null(settings[0].NotApplicableReason);
        Assert.StartsWith("Results are published", settings[0].Notes);
        Assert.StartsWith("We deliver every course", settings[1].NotApplicableReason);
        Assert.Equal(DemoDataSeeder.Providers.Sum(p => p.Indicators.Count), await db.ProviderIndicators.CountAsync());
    }

    [Fact]
    public async Task Every_demo_account_accepts_the_demo_password()
    {
        await SeedAsync();

        await using var db = _database.CreateContext();
        foreach (var user in await db.Users.ToListAsync())
        {
            Assert.True(_hasher.Verify(user.PasswordHash, DemoDataSeeder.DemoPassword), user.Email);
        }
    }

    [Fact]
    public async Task Second_run_adds_nothing()
    {
        await SeedAsync();

        Assert.False(await SeedAsync());

        await using var db = _database.CreateContext();
        Assert.Equal(4, await db.Users.CountAsync());
        Assert.Equal(2, await db.Providers.CountAsync());
    }

    [Fact]
    public async Task Skips_a_database_that_already_has_users()
    {
        await using (var db = _database.CreateContext())
        {
            var provider = new Provider("Someone Training", null, ActivityCategories.All, Now);
            db.Providers.Add(provider);
            db.Users.Add(new User(provider.Id, "someone@example.com", "Someone", "hash", UserRole.QualityLead, Now));
            await db.SaveChangesAsync();
        }

        Assert.False(await SeedAsync());

        await using var check = _database.CreateContext();
        Assert.Equal(1, await check.Users.CountAsync());
        Assert.Equal(1, await check.Providers.CountAsync());
    }

    private async Task<bool> SeedAsync()
    {
        await using var db = _database.CreateContext();

        return await new DemoDataSeeder(db, _hasher, new FixedTimeProvider(Now)).SeedAsync();
    }
}
