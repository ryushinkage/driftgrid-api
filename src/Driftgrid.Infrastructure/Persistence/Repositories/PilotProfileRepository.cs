using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Driftgrid.Infrastructure.Persistence.Repositories;

public sealed class PilotProfileRepository(
    DriftgridDbContext context)
    : IPilotProfileRepository
{
    public Task<PilotProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return context.PilotProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public Task<PilotProfile?> GetTrackedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return context.PilotProfiles
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}