using Driftgrid.Domain.Entities;

namespace Driftgrid.Application.Features.Garage;

public sealed record GarageCarDto(
    Guid Id,
    Guid OwnerId,
    string Make,
    string Model,
    int Year,
    int Horsepower,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static GarageCarDto FromEntity(GarageCar car)
    {
        return new GarageCarDto(
            car.Id,
            car.OwnerId,
            car.Make,
            car.Model,
            car.Year,
            car.Horsepower,
            car.Notes,
            car.CreatedAtUtc,
            car.UpdatedAtUtc);
    }
}