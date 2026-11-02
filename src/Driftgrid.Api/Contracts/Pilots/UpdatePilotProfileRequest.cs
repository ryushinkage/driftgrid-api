using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Pilots;

public sealed class UpdatePilotProfileRequest
{
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Callsign { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; init; }
}