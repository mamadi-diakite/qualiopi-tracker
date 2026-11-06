namespace QualiopiTracker.Infrastructure.Tests;

public sealed class UserRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly Provider _provider = new("Horizon Skills Academy", null, ActivityCategories.TrainingCourses, Now);
    private readonly User _user;

    public UserRepositoryTests()
    {
        _user = new User(_provider.Id, "claire.morel@example.com", "Claire", "hash", UserRole.QualityLead, Now);
        using var db = _database.CreateContext();
        db.Providers.Add(_provider);
        db.Users.Add(_user);
        db.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Finds_users_by_email_and_by_id_and_their_provider()
    {
        await using var db = _database.CreateContext();
        var repository = new UserRepository(db);

        Assert.Equal(_user.Id, (await repository.FindByEmailAsync("claire.morel@example.com", default))?.Id);
        Assert.Equal(_user.Email, (await repository.FindByIdAsync(_user.Id, default))?.Email);
        Assert.Equal("Horizon Skills Academy", (await repository.FindProviderAsync(_provider.Id, default))?.Name);
        Assert.Null(await repository.FindByEmailAsync("nobody@example.com", default));
        Assert.Null(await repository.FindByIdAsync(Guid.NewGuid(), default));
        Assert.Null(await repository.FindProviderAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task Adds_a_provider_with_its_lead_and_saves_changes()
    {
        var provider = new Provider("Pathway Career Assessments", null, ActivityCategories.SkillsAssessments, Now);
        var lead = new User(provider.Id, "julien.faure@example.com", "Julien", "hash", UserRole.QualityLead, Now);
        await using (var db = _database.CreateContext())
        {
            var repository = new UserRepository(db);
            repository.AddProvider(provider);
            repository.Add(lead);
            (await repository.FindByIdAsync(_user.Id, default))!.ChangePassword("new-hash");
            (await repository.FindProviderAsync(_provider.Id, default))!
                .UpdateProfile("Horizon Academy", "12345678901", ActivityCategories.All);
            await repository.SaveChangesAsync(default);
        }

        await using var check = _database.CreateContext();
        var saved = await check.Users.Include(u => u.Provider).SingleAsync(u => u.Id == lead.Id);
        Assert.Equal("Pathway Career Assessments", saved.Provider?.Name);
        Assert.Equal("new-hash", (await check.Users.SingleAsync(u => u.Id == _user.Id)).PasswordHash);
        var updated = await check.Providers.SingleAsync(p => p.Id == _provider.Id);
        Assert.Equal("Horizon Academy", updated.Name);
        Assert.Equal(ActivityCategories.All, updated.ActivityCategories);
    }
}
