
using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Common.Pagination;
using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Driftgrid.Infrastructure.Persistence.Repositories;

public sealed class GarageCarRepository(
    DriftgridDbContext context)
    : IGarageCarRepository
{
    public async Task<PagedResult<GarageCar>> GetPagedByOwnerAsync(
        Guid ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.GarageCars
            .AsNoTracking()
            .Where(car => car.OwnerId == ownerId);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var cars = await query
            .OrderByDescending(car => car.CreatedAtUtc)
            .ThenBy(car => car.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GarageCar>(
            cars,
            page,
            pageSize,
            totalCount);
    }

    public Task<GarageCar?> GetByIdForOwnerAsync(
        Guid ownerId,
        Guid carId,
        CancellationToken cancellationToken)
    {
        return context.GarageCars
            .AsNoTracking()
            .FirstOrDefaultAsync(
                car =>
                    car.Id == carId &&
                    car.OwnerId == ownerId,
                cancellationToken);
    }

    public Task<GarageCar?> GetTrackedByIdForOwnerAsync(
        Guid ownerId,
        Guid carId,
        CancellationToken cancellationToken)
    {
        return context.GarageCars
            .AsTracking()
            .FirstOrDefaultAsync(
                car =>
                    car.Id == carId &&
                    car.OwnerId == ownerId,
                cancellationToken);
    }

    public async Task AddAsync(
        GarageCar car,
        CancellationToken cancellationToken)
    {
        await context.GarageCars.AddAsync(
            car,
            cancellationToken);
    }

    public void Remove(GarageCar car)
    {
        context.GarageCars.Remove(car);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
