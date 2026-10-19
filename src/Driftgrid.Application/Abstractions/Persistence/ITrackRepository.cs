using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Abstractions.Persistence;

public interface ITrackRepository
{
    Task<Track?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Track>> GetAllAsync(
        CancellationToken cancellationToken);

    Task AddAsync(
        Track track,
        CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}