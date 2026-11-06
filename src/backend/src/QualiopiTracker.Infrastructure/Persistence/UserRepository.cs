namespace QualiopiTracker.Infrastructure.Persistence;

public sealed class UserRepository(QualiopiTrackerDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Provider?> FindProviderAsync(Guid id, CancellationToken cancellationToken) =>
        db.Providers.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(User user) => db.Users.Add(user);

    public void AddProvider(Provider provider) => db.Providers.Add(provider);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
