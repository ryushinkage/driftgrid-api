using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Features.Tracks;

public sealed record TrackDto(
    Guid Id,
    string Name,
    string Location)
{
    public static TrackDto FromEntity(Track track)
    {
        return new TrackDto(
            track.Id,
            track.Name,
            track.Location);
    }
}