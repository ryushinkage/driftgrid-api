using Driftgrid.Application.Features.Auth;

namespace Driftgrid.Application.Abstractions.Identity;

public interface IUserRegistrationService
{
    Task<RegisterPilotResult> RegisterPilotAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}