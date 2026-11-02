using Driftgrid.Application.Common.Pagination;
using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Abstractions.Persistence;

public interface IGarageCarRepository
{
    Task<PagedResult<GarageCar>> GetPagedByOwnerAsync(
        Guid ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<GarageCar?> GetByIdForOwnerAsync(
        Guid ownerId,
        Guid carId,
        CancellationToken cancellationToken);

    Task<GarageCar?> GetTrackedByIdForOwnerAsync(
        Guid ownerId,
        Guid carId,
        CancellationToken cancellationToken);

    Task AddAsync(
        GarageCar car,
        CancellationToken cancellationToken);

    void Remove(GarageCar car);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}