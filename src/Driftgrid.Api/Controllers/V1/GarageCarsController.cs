
using Driftgrid.Api.Authorization;
using Driftgrid.Api.Contracts.Garage;
using Driftgrid.Application.Common.Pagination;
using Driftgrid.Application.Features.Garage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Driftgrid.Api.Controllers.V1;

[ApiController]
[Route("api/v1/garage/cars")]
[Authorize(Policy = AuthorizationPolicies.PilotOnly)]
public sealed class GarageCarsController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GarageCarDto>>> GetAll(
        [FromQuery] GetGarageCarsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var ownerId))
            return Unauthorized();

        var result = await sender.Send(
            new GetMyGarageCarsQuery(
                ownerId,
                request.Page,
                request.PageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GarageCarDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var ownerId))
            return Unauthorized();

        var car = await sender.Send(
            new GetMyGarageCarByIdQuery(ownerId, id),
            cancellationToken);

        if (car is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Car not found");
        }

        return Ok(car);
    }

    [HttpPost]
    public async Task<ActionResult<GarageCarDto>> Create(
        [FromBody] UpsertGarageCarRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var ownerId))
            return Unauthorized();

        var car = await sender.Send(
            new CreateGarageCarCommand(
                ownerId,
                request.Make,
                request.Model,
                request.Year,
                request.Horsepower,
                request.Notes),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = car.Id },
            car);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpsertGarageCarRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var ownerId))
            return Unauthorized();

        var updated = await sender.Send(
            new UpdateGarageCarCommand(
                ownerId,
                id,
                request.Make,
                request.Model,
                request.Year,
                request.Horsepower,
                request.Notes),
            cancellationToken);

        if (!updated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Car not found");
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var ownerId))
            return Unauthorized();

        var deleted = await sender.Send(
            new DeleteGarageCarCommand(ownerId, id),
            cancellationToken);

        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Car not found");
        }

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirst("sub")?.Value,
            out userId) &&
            userId != Guid.Empty;
    }
}
