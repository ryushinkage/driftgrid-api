using Driftgrid.Api.Authorization;
using Driftgrid.Api.Contracts.Pilots;
using Driftgrid.Application.Features.Pilots;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;

namespace Driftgrid.Api.Controllers.V1;

[ApiController]
[Route("api/v1/pilots")]
[Authorize(Policy = AuthorizationPolicies.PilotOnly)]
public sealed class PilotsController(ISender sender)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<PilotProfileDto>> GetMe(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var UserId))
            return Unauthorized();

        var profile = await sender.Send(
            new GetMyPilotProfileQuery(UserId),
            cancellationToken);

        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Profile not found");
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<PilotProfileDto>> UpdateMe(
        [FromBody] UpdatePilotProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var UserId))
            return Unauthorized();

        var profile = await sender.Send(
            new UpdateMyPilotProfileCommand(
                UserId,
                request.Callsign,
                request.Bio),
            cancellationToken);

        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Pilot profile is not found");
        }

        return Ok(profile);
    }

    private bool TryGetCurrentUserId(out Guid userId)
        {
            return Guid.TryParse(
                       User.FindFirst("sub")?.Value,
                       out userId) &&
                   userId != Guid.Empty;
        }
    }