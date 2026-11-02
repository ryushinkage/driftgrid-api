using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Domain.Entities;
using MediatR;

namespace Driftgrid.Application.Features.Garage;

public sealed record CreateGarageCarCommand(
    Guid OwnerId,
    string Make,
    string Model,
    int Year,
    int Horsepower,
    string? Notes)
    : IRequest<GarageCarDto>;

public sealed class CreateGarageCarCommandHandler(
    IGarageCarRepository repository)
    : IRequestHandler<CreateGarageCarCommand, GarageCarDto>
{
    public async Task<GarageCarDto> Handle(
        CreateGarageCarCommand request,
        CancellationToken cancellationToken)
    {
        var car = new GarageCar(
            request.OwnerId,
            request.Make,
            request.Model,
            request.Year,
            request.Horsepower,
            request.Notes);

        await repository.AddAsync(
            car,
            cancellationToken);

        await repository.SaveChangesAsync(
            cancellationToken);

        return GarageCarDto.FromEntity(car);
    }
}