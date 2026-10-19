using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record GetTracksQuery()
    : IRequest<IReadOnlyList<TrackDto>>;

public sealed class GetTracksQueryHandler(
    ITrackRepository repository)
    : IRequestHandler<GetTracksQuery, IReadOnlyList<TrackDto>>
{
    public async Task<IReadOnlyList<TrackDto>> Handle(
        GetTracksQuery request,
        CancellationToken cancellationToken)
    {
        var tracks = await repository.GetAllAsync(
            cancellationToken);

        return tracks
            .Select(TrackDto.FromEntity)
            .ToList();
    }
}