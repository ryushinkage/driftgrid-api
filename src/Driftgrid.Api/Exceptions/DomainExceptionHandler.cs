using Driftgrid.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Driftgrid.Api.Exceptions;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainValidationException)
            return false;

        await Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid domain data",
            detail: exception.Message
        ).ExecuteAsync(httpContext);

        return true;
    }
}