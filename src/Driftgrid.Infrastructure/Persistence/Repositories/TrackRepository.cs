
using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Common.Pagination;
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

    public Task<Track?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return context.Tracks
            .AsTracking()
            .FirstOrDefaultAsync(
                track => track.Id == id,
                cancellationToken);
    }

    public async Task<PagedResult<Track>> GetPagedAsync(
        TrackSearchOptions options,
        CancellationToken cancellationToken)
    {
        IQueryable<Track> query = context.Tracks
            .AsNoTracking();

        // Filtering
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var search = options.Search.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            var pattern = $"%{search}%";

            query = query.Where(track =>
                EF.Functions.ILike(track.Name, pattern, "\\") ||
                EF.Functions.ILike(track.Location, pattern, "\\"));
        }

        // Count before pagination
        var totalCount = await query.CountAsync(
            cancellationToken);

        // Whitelisted sorting with unique tie-breaker
        query = (options.SortBy, options.Descending) switch
        {
            ("location", true) => query
                .OrderByDescending(t => t.Location)
                .ThenBy(t => t.Id),

            ("location", false) => query
                .OrderBy(t => t.Location)
                .ThenBy(t => t.Id),

            ("name", true) => query
                .OrderByDescending(t => t.Name)
                .ThenBy(t => t.Id),

            _ => query
                .OrderBy(t => t.Name)
                .ThenBy(t => t.Id)
        };

        // Database-level pagination
        var offset = (options.Page - 1) * options.PageSize;

        var tracks = await query
            .Skip(offset)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Track>(
            tracks,
            options.Page,
            options.PageSize,
            totalCount);
    }

    public async Task AddAsync(
        Track track,
        CancellationToken cancellationToken)
    {
        await context.Tracks.AddAsync(
            track,
            cancellationToken);
    }

    public void Remove(Track track)
    {
        context.Tracks.Remove(track);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(
            cancellationToken);
    }
}
