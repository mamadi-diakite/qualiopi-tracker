namespace QualiopiTracker.Domain;

public sealed record Criterion(int Number, string Title);

/// <param name="AppliesTo">The activity categories this indicator is checked for.</param>
public sealed record Indicator(int Number, int CriterionNumber, string Title, ActivityCategories AppliesTo);

/// <summary>
/// The 7 criteria and 32 indicators of the national framework, with short titles written for this app.
/// The official texts are not reproduced.
/// </summary>
public static class QualityFramework
{
    private const ActivityCategories All = ActivityCategories.All;
    private const ActivityCategories Apprenticeship = ActivityCategories.Apprenticeship;

    public static IReadOnlyList<Criterion> Criteria { get; } =
    [
        new(1, "Information on the offer"),
        new(2, "Objectives and design"),
        new(3, "Learner support"),
        new(4, "Teaching resources"),
        new(5, "Staff skills"),
        new(6, "Professional environment"),
        new(7, "Feedback and improvement"),
    ];

    public static IReadOnlyList<Indicator> Indicators { get; } =
    [
        new(1, 1, "Offer details", All),
        new(2, 1, "Results figures", All),
        new(3, 1, "Certification rates", All),
        new(4, 2, "Needs analysis", All),
        new(5, 2, "Measurable objectives", All),
        new(6, 2, "Content and methods", All),
        new(7, 2, "Fit with the certification", All),
        new(8, 2, "Entry level check", All),
        new(9, 3, "Practical information", All),
        new(10, 3, "Tailored delivery", All),
        new(11, 3, "Progress checks", All),
        new(12, 3, "Engagement and dropout", All),
        new(13, 3, "Work-study coordination", All),
        new(14, 3, "Social and career support", Apprenticeship),
        new(15, 3, "Apprentice rights", Apprenticeship),
        new(16, 3, "Exam arrangements", All),
        new(17, 4, "Premises and equipment", All),
        new(18, 4, "Team coordination", All),
        new(19, 4, "Learning materials", All),
        new(20, 4, "Dedicated referents", Apprenticeship),
        new(21, 5, "Staff skills", All),
        new(22, 5, "Skills development", All),
        new(23, 6, "Regulatory watch", All),
        new(24, 6, "Jobs and skills watch", All),
        new(25, 6, "Teaching innovation watch", All),
        new(26, 6, "Disability inclusion", All),
        new(27, 6, "Subcontracting", All),
        new(28, 6, "Workplace partners", All),
        new(29, 6, "Further study and jobs", Apprenticeship),
        new(30, 7, "Stakeholder feedback", All),
        new(31, 7, "Complaints and incidents", All),
        new(32, 7, "Improvement actions", All),
    ];

    public static Indicator? FindIndicator(int number) => Indicators.SingleOrDefault(i => i.Number == number);
}
