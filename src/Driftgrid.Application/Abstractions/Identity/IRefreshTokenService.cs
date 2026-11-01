using Driftgrid.Application.Features.Auth;

namespace Driftgrid.Application.Abstractions.Identity;

public interface IRefreshTokenService
{
    Task<RefreshTokenIssue> IssueAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<LoginResult?> RotateAsync(
        string token,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        string token,
        CancellationToken cancellationToken);
}