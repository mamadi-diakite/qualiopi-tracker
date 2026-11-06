namespace QualiopiTracker.Infrastructure.Tests;

public sealed class QualiopiTrackerDbContextTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Saves_and_reads_a_provider_with_its_users()
    {
        var provider = new Provider(
            "Northwind Training", "12345678901",
            ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, Now);
        var lead = new User(provider.Id, "claire.morel@example.com", "Claire Morel", "hash", UserRole.QualityLead, Now);
        var member = new User(provider.Id, "hugo.lambert@example.com", "Hugo Lambert", "hash", UserRole.Member, Now);

        await using (var db = _database.CreateContext())
        {
            db.Providers.Add(provider);
            db.Users.AddRange(lead, member);
            await db.SaveChangesAsync();
        }

        await using (var db = _database.CreateContext())
        {
            var users = await db.Users.Include(u => u.Provider).OrderBy(u => u.Email).ToListAsync();

            Assert.Equal(UserRole.QualityLead, users[0].Role);
            Assert.Equal(UserRole.Member, users[1].Role);
            Assert.All(users, u => Assert.Equal("Northwind Training", u.Provider!.Name));
            Assert.Equal("12345678901", users[0].Provider!.RegistrationNumber);
            Assert.Equal(
                ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship,
                users[0].Provider!.ActivityCategories);
            Assert.Equal(DateTimeKind.Utc, users[0].CreatedAtUtc.Kind);
            Assert.Equal(DateTimeKind.Utc, users[0].Provider!.CreatedAtUtc.Kind);
        }
    }

    [Fact]
    public async Task Email_is_unique()
    {
        var provider = new Provider("Northwind Training", null, ActivityCategories.All, Now);
        await using var db = _database.CreateContext();
        db.Providers.Add(provider);
        db.Users.Add(new User(provider.Id, "claire@example.com", "Claire", "hash", UserRole.QualityLead, Now));
        db.Users.Add(new User(provider.Id, "Claire@Example.com", "Claire 2", "hash", UserRole.Member, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task A_provider_with_users_cannot_be_deleted()
    {
        var provider = new Provider("Northwind Training", null, ActivityCategories.All, Now);
        await using (var db = _database.CreateContext())
        {
            db.Providers.Add(provider);
            db.Users.Add(new User(provider.Id, "claire@example.com", "Claire", "hash", UserRole.QualityLead, Now));
            await db.SaveChangesAsync();
        }

        await using var context = _database.CreateContext();
        context.Providers.Remove(await context.Providers.SingleAsync());

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
