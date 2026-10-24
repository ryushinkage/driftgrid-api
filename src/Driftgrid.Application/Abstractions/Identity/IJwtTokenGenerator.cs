using Driftgrid.Application.Features.Auth;

namespace Driftgrid.Application.Abstractions.Identity;

public interface IJwtTokenGenerator
{
    LoginResult Generate(
        Guid userId,
        string email,
        IReadOnlyList<string> roles);
}