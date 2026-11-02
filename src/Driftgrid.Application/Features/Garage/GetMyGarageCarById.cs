using Driftgrid.Application.Abstractions.Persistence;
using MediatR;

namespace Driftgrid.Application.Features.Garage;

public sealed record GetMyGarageCarByIdQuery(
    Guid OwnerId,
    Guid CarId)
    : IRequest<GarageCarDto?>;

public sealed class GetMyGarageCarByIdQueryHandler(
    IGarageCarRepository repository)
    : IRequestHandler<
        GetMyGarageCarByIdQuery,
        GarageCarDto?>
{
    public async Task<GarageCarDto?> Handle(
        GetMyGarageCarByIdQuery request,
        CancellationToken cancellationToken)
    {
        var car = await repository.GetByIdForOwnerAsync(
            request.OwnerId,
            request.CarId,
            cancellationToken);

        return car is null
            ? null
            : GarageCarDto.FromEntity(car);
    }
}