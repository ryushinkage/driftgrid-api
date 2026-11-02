
using System.Net;
using System.Net.Http.Json;
using Driftgrid.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Driftgrid.IntegrationTests.Infrastructure;

public static class TestAuthentication
{
    public static async Task<string> CreateAccessTokenAsync(
        DriftgridApiFactory factory,
        string role)
    {
        using var client = factory.CreateClient();

        var email = $"auth-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123";

        // rael public registration, always creates a Pilot
        using var registration = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password });

        if (registration.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException(
                $"Test registration failed: {registration.StatusCode}");
        }

        // rolee elevation exists exclusively in test setup
        if (role != ApplicationRoles.Pilot)
        {
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByEmailAsync(email)
                ?? throw new InvalidOperationException(
                    "Test user was not found.");

            var added = await userManager.AddToRoleAsync(
                user, role);

            if (!added.Succeeded)
                throw new InvalidOperationException(
                    "Failed to assign test role.");

            var removed = await userManager.RemoveFromRoleAsync(
                user, ApplicationRoles.Pilot);

            if (!removed.Succeeded)
                throw new InvalidOperationException(
                    "Failed to remove default test role.");
        }

        // login after assigning role
        using var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password });

        login.EnsureSuccessStatusCode();

        var result = await login.Content
            .ReadFromJsonAsync<TokenResponse>();

        return result?.AccessToken
            ?? throw new InvalidOperationException(
                "Access token was not returned.");
    }
    private sealed record TokenResponse(string AccessToken);
}
