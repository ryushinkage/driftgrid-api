using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    [RegularExpression(
        "^[0-9A-Fa-f]{64}$",
        ErrorMessage = "Invalid refresh token format.")]
    public string RefreshToken { get; init; } = string.Empty;
}