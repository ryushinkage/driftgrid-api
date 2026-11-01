using Driftgrid.Application.Abstractions.Identity;
using MediatR;

namespace Driftgrid.Application.Features.Auth;

public sealed record RevokeRefreshSessionCommand(
    string RefreshToken) : IRequest<bool>;

public sealed class RevokeRefreshSessionCommandHandler(
    IRefreshTokenService service)
    : IRequestHandler<RevokeRefreshSessionCommand, bool>
{
    public async Task<bool> Handle(
        RevokeRefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        await service.RevokeAsync(
            request.RefreshToken,
            cancellationToken);

        return true;
    }
}