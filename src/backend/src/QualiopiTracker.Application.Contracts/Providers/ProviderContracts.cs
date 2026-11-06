namespace QualiopiTracker.Application.Contracts.Providers;

/// <param name="ActivityCategories">Names in a fixed order: TrainingCourses, SkillsAssessments, PriorExperienceAccreditation, Apprenticeship.</param>
public sealed record ProviderProfileResponse(
    Guid Id,
    string Name,
    string? RegistrationNumber,
    IReadOnlyList<string> ActivityCategories);

public sealed record UpdateProviderProfileRequest(
    string Name,
    string? RegistrationNumber,
    IReadOnlyList<string>? ActivityCategories);
