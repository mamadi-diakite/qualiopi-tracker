namespace QualiopiTracker.Infrastructure.Seeding;

public sealed class DemoDataSeeder(QualiopiTrackerDbContext db, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    public const string DemoPassword = "TestDemo123!";

    /// <summary>Fictional providers, their accounts and indicator notes. All names and numbers are made up.</summary>
    public static IReadOnlyList<DemoProvider> Providers { get; } =
    [
        new(
            "Horizon Skills Academy",
            "12345678901",
            ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship,
            [
                new("claire.morel@example.com", "Claire Morel", UserRole.QualityLead),
                new("hugo.lambert@example.com", "Hugo Lambert", UserRole.Member),
                new("sarah.benali@example.com", "Sarah Benali", UserRole.Member),
            ],
            [
                new(2, null, "Results are published on the website every January. Owner: Claire."),
                new(27, "We deliver every course with our own trainers and never subcontract.", null),
            ]),
        new(
            "Pathway Career Assessments",
            null,
            ActivityCategories.SkillsAssessments,
            [
                new("julien.faure@example.com", "Julien Faure", UserRole.QualityLead),
            ],
            [
                new(4, null, "Each assessment starts with a needs interview. Owner: Julien."),
            ]),
    ];

    /// <returns><c>false</c> when the database already holds users.</returns>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return false;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var passwordHash = passwordHasher.Hash(DemoPassword);

        foreach (var demo in Providers)
        {
            var provider = new Provider(demo.Name, demo.RegistrationNumber, demo.ActivityCategories, now);
            db.Providers.Add(provider);
            db.Users.AddRange(demo.Users.Select(
                u => new User(provider.Id, u.Email, u.DisplayName, passwordHash, u.Role, now)));

            foreach (var indicator in demo.Indicators)
            {
                var setting = new ProviderIndicator(provider.Id, indicator.Number, now);
                setting.Update(indicator.NotApplicableReason, indicator.Notes, now);
                db.ProviderIndicators.Add(setting);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public sealed record DemoProvider(
    string Name,
    string? RegistrationNumber,
    ActivityCategories ActivityCategories,
    IReadOnlyList<DemoUser> Users,
    IReadOnlyList<DemoIndicator> Indicators);

public sealed record DemoUser(string Email, string DisplayName, UserRole Role);

public sealed record DemoIndicator(int Number, string? NotApplicableReason, string? Notes);
