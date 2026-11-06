namespace QualiopiTracker.Domain;

public enum IndicatorApplicability
{
    Applicable = 0,

    /// <summary>In scope, but the quality lead marked it as not applicable with a reason.</summary>
    NotApplicable = 1,

    /// <summary>The indicator is not checked for any of the provider's activity categories.</summary>
    OutOfScope = 2,
}

public static class Applicability
{
    public static bool IsInScope(Indicator indicator, ActivityCategories providerCategories)
    {
        ArgumentNullException.ThrowIfNull(indicator);

        return (indicator.AppliesTo & providerCategories) != ActivityCategories.None;
    }

    /// <param name="notApplicableReason">The reason given by the quality lead, or <c>null</c>.</param>
    public static IndicatorApplicability Of(
        Indicator indicator,
        ActivityCategories providerCategories,
        string? notApplicableReason)
    {
        if (!IsInScope(indicator, providerCategories))
        {
            return IndicatorApplicability.OutOfScope;
        }

        return string.IsNullOrWhiteSpace(notApplicableReason)
            ? IndicatorApplicability.Applicable
            : IndicatorApplicability.NotApplicable;
    }
}
