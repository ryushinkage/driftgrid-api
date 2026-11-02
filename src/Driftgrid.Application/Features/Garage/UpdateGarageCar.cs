using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Garage;

public sealed record UpdateGarageCarCommand(
    Guid OwnerId,
    Guid CarId,
    string Make,
    string Model,
    int Year,
    int Horsepower,
    string? Notes)
    : IRequest<bool>;

public sealed class UpdateGarageCarCommandHandler(
    IGarageCarRepository repository)
    : IRequestHandler<UpdateGarageCarCommand, bool>
{
    public async Task<bool> Handle(
        UpdateGarageCarCommand request,
        CancellationToken cancellationToken)
    {
        var car = await repository.GetTrackedByIdForOwnerAsync(
            request.OwnerId,
            request.CarId,
            cancellationToken);

        if (car is null)
            return false;

        car.UpdateDetails(
            request.Make,
            request.Model,
            request.Year,
            request.Horsepower,
            request.Notes);

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}