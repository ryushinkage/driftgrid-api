using Driftgrid.Application.Features.Auth;

namespace Driftgrid.Application.Abstractions.Identity;

public interface IUserLoginService
{
    Task<LoginResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}