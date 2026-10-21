using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Tracks;

public sealed class UpdateTrackRequest
{
    [Required]
    [StringLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; init; } = string.Empty;
}