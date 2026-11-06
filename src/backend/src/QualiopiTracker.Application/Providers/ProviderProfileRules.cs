namespace QualiopiTracker.Application.Providers;

internal static class ProviderProfileRules
{
    /// <summary>Adds the errors of a provider profile, shared by sign-up and profile updates.</summary>
    /// <returns>The parsed categories, meaningful only when no error was added.</returns>
    public static ActivityCategories Check(
        Dictionary<string, string[]> errors,
        string nameField,
        string? name,
        string? registrationNumber,
        IReadOnlyList<string>? activityCategories)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors[nameField] = ["Provider name is required."];
        }
        else if (name.Trim().Length > Provider.NameMaxLength)
        {
            errors[nameField] = [$"Use at most {Provider.NameMaxLength} characters."];
        }

        if (!Provider.IsValidRegistrationNumber(registrationNumber))
        {
            errors["registrationNumber"] = ["Registration number must be 11 digits."];
        }

        var categories = ActivityCategoryNames.Parse(activityCategories);
        if (categories is null)
        {
            errors["activityCategories"] = ["Unknown activity category."];
        }
        else if (categories == ActivityCategories.None)
        {
            errors["activityCategories"] = ["Choose at least one activity category."];
        }

        return categories ?? ActivityCategories.None;
    }
}
