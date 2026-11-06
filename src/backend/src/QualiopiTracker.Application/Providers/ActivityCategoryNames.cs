namespace QualiopiTracker.Application.Providers;

/// <summary>Maps activity categories to the names used by the API.</summary>
public static class ActivityCategoryNames
{
    private static readonly ActivityCategories[] Known =
    [
        ActivityCategories.TrainingCourses,
        ActivityCategories.SkillsAssessments,
        ActivityCategories.PriorExperienceAccreditation,
        ActivityCategories.Apprenticeship,
    ];

    public static IReadOnlyList<string> ToNames(ActivityCategories categories) =>
        [.. Known.Where(c => categories.HasFlag(c)).Select(c => c.ToString())];

    /// <returns><c>null</c> when a name is unknown; <see cref="ActivityCategories.None"/> for no names.</returns>
    public static ActivityCategories? Parse(IReadOnlyList<string>? names)
    {
        var categories = ActivityCategories.None;
        foreach (var name in names ?? [])
        {
            var category = Known.FirstOrDefault(c => c.ToString() == name);
            if (category == ActivityCategories.None)
            {
                return null;
            }

            categories |= category;
        }

        return categories;
    }
}
