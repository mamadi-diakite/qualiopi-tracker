namespace QualiopiTracker.Infrastructure.Persistence;

public sealed class ProviderIndicatorRepository(QualiopiTrackerDbContext db) : IProviderIndicatorRepository
{
    public async Task<IReadOnlyList<ProviderIndicator>> ListAsync(Guid providerId, CancellationToken cancellationToken) =>
        await db.ProviderIndicators.Where(p => p.ProviderId == providerId).ToListAsync(cancellationToken);

    public Task<ProviderIndicator?> FindAsync(Guid providerId, int indicatorNumber, CancellationToken cancellationToken) =>
        db.ProviderIndicators.SingleOrDefaultAsync(
            p => p.ProviderId == providerId && p.IndicatorNumber == indicatorNumber, cancellationToken);

    public void Add(ProviderIndicator providerIndicator) => db.ProviderIndicators.Add(providerIndicator);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
