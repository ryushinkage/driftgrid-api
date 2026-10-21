using System.ComponentModel.DataAnnotations;

namespace Driftgrid.Api.Contracts.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; init; } = string.Empty;
}