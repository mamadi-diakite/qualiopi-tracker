namespace QualiopiTracker.Api.Controllers;

/// <summary>The criteria and indicators, with their applicability for the user's provider.</summary>
[ApiController]
[Authorize]
[Route("api/framework")]
public sealed class FrameworkController(FrameworkService frameworkService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CriterionResponse>>> Get(CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await frameworkService.GetFrameworkAsync(userId, cancellationToken))
            : Unauthorized();

    [HttpGet("indicators/{number:int}")]
    public async Task<ActionResult<IndicatorResponse>> GetIndicator(int number, CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await frameworkService.GetIndicatorAsync(userId, number, cancellationToken))
            : Unauthorized();

    [HttpPut("indicators/{number:int}")]
    public async Task<ActionResult<IndicatorResponse>> UpdateIndicator(
        int number,
        UpdateIndicatorRequest request,
        CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await frameworkService.UpdateIndicatorAsync(userId, number, request, cancellationToken))
            : Unauthorized();

    private ActionResult<T> ToActionResult<T>(FrameworkResult<T> result)
        where T : class => result.Status switch
    {
        FrameworkStatus.Ok => Ok(result.Value),
        FrameworkStatus.UserNotFound => Unauthorized(),
        FrameworkStatus.IndicatorNotFound => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "This indicator does not exist."),
        FrameworkStatus.NotQualityLead => Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Only the quality lead can change indicators."),
        _ => this.InvalidRequest(result.Errors),
    };
}
