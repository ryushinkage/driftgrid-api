namespace Driftgrid.Application.Features.Auth;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);