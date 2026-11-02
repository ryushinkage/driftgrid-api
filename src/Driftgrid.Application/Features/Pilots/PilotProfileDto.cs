using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Features.Pilots;

public sealed record PilotProfileDto(
    Guid UserId,
    string Callsign,
    string? Bio,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static PilotProfileDto FromEntity(
        PilotProfile profile)
    {
        return new PilotProfileDto(
            profile.UserId,
            profile.Callsign,
            profile.Bio,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);
    }
}