using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record DeleteTrackCommand(Guid Id)
    : IRequest<bool>;

public sealed class DeleteTrackCommandHandler(
    ITrackRepository repository)
    : IRequestHandler<DeleteTrackCommand, bool>
{
    public async Task<bool> Handle(
        DeleteTrackCommand request,
        CancellationToken cancellationToken)
    {
        var track = await repository.GetTrackedByIdAsync(
            request.Id,
            cancellationToken);

        if (track is null)
            return false;

        repository.Remove(track);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}