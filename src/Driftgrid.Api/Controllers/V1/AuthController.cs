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
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(
                request.Email,
                request.Password),
            cancellationToken);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials");
        }

        return Ok(new LoginResponse(
            result.AccessToken,
            "Bearer",
            result.ExpiresAtUtc));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirst("sub")?.Value,
            email = User.FindFirst("email")?.Value,
            roles = User.FindAll("role")
                .Select(claim => claim.Value)
                .ToArray()
        });
    }

}