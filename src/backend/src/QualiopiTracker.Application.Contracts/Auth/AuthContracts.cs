namespace QualiopiTracker.Application.Contracts.Auth;

/// <summary>Sign-up creates a provider together with its first quality lead.</summary>
/// <param name="ActivityCategories">Names such as "TrainingCourses" or "Apprenticeship".</param>
public sealed record SignUpRequest(
    string ProviderName,
    string? RegistrationNumber,
    IReadOnlyList<string>? ActivityCategories,
    string DisplayName,
    string Email,
    string Password,
    string ConfirmPassword);

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, CurrentUserResponse User);

/// <param name="Role">"QualityLead" or "Member".</param>
public sealed record CurrentUserResponse(Guid Id, string Email, string DisplayName, string Role, Guid ProviderId);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
