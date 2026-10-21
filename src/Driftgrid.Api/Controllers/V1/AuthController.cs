using Driftgrid.Api.Contracts.Auth;
using Driftgrid.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Driftgrid.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ISender sender)
    : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterPilotCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return result.Status switch
        {
            RegisterPilotStatus.Created => StatusCode(
                StatusCodes.Status201Created,
                new RegisterResponse(
                    result.UserId!.Value,
                    result.Email!)),

            RegisterPilotStatus.DuplicateEmail => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Email already registered"),

            RegisterPilotStatus.InvalidInput => BadRequest(
                new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        ["registration"] =
                            result.Errors?.ToArray() ?? []
                    })
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed"
                }),

            _ => throw new InvalidOperationException(
                "Unexpected registration status.")
        };
    }
}