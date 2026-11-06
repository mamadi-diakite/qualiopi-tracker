namespace QualiopiTracker.Application.Framework;

public enum FrameworkStatus
{
    Ok = 0,
    UserNotFound = 1,
    IndicatorNotFound = 2,
    NotQualityLead = 3,
    Invalid = 4,
}

/// <param name="Value">Set when the status is <see cref="FrameworkStatus.Ok"/>.</param>
/// <param name="Errors">Messages by request field, in camelCase, when the status is <see cref="FrameworkStatus.Invalid"/>.</param>
public sealed record FrameworkResult<T>(FrameworkStatus Status, T? Value, IReadOnlyDictionary<string, string[]> Errors)
    where T : class
{
    public static FrameworkResult<T> Ok(T value) => new(FrameworkStatus.Ok, value, new Dictionary<string, string[]>());

    public static FrameworkResult<T> Failed(FrameworkStatus status) => new(status, null, new Dictionary<string, string[]>());

    public static FrameworkResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(FrameworkStatus.Invalid, null, errors);
}
