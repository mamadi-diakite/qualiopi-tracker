namespace QualiopiTracker.Domain;

/// <summary>What a provider says about one indicator: why it does not apply, and its own notes.</summary>
public sealed class ProviderIndicator
{
    public const int NotApplicableReasonMaxLength = 500;
    public const int NotesMaxLength = 2000;

    private ProviderIndicator()
    {
    }

    public ProviderIndicator(Guid providerId, int indicatorNumber, DateTime createdAtUtc)
    {
        ProviderId = Guard.NotEmpty(providerId, nameof(providerId));
        IndicatorNumber = QualityFramework.FindIndicator(indicatorNumber) is not null
            ? indicatorNumber
            : throw new ArgumentOutOfRangeException(nameof(indicatorNumber), indicatorNumber, "Unknown indicator.");
        UpdatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid ProviderId { get; private set; }

    public int IndicatorNumber { get; private set; }

    /// <summary>Set when the quality lead marks an in-scope indicator as not applicable.</summary>
    public string? NotApplicableReason { get; private set; }

    public string? Notes { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <param name="notApplicableReason">A blank reason makes the indicator applicable again.</param>
    public void Update(string? notApplicableReason, string? notes, DateTime updatedAtUtc)
    {
        var reason = Guard.Optional(notApplicableReason, nameof(notApplicableReason), NotApplicableReasonMaxLength);
        var newNotes = Guard.Optional(notes, nameof(notes), NotesMaxLength);
        var updatedAt = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));

        NotApplicableReason = reason;
        Notes = newNotes;
        UpdatedAtUtc = updatedAt;
    }
}
