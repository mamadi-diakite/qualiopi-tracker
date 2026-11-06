namespace QualiopiTracker.Infrastructure.Persistence.Configurations;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(Provider.NameMaxLength).IsRequired();
        builder.Property(p => p.RegistrationNumber).HasMaxLength(Provider.RegistrationNumberLength).IsFixedLength();
        builder.Property(p => p.ActivityCategories).IsRequired();
    }
}
