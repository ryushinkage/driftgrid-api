using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Driftgrid.Infrastructure.Persistence.Repositories;

public sealed class TrackRepository(
    DriftgridDbContext context) : ITrackRepository
{
    public Task<Track?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return context.Tracks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                track => track.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.Tracks
            .AsNoTracking()
            .OrderBy(track => track.Name)
            .ThenBy(track => track.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Track track,
        CancellationToken cancellationToken)
    {
        await context.Tracks.AddAsync(
            track,
            cancellationToken);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(
            cancellationToken);
    }
}