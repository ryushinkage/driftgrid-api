using Driftgrid.Application.Common.Pagination;
using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Abstractions.Persistence;

public interface ITrackRepository
{
    Task<Track?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Track?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResult<Track>> GetPagedAsync(
        TrackSearchOptions options,
        CancellationToken cancellationToken);

    Task AddAsync(
        Track track,
        CancellationToken cancellationToken);

    void Remove(Track track);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}