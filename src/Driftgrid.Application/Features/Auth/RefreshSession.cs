using Driftgrid.Application.Abstractions.Identity;
using MediatR;

namespace Driftgrid.Application.Features.Auth;

public sealed record RefreshSessionCommand(
    string RefreshToken) : IRequest<LoginResult?>;

public sealed class RefreshSessionCommandHandler(
    IRefreshTokenService service)
    : IRequestHandler<RefreshSessionCommand, LoginResult?>
{
    public Task<LoginResult?> Handle(
        RefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        return service.RotateAsync(
            request.RefreshToken,
            cancellationToken);
    }
}