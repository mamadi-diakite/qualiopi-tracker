namespace QualiopiTracker.Domain;

/// <summary>The training organization that prepares its certification.</summary>
public sealed class Provider
{
    public const int NameMaxLength = 150;
    public const int RegistrationNumberLength = 11;

    private Provider()
    {
    }

    public Provider(string name, string? registrationNumber, ActivityCategories activityCategories, DateTime createdAtUtc)
    {
        Id = Guid.CreateVersion7();
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        UpdateProfile(name, registrationNumber, activityCategories);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The 11-digit activity registration number, when the provider has one.</summary>
    public string? RegistrationNumber { get; private set; }

    public ActivityCategories ActivityCategories { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <returns><c>true</c> for a blank value or 11 digits.</returns>
    public static bool IsValidRegistrationNumber(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || value.Trim() is { Length: RegistrationNumberLength } number && number.All(char.IsAsciiDigit);

    /// <returns><c>true</c> when at least one known category is set and nothing else.</returns>
    public static bool IsValidActivityCategories(ActivityCategories value) =>
        value != ActivityCategories.None && (value & ~ActivityCategories.All) == 0;

    public void UpdateProfile(string name, string? registrationNumber, ActivityCategories activityCategories)
    {
        var newName = Guard.Required(name, nameof(name), NameMaxLength);
        if (!IsValidRegistrationNumber(registrationNumber))
        {
            throw new ArgumentException("Registration number must be 11 digits.", nameof(registrationNumber));
        }

        if (!IsValidActivityCategories(activityCategories))
        {
            throw new ArgumentOutOfRangeException(
                nameof(activityCategories), activityCategories, "Choose at least one known activity category.");
        }

        Name = newName;
        RegistrationNumber = Guard.Optional(registrationNumber, nameof(registrationNumber), RegistrationNumberLength);
        ActivityCategories = activityCategories;
    }
}
