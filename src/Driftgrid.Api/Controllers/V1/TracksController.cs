using Driftgrid.Api.Contracts.Tracks;
using Driftgrid.Application.Features.Tracks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Driftgrid.Api.Controllers.V1;

[ApiController]
[Route("api/v1/tracks")]
public sealed class TracksController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tracks = await sender.Send(
            new GetTracksQuery(),
            cancellationToken);

        return Ok(tracks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var track = await sender.Send(
            new GetTrackByIdQuery(id),
            cancellationToken);

        if (track is null)
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Track not found");;

        return Ok(track);
    }

    [HttpPost]
    public async Task<ActionResult<TrackDto>> Create(
        [FromBody] CreateTrackRequest request,
        CancellationToken cancellationToken)
    {
        var track = await sender.Send(
            new CreateTrackCommand(
                request.Name,
                request.Location),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = track.Id },
            track);
    }
    
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTrackRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateTrackCommand(
                id,
                request.Name,
                request.Location),
            cancellationToken);

        if (!updated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Track not found");
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteTrackCommand(id),
            cancellationToken);

        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Track not found");
        }

        return NoContent();
    }

}