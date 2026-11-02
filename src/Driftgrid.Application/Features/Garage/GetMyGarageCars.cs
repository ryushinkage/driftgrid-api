using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Common.Pagination;
using Driftgrid.Domain.Exceptions;
using MediatR;

namespace Driftgrid.Application.Features.Garage;

public sealed record GetMyGarageCarsQuery(
    Guid OwnerId,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedResult<GarageCarDto>>;

public sealed class GetMyGarageCarsQueryHandler(
    IGarageCarRepository repository)
    : IRequestHandler<
        GetMyGarageCarsQuery,
        PagedResult<GarageCarDto>>
{
    public async Task<PagedResult<GarageCarDto>> Handle(
        GetMyGarageCarsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new DomainValidationException(
                "Page must be greater than zero.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new DomainValidationException(
                "PageSize must be between 1 and 100.");
        }

        var offset = ((long)request.Page - 1)
                     * request.PageSize;

        if (offset > int.MaxValue)
        {
            throw new DomainValidationException(
                "Requested page offset is too large.");
        }

        var result = await repository.GetPagedByOwnerAsync(
            request.OwnerId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<GarageCarDto>(
            result.Items
                .Select(GarageCarDto.FromEntity)
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}