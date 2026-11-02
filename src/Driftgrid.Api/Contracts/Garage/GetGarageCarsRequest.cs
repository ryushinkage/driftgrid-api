using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Garage;

public sealed class GetGarageCarsRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}