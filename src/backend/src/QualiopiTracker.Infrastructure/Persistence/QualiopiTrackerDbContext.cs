namespace QualiopiTracker.Infrastructure.Persistence;

public sealed class QualiopiTrackerDbContext(DbContextOptions<QualiopiTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<User> Users => Set<User>();

    public DbSet<ProviderIndicator> ProviderIndicators => Set<ProviderIndicator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QualiopiTrackerDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
}
