
using System.Security.Cryptography;
using System.Text;
using Driftgrid.Application.Abstractions.Identity;
using Driftgrid.Application.Features.Auth;
using Driftgrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Driftgrid.Infrastructure.Identity;

public sealed class EfRefreshTokenService(
    DriftgridDbContext db,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtGenerator)
    : IRefreshTokenService
{
    private static readonly TimeSpan TokenLifetime =
        TimeSpan.FromDays(7);

    private static readonly TimeSpan SessionLifetime =
        TimeSpan.FromDays(30);

    public async Task<RefreshTokenIssue> IssueAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
            throw new InvalidOperationException(
                "Cannot create session for missing user.");

        var now = DateTimeOffset.UtcNow;
        var rawToken = GenerateToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            SecurityStamp = user.SecurityStamp ?? string.Empty,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(TokenLifetime),
            FamilyExpiresAtUtc = now.Add(SessionLifetime)
        });

        await db.SaveChangesAsync(cancellationToken);

        return new RefreshTokenIssue(
            rawToken,
            now.Add(TokenLifetime));
    }

    public async Task<LoginResult?> RotateAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        if (!IsValidTokenFormat(rawToken))
            return null;

        var tokenHash = HashToken(rawToken);
        var now = DateTimeOffset.UtcNow;

        var current = await db.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (current is null)
            return null;

        // An already consumed token indicates possible replay.
        if (current.UsedAtUtc is not null)
        {
            await RevokeFamilyAsync(
                current.FamilyId, cancellationToken);
            return null;
        }

        if (current.RevokedAtUtc is not null ||
            current.ExpiresAtUtc <= now ||
            current.FamilyExpiresAtUtc <= now)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(
            current.UserId.ToString());

        if (user is null ||
            await userManager.IsLockedOutAsync(user) ||
            !string.Equals(
                user.SecurityStamp ?? string.Empty,
                current.SecurityStamp,
                StringComparison.Ordinal))
        {
            await RevokeFamilyAsync(
                current.FamilyId, cancellationToken);
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = jwtGenerator.Generate(
            user.Id,
            user.Email!,
            roles.ToArray());

        var replacementRaw = GenerateToken();
        var replacementHash = HashToken(replacementRaw);

        var proposedExpiry = now.Add(TokenLifetime);
        var replacementExpiry =
            proposedExpiry < current.FamilyExpiresAtUtc
                ? proposedExpiry
                : current.FamilyExpiresAtUtc;

        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = current.UserId,
            FamilyId = current.FamilyId,
            TokenHash = replacementHash,
            SecurityStamp = current.SecurityStamp,
            CreatedAtUtc = now,
            ExpiresAtUtc = replacementExpiry,
            FamilyExpiresAtUtc = current.FamilyExpiresAtUtc
        };

        bool rotated;

        await using (var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken))
        {
            // Atomic compare-and-swap.
            var updated = await db.RefreshTokens
                .Where(x =>
                    x.Id == current.Id &&
                    x.UsedAtUtc == null &&
                    x.RevokedAtUtc == null &&
                    x.ExpiresAtUtc > now &&
                    x.FamilyExpiresAtUtc > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.UsedAtUtc,
                        (DateTimeOffset?)now)
                    .SetProperty(
                        x => x.ReplacedByTokenHash,
                        replacementHash),
                    cancellationToken);

            rotated = updated == 1;

            if (rotated)
            {
                db.RefreshTokens.Add(replacement);

                await db.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);
            }
        }

        if (!rotated)
        {
            // Another request may have consumed the same token.
            await RevokeFamilyAsync(
                current.FamilyId, cancellationToken);

            return null;
        }

        return new LoginResult(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            replacementRaw,
            replacementExpiry);
    }

    public async Task RevokeAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        if (!IsValidTokenFormat(rawToken))
            return;

        var hash = HashToken(rawToken);

        var session = await db.RefreshTokens
            .AsNoTracking()
            .Where(x => x.TokenHash == hash)
            .Select(x => new { x.FamilyId })
            .SingleOrDefaultAsync(cancellationToken);

        if (session is null)
            return;

        await RevokeFamilyAsync(
            session.FamilyId, cancellationToken);
    }

    private Task<int> RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return db.RefreshTokens
            .Where(x =>
                x.FamilyId == familyId &&
                x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.RevokedAtUtc,
                    (DateTimeOffset?)now),
                cancellationToken);
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        return Convert.ToHexString(
            SHA256.HashData(bytes));
    }

    private static bool IsValidTokenFormat(string? token)
    {
        return token is { Length: 64 } &&
               token.All(Uri.IsHexDigit);
    }
}
