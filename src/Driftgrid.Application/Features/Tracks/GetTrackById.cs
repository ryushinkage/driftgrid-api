using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record GetTrackByIdQuery(Guid Id)
    : IRequest<TrackDto?>;

public sealed class GetTrackByIdQueryHandler(
    ITrackRepository repository)
    : IRequestHandler<GetTrackByIdQuery, TrackDto?>
{
    public async Task<TrackDto?> Handle(
        GetTrackByIdQuery request,
        CancellationToken cancellationToken)
    {
        var track = await repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        return track is null
            ? null
            : TrackDto.FromEntity(track);
    }
}