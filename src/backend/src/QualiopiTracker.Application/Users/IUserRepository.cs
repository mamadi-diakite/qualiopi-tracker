namespace QualiopiTracker.Application.Users;

public interface IUserRepository
{
    /// <param name="email">Normalized email: trimmed and lowercase.</param>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Provider?> FindProviderAsync(Guid id, CancellationToken cancellationToken);

    void Add(User user);

    void AddProvider(Provider provider);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
