
using System.Net;
using System.Net.Http.Json;
using Driftgrid.Infrastructure.Identity;
using Driftgrid.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Driftgrid.IntegrationTests.Auth;

public sealed class RegistrationTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidUser_CreatesPilot()
    {
        var email = $"pilot-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password, role = "Admin" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<RegisterResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);

        using var scope = factory.Services.CreateScope();

        var manager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await manager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.Equal(created.Id, user.Id);
        Assert.NotEqual(password, user.PasswordHash);

        Assert.True(
            await manager.CheckPasswordAsync(user, password));

        var roles = await manager.GetRolesAsync(user);

        Assert.Single(roles);
        Assert.Equal(ApplicationRoles.Pilot, roles[0]);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            email,
            password = "SecurePass123"
        };

        var first = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", request);

        var second = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                email = $"weak-{Guid.NewGuid():N}@example.com",
                password = "lowercaseonly123"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                email = "not-an-email",
                password = "SecurePass123"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private sealed record RegisterResponse(
        Guid Id,
        string Email);
}
