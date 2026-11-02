using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Pilots;

public sealed record UpdateMyPilotProfileCommand(
    Guid UserId,
    string Callsign,
    string? Bio)
    : IRequest<PilotProfileDto?>;

public sealed class UpdateMyPilotProfileCommandHandler(
    IPilotProfileRepository repository)
    : IRequestHandler<
        UpdateMyPilotProfileCommand,
        PilotProfileDto?>
{
    public async Task<PilotProfileDto?> Handle(
        UpdateMyPilotProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetTrackedByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (profile is null)
            return null;

        profile.UpdateDetails(
            request.Callsign,
            request.Bio);

        await repository.SaveChangesAsync(
            cancellationToken);

        return PilotProfileDto.FromEntity(profile);
    }
}