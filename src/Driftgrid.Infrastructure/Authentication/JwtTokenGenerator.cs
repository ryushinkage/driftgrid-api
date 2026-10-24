using System.Security.Claims;
using Driftgrid.Application.Abstractions.Identity;
using Driftgrid.Application.Features.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Driftgrid.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(JwtOptions options)
    : IJwtTokenGenerator
{
    public LoginResult Generate(
        Guid userId,
        string email,
        IReadOnlyList<string> roles)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(
            options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new("sub", userId.ToString()),
            new("email", email),
            new("jti", Guid.NewGuid().ToString("N"))
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        var keyBytes = Convert.FromBase64String(
            options.SigningKeyBase64);

        var securityKey = new SymmetricSecurityKey(
            keyBytes);

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(descriptor);

        return new LoginResult(
            token,
            expires);
    }
}