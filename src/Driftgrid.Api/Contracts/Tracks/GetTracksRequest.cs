using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Tracks;

public sealed class GetTracksRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [StringLength(100)]
    public string? Search { get; init; }

    [Required]
    [RegularExpression(
        "(?i)^(name|location)$",
        ErrorMessage = "SortBy must be name or location.")]
    public string SortBy { get; init; } = "name";

    public bool Descending { get; init; } = false;
}