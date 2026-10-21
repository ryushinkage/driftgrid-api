
using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Common.Pagination;
using Driftgrid.Domain.Exceptions;
using MediatR;

namespace Driftgrid.Application.Features.Tracks;

public sealed record GetTracksQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string SortBy = "name",
    bool Descending = false
) : IRequest<PagedResult<TrackDto>>;

public sealed class GetTracksQueryHandler(
    ITrackRepository repository)
    : IRequestHandler<GetTracksQuery, PagedResult<TrackDto>>
{
    public async Task<PagedResult<TrackDto>> Handle(
        GetTracksQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new DomainValidationException(
                "Page must be greater than zero.");

        if (request.PageSize is < 1 or > 100)
            throw new DomainValidationException(
                "PageSize must be between 1 and 100.");

        if (string.IsNullOrWhiteSpace(request.SortBy))
            throw new DomainValidationException(
                "SortBy is required.");

        var sortBy = request.SortBy.Trim().ToLowerInvariant();

        if (sortBy is not ("name" or "location"))
            throw new DomainValidationException(
                "SortBy must be name or location.");

        var search = request.Search?.Trim();

        if (search?.Length > 100)
            throw new DomainValidationException(
                "Search cannot exceed 100 characters.");

        var offset = ((long)request.Page - 1)
                     * request.PageSize;

        if (offset > int.MaxValue)
            throw new DomainValidationException(
                "Requested page offset is too large.");

        var options = new TrackSearchOptions(
            request.Page,
            request.PageSize,
            search,
            sortBy,
            request.Descending);

        var result = await repository.GetPagedAsync(
            options,
            cancellationToken);

        return new PagedResult<TrackDto>(
            result.Items
                .Select(TrackDto.FromEntity)
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}
