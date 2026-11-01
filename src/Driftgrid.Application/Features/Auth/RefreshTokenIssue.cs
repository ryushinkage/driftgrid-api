namespace Driftgrid.Application.Features.Auth;

public sealed record RefreshTokenIssue(
    string Token,
    DateTimeOffset ExpiresAtUtc);