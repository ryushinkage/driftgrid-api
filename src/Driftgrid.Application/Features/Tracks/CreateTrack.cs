using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Domain.Entities;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record CreateTrackCommand(
    string Name,
    string Location) : IRequest<TrackDto>;

public sealed class CreateTrackCommandHandler(
    ITrackRepository repository)
    : IRequestHandler<CreateTrackCommand, TrackDto>
{
    public async Task<TrackDto> Handle(
        CreateTrackCommand request,
        CancellationToken cancellationToken)
    {
        var track = new Track(
            request.Name,
            request.Location);

        await repository.AddAsync(
            track,
            cancellationToken);

        await repository.SaveChangesAsync(
            cancellationToken);

        return TrackDto.FromEntity(track);
    }
}