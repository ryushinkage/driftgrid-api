using Driftgrid.Application.Abstractions.Identity;
using MediatR;

namespace Driftgrid.Application.Features.Auth;

public sealed record RegisterPilotCommand(
    string Email,
    string Password) : IRequest<RegisterPilotResult>;

public sealed class RegisterPilotCommandHandler(
    IUserRegistrationService registrationService)
    : IRequestHandler<RegisterPilotCommand, RegisterPilotResult>
{
    public Task<RegisterPilotResult> Handle(
        RegisterPilotCommand request,
        CancellationToken cancellationToken)
    {
        return registrationService.RegisterPilotAsync(
            request.Email,
            request.Password,
            cancellationToken);
    }
}