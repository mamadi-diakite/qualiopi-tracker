namespace QualiopiTracker.Application.Framework;

public sealed class FrameworkService(
    IUserRepository users,
    IProviderIndicatorRepository providerIndicators,
    TimeProvider timeProvider)
{
    /// <summary>Every criterion with all its indicators, each with its applicability for the user's provider.</summary>
    public async Task<FrameworkResult<IReadOnlyList<CriterionResponse>>> GetFrameworkAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var (_, provider) = await FindUserAndProviderAsync(userId, cancellationToken);
        if (provider is null)
        {
            return FrameworkResult<IReadOnlyList<CriterionResponse>>.Failed(FrameworkStatus.UserNotFound);
        }

        var settings = (await providerIndicators.ListAsync(provider.Id, cancellationToken))
            .ToDictionary(s => s.IndicatorNumber);
        IReadOnlyList<CriterionResponse> criteria =
        [
            .. QualityFramework.Criteria.Select(criterion => new CriterionResponse(
                criterion.Number,
                criterion.Title,
                [
                    .. QualityFramework.Indicators
                        .Where(i => i.CriterionNumber == criterion.Number)
                        .Select(i => ToResponse(i, provider.ActivityCategories, settings.GetValueOrDefault(i.Number))),
                ])),
        ];

        return FrameworkResult<IReadOnlyList<CriterionResponse>>.Ok(criteria);
    }

    public async Task<FrameworkResult<IndicatorResponse>> GetIndicatorAsync(
        Guid userId,
        int indicatorNumber,
        CancellationToken cancellationToken = default)
    {
        var (_, provider) = await FindUserAndProviderAsync(userId, cancellationToken);
        if (provider is null)
        {
            return FrameworkResult<IndicatorResponse>.Failed(FrameworkStatus.UserNotFound);
        }

        if (QualityFramework.FindIndicator(indicatorNumber) is not { } indicator)
        {
            return FrameworkResult<IndicatorResponse>.Failed(FrameworkStatus.IndicatorNotFound);
        }

        var setting = await providerIndicators.FindAsync(provider.Id, indicatorNumber, cancellationToken);

        return FrameworkResult<IndicatorResponse>.Ok(ToResponse(indicator, provider.ActivityCategories, setting));
    }

    /// <remarks>Only the quality lead marks indicators as not applicable and writes notes.</remarks>
    public async Task<FrameworkResult<IndicatorResponse>> UpdateIndicatorAsync(
        Guid userId,
        int indicatorNumber,
        UpdateIndicatorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (user, provider) = await FindUserAndProviderAsync(userId, cancellationToken);
        if (user is null || provider is null)
        {
            return FrameworkResult<IndicatorResponse>.Failed(FrameworkStatus.UserNotFound);
        }

        if (QualityFramework.FindIndicator(indicatorNumber) is not { } indicator)
        {
            return FrameworkResult<IndicatorResponse>.Failed(FrameworkStatus.IndicatorNotFound);
        }

        if (user.Role != UserRole.QualityLead)
        {
            return FrameworkResult<IndicatorResponse>.Failed(FrameworkStatus.NotQualityLead);
        }

        var errors = new Dictionary<string, string[]>();
        if (!string.IsNullOrWhiteSpace(request.NotApplicableReason))
        {
            if (request.NotApplicableReason.Trim().Length > ProviderIndicator.NotApplicableReasonMaxLength)
            {
                errors["notApplicableReason"] = [$"Use at most {ProviderIndicator.NotApplicableReasonMaxLength} characters."];
            }
            else if (!Applicability.IsInScope(indicator, provider.ActivityCategories))
            {
                errors["notApplicableReason"] = ["This indicator is outside your activity categories, so it already does not apply."];
            }
        }

        if (request.Notes?.Trim().Length > ProviderIndicator.NotesMaxLength)
        {
            errors["notes"] = [$"Use at most {ProviderIndicator.NotesMaxLength} characters."];
        }

        if (errors.Count > 0)
        {
            return FrameworkResult<IndicatorResponse>.Invalid(errors);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var setting = await providerIndicators.FindAsync(provider.Id, indicatorNumber, cancellationToken);
        if (setting is null)
        {
            setting = new ProviderIndicator(provider.Id, indicatorNumber, now);
            providerIndicators.Add(setting);
        }

        setting.Update(request.NotApplicableReason, request.Notes, now);
        await providerIndicators.SaveChangesAsync(cancellationToken);

        return FrameworkResult<IndicatorResponse>.Ok(ToResponse(indicator, provider.ActivityCategories, setting));
    }

    private async Task<(User? User, Provider? Provider)> FindUserAndProviderAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);

        return user is null ? (null, null) : (user, await users.FindProviderAsync(user.ProviderId, cancellationToken));
    }

    private static IndicatorResponse ToResponse(
        Indicator indicator,
        ActivityCategories providerCategories,
        ProviderIndicator? setting) =>
        new(
            indicator.Number,
            indicator.CriterionNumber,
            indicator.Title,
            ActivityCategoryNames.ToNames(indicator.AppliesTo),
            Applicability.Of(indicator, providerCategories, setting?.NotApplicableReason).ToString(),
            setting?.NotApplicableReason,
            setting?.Notes);
}
