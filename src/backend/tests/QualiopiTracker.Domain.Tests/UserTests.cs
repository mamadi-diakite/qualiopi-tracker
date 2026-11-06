namespace QualiopiTracker.Domain.Tests;

public sealed class UserTests
{
    private static readonly Guid ProviderId = Guid.NewGuid();

    [Fact]
    public void Creates_a_user_in_a_provider()
    {
        var user = new User(ProviderId, "  Claire.Morel@Example.com ", " Claire Morel ", "hash", UserRole.QualityLead, TestDates.Now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(ProviderId, user.ProviderId);
        Assert.Equal("claire.morel@example.com", user.Email);
        Assert.Equal("Claire Morel", user.DisplayName);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(UserRole.QualityLead, user.Role);
        Assert.Equal(TestDates.Now, user.CreatedAtUtc);
    }

    [Fact]
    public void Rejects_invalid_values()
    {
        Assert.Equal("providerId", Assert.Throws<ArgumentException>(
            () => new User(Guid.Empty, "a@example.com", "Claire", "hash", UserRole.Member, TestDates.Now)).ParamName);
        Assert.Equal("email", Assert.Throws<ArgumentException>(
            () => new User(ProviderId, "claire@", "Claire", "hash", UserRole.Member, TestDates.Now)).ParamName);
        Assert.Equal("displayName", Assert.Throws<ArgumentException>(
            () => new User(ProviderId, "a@example.com", " ", "hash", UserRole.Member, TestDates.Now)).ParamName);
        Assert.Equal("passwordHash", Assert.Throws<ArgumentException>(
            () => new User(ProviderId, "a@example.com", "Claire", "", UserRole.Member, TestDates.Now)).ParamName);
        Assert.Equal("role", Assert.Throws<ArgumentOutOfRangeException>(
            () => new User(ProviderId, "a@example.com", "Claire", "hash", (UserRole)9, TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new User(ProviderId, "a@example.com", "Claire", "hash", UserRole.Member,
                DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }
}
