namespace QualiopiTracker.Domain.Tests;

public sealed class ApplicabilityTests
{
    private static readonly Indicator ForEveryone = new(1, 1, "Offer details", ActivityCategories.All);
    private static readonly Indicator ForApprenticeship = new(14, 3, "Social and career support", ActivityCategories.Apprenticeship);

    [Theory]
    [InlineData(ActivityCategories.TrainingCourses, false)]
    [InlineData(ActivityCategories.SkillsAssessments | ActivityCategories.PriorExperienceAccreditation, false)]
    [InlineData(ActivityCategories.Apprenticeship, true)]
    [InlineData(ActivityCategories.TrainingCourses | ActivityCategories.Apprenticeship, true)]
    [InlineData(ActivityCategories.None, false)]
    public void An_indicator_is_in_scope_when_one_category_matches(ActivityCategories categories, bool expected)
    {
        Assert.Equal(expected, Applicability.IsInScope(ForApprenticeship, categories));
    }

    [Fact]
    public void An_indicator_for_every_category_is_in_scope_for_any_provider()
    {
        Assert.True(Applicability.IsInScope(ForEveryone, ActivityCategories.SkillsAssessments));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void In_scope_without_a_reason_is_applicable(string? reason)
    {
        Assert.Equal(IndicatorApplicability.Applicable, Applicability.Of(ForEveryone, ActivityCategories.TrainingCourses, reason));
    }

    [Fact]
    public void In_scope_with_a_reason_is_not_applicable()
    {
        Assert.Equal(
            IndicatorApplicability.NotApplicable,
            Applicability.Of(ForEveryone, ActivityCategories.TrainingCourses, "We never subcontract."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("A reason kept from an earlier profile.")]
    public void Out_of_scope_wins_over_any_reason(string? reason)
    {
        Assert.Equal(
            IndicatorApplicability.OutOfScope,
            Applicability.Of(ForApprenticeship, ActivityCategories.TrainingCourses, reason));
    }

    [Fact]
    public void Rejects_a_missing_indicator()
    {
        Assert.Throws<ArgumentNullException>(() => Applicability.IsInScope(null!, ActivityCategories.All));
    }
}
