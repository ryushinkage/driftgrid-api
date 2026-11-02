using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Abstractions.Persistence;

public interface IPilotProfileRepository
{
    Task<PilotProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<PilotProfile?> GetTrackedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}