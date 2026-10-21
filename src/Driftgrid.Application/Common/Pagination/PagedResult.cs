namespace Driftgrid.Application.Common.Pagination;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : 1 + (TotalCount - 1) / PageSize;
}