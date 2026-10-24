namespace Driftgrid.Application.Features.Auth;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);