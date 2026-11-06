namespace QualiopiTracker.Application.Providers;

public enum ProviderProfileStatus
{
    Ok = 0,
    UserNotFound = 1,
    NotQualityLead = 2,
    Invalid = 3,
}

/// <param name="Profile">Set when the status is <see cref="ProviderProfileStatus.Ok"/>.</param>
/// <param name="Errors">Messages by request field, in camelCase, when the status is <see cref="ProviderProfileStatus.Invalid"/>.</param>
public sealed record ProviderProfileResult(
    ProviderProfileStatus Status,
    ProviderProfileResponse? Profile,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ProviderProfileResult UserNotFound { get; } =
        new(ProviderProfileStatus.UserNotFound, null, new Dictionary<string, string[]>());

    public static ProviderProfileResult NotQualityLead { get; } =
        new(ProviderProfileStatus.NotQualityLead, null, new Dictionary<string, string[]>());

    public static ProviderProfileResult Ok(ProviderProfileResponse profile) =>
        new(ProviderProfileStatus.Ok, profile, new Dictionary<string, string[]>());

    public static ProviderProfileResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(ProviderProfileStatus.Invalid, null, errors);
}

public sealed class ProviderProfileService(IUserRepository users)
{
    public async Task<ProviderProfileResult> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var provider = await FindProviderOfAsync(userId, cancellationToken);

        return provider is null ? ProviderProfileResult.UserNotFound : ProviderProfileResult.Ok(ToResponse(provider));
    }

    /// <remarks>Only the quality lead edits the profile.</remarks>
    public async Task<ProviderProfileResult> UpdateAsync(
        Guid userId,
        UpdateProviderProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ProviderProfileResult.UserNotFound;
        }

        var provider = await users.FindProviderAsync(user.ProviderId, cancellationToken);
        if (provider is null)
        {
            return ProviderProfileResult.UserNotFound;
        }

        if (user.Role != UserRole.QualityLead)
        {
            return ProviderProfileResult.NotQualityLead;
        }

        var errors = new Dictionary<string, string[]>();
        var categories = ProviderProfileRules.Check(
            errors, "name", request.Name, request.RegistrationNumber, request.ActivityCategories);
        if (errors.Count > 0)
        {
            return ProviderProfileResult.Invalid(errors);
        }

        provider.UpdateProfile(request.Name, request.RegistrationNumber, categories);
        await users.SaveChangesAsync(cancellationToken);

        return ProviderProfileResult.Ok(ToResponse(provider));
    }

    private async Task<Provider?> FindProviderOfAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);

        return user is null ? null : await users.FindProviderAsync(user.ProviderId, cancellationToken);
    }

    private static ProviderProfileResponse ToResponse(Provider provider) =>
        new(provider.Id, provider.Name, provider.RegistrationNumber, ActivityCategoryNames.ToNames(provider.ActivityCategories));
}
