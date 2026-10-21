using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record UpdateTrackCommand(
    Guid Id,
    string Name,
    string Location) : IRequest<bool>;

public sealed class UpdateTrackCommandHandler(
    ITrackRepository repository)
    : IRequestHandler<UpdateTrackCommand, bool>
{
    public async Task<bool> Handle(
        UpdateTrackCommand request,
        CancellationToken cancellationToken)
    {
        var track = await repository.GetTrackedByIdAsync(
            request.Id,
            cancellationToken);

        if (track is null)
            return false;

        track.UpdateDetails(
            request.Name,
            request.Location);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}