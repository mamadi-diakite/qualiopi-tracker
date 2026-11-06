namespace QualiopiTracker.Infrastructure.Persistence.Configurations;

internal sealed class ProviderIndicatorConfiguration : IEntityTypeConfiguration<ProviderIndicator>
{
    public void Configure(EntityTypeBuilder<ProviderIndicator> builder)
    {
        builder.ToTable("ProviderIndicators");
        builder.HasKey(p => new { p.ProviderId, p.IndicatorNumber });

        builder.Property(p => p.NotApplicableReason).HasMaxLength(ProviderIndicator.NotApplicableReasonMaxLength);
        builder.Property(p => p.Notes).HasMaxLength(ProviderIndicator.NotesMaxLength);

        builder.HasOne<Provider>()
            .WithMany()
            .HasForeignKey(p => p.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
