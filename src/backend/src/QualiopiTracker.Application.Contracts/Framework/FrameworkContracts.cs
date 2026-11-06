namespace QualiopiTracker.Application.Contracts.Framework;

public sealed record CriterionResponse(int Number, string Title, IReadOnlyList<IndicatorResponse> Indicators);

/// <param name="AppliesTo">Activity category names the indicator is checked for.</param>
/// <param name="Status">"Applicable", "NotApplicable" (marked by the quality lead) or "OutOfScope" (not checked for the provider's activities).</param>
public sealed record IndicatorResponse(
    int Number,
    int CriterionNumber,
    string Title,
    IReadOnlyList<string> AppliesTo,
    string Status,
    string? NotApplicableReason,
    string? Notes);

/// <param name="NotApplicableReason">A reason marks the indicator as not applicable; blank makes it applicable again.</param>
public sealed record UpdateIndicatorRequest(string? NotApplicableReason, string? Notes);
