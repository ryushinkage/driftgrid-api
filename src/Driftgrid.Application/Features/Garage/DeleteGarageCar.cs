using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Garage;

public sealed record DeleteGarageCarCommand(
    Guid OwnerId,
    Guid CarId)
    : IRequest<bool>;

public sealed class DeleteGarageCarCommandHandler(
    IGarageCarRepository repository)
    : IRequestHandler<DeleteGarageCarCommand, bool>
{
    public async Task<bool> Handle(
        DeleteGarageCarCommand request,
        CancellationToken cancellationToken)
    {
        var car = await repository.GetTrackedByIdForOwnerAsync(
            request.OwnerId,
            request.CarId,
            cancellationToken);

        if (car is null)
            return false;

        repository.Remove(car);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}