using Driftgrid.Application.Features.Auth;

namespace Driftgrid.Application.Abstractions.Identity;

public interface IJwtTokenGenerator
{
    AccessTokenResult Generate(
        Guid userId,
        string email,
        IReadOnlyList<string> roles);
}