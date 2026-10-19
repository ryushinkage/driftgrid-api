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
            return NotFound();

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
}