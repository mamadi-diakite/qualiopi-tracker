namespace QualiopiTracker.Application.Framework;

public interface IProviderIndicatorRepository
{
    Task<IReadOnlyList<ProviderIndicator>> ListAsync(Guid providerId, CancellationToken cancellationToken);

    Task<ProviderIndicator?> FindAsync(Guid providerId, int indicatorNumber, CancellationToken cancellationToken);

    void Add(ProviderIndicator providerIndicator);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
