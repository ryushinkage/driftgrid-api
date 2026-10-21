namespace Driftgrid.Application.Features.Auth;

public enum RegisterPilotStatus
{
    Created,
    DuplicateEmail,
    InvalidInput
}

public sealed record RegisterPilotResult(
    RegisterPilotStatus Status,
    Guid? UserId = null,
    string? Email = null,
    IReadOnlyList<string>? Errors = null);