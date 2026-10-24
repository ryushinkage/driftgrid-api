using Driftgrid.Application.Abstractions.Identity;
using MediatR;

namespace Driftgrid.Application.Features.Auth;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<LoginResult?>;

public sealed class LoginCommandHandler(
    IUserLoginService loginService)
    : IRequestHandler<LoginCommand, LoginResult?>
{
    public Task<LoginResult?> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        return loginService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);
    }
}