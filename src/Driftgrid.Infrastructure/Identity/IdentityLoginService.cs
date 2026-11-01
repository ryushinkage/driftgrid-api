using Driftgrid.Application.Abstractions.Identity;
using Driftgrid.Application.Features.Auth;
using Microsoft.AspNetCore.Identity;

namespace Driftgrid.Infrastructure.Identity;

public sealed class IdentityLoginService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService)
    : IUserLoginService
{
    public async Task<LoginResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(
            email.Trim());

        if (user is null)
            return null;

        var result = await signInManager
            .CheckPasswordSignInAsync(
                user,
                password,
                lockoutOnFailure: true);

        if (!result.Succeeded)
            return null;

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = tokenGenerator.Generate(
            user.Id,
            user.Email!,
            roles.ToArray());

        var refreshToken =
            await refreshTokenService.IssueAsync(
                user.Id,
                cancellationToken);

        return new LoginResult(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }
}