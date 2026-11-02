using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Garage;

public sealed class UpsertGarageCarRequest
{
    [Required]
    [StringLength(60, MinimumLength = 1)]
    public string Make { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Model { get; init; } = string.Empty;

    [Range(1950, 2100)]
    public int Year { get; init; }

    [Range(1, 3000)]
    public int Horsepower { get; init; }

    [StringLength(1000)]
    public string? Notes { get; init; }
}