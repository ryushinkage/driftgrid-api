using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Pilots;

public sealed record GetMyPilotProfileQuery(Guid UserId)
    : IRequest<PilotProfileDto?>;

public sealed class GetMyPilotProfileQueryHandler(
    IPilotProfileRepository repository)
    : IRequestHandler<
        GetMyPilotProfileQuery,
        PilotProfileDto?>
{
    public async Task<PilotProfileDto?> Handle(
        GetMyPilotProfileQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        return profile is null
            ? null
            : PilotProfileDto.FromEntity(profile);
    }
}