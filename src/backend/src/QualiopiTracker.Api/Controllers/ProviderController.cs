namespace QualiopiTracker.Api.Controllers;

/// <summary>The profile of the provider the user belongs to.</summary>
[ApiController]
[Authorize]
[Route("api/provider")]
public sealed class ProviderController(ProviderProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProviderProfileResponse>> Get(CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await profileService.GetAsync(userId, cancellationToken))
            : Unauthorized();

    [HttpPut]
    public async Task<ActionResult<ProviderProfileResponse>> Update(
        UpdateProviderProfileRequest request,
        CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await profileService.UpdateAsync(userId, request, cancellationToken))
            : Unauthorized();

    private ActionResult<ProviderProfileResponse> ToActionResult(ProviderProfileResult result) => result.Status switch
    {
        ProviderProfileStatus.Ok => Ok(result.Profile),
        ProviderProfileStatus.UserNotFound => Unauthorized(),
        ProviderProfileStatus.NotQualityLead => Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Only the quality lead can edit the provider profile."),
        _ => this.InvalidRequest(result.Errors),
    };
}
