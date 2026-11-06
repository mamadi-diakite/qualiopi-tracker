namespace QualiopiTracker.Api.IntegrationTests;

public sealed class DatabaseStartupTests(QualiopiTrackerApiFactory factory) : IClassFixture<QualiopiTrackerApiFactory>
{
    [Fact]
    public async Task Startup_applies_all_migrations()
    {
        _ = factory.CreateClient();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<QualiopiTrackerDbContext>();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(0, await db.Providers.CountAsync());
    }
}
