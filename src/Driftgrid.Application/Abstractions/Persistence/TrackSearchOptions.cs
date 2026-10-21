namespace Driftgrid.Application.Abstractions.Persistence;

public sealed record TrackSearchOptions(
    int Page,
    int PageSize,
    string? Search,
    string SortBy,
    bool Descending);