
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Driftgrid.Infrastructure.Identity;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Pilots;

public sealed class PilotProfileTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    [Fact]
    public async Task NewPilot_HasProfile()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var response = await client.GetAsync(
            "/api/v1/pilots/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content
            .ReadFromJsonAsync<ProfileResponse>();

        Assert.NotNull(profile);
        Assert.NotEqual(Guid.Empty, profile.UserId);
        Assert.StartsWith("Pilot-", profile.Callsign);
        Assert.Null(profile.Bio);
    }

    [Fact]
    public async Task Pilot_CanUpdateOwnProfile()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var response = await client.PutAsJsonAsync(
            "/api/v1/pilots/me",
            new
            {
                callsign = "  TokyoGhost  ",
                bio = "  Drifting at Ebisu  "
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content
            .ReadFromJsonAsync<ProfileResponse>();

        Assert.NotNull(updated);
        Assert.Equal("TokyoGhost", updated.Callsign);
        Assert.Equal("Drifting at Ebisu", updated.Bio);

        var fetched = await client
            .GetFromJsonAsync<ProfileResponse>(
                "/api/v1/pilots/me");

        Assert.NotNull(fetched);
        Assert.Equal(updated.UserId, fetched.UserId);
        Assert.Equal("TokyoGhost", fetched.Callsign);
        Assert.True(
            fetched.UpdatedAtUtc >= fetched.CreatedAtUtc);
    }

    [Fact]
    public async Task Pilot_CannotUpdateAnotherPilotsProfile()
    {
        using var first = await CreateClientAsync(
            ApplicationRoles.Pilot);

        using var second = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var firstProfile = await first
            .GetFromJsonAsync<ProfileResponse>(
                "/api/v1/pilots/me");

        var secondProfile = await second
            .GetFromJsonAsync<ProfileResponse>(
                "/api/v1/pilots/me");

        Assert.NotNull(firstProfile);
        Assert.NotNull(secondProfile);
        Assert.NotEqual(
            firstProfile.UserId,
            secondProfile.UserId);

        // Attempt to supply another user's ID in the body.
        var update = await first.PutAsJsonAsync(
            "/api/v1/pilots/me",
            new
            {
                userId = secondProfile.UserId,
                callsign = "OnlyFirstPilot",
                bio = "Own profile only"
            });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var updatedFirst = await first
            .GetFromJsonAsync<ProfileResponse>(
                "/api/v1/pilots/me");

        var unchangedSecond = await second
            .GetFromJsonAsync<ProfileResponse>(
                "/api/v1/pilots/me");

        Assert.NotNull(updatedFirst);
        Assert.NotNull(unchangedSecond);

        Assert.Equal(
            firstProfile.UserId,
            updatedFirst.UserId);

        Assert.Equal(
            "OnlyFirstPilot",
            updatedFirst.Callsign);

        Assert.Equal(
            secondProfile.UserId,
            unchangedSecond.UserId);

        Assert.Equal(
            secondProfile.Callsign,
            unchangedSecond.Callsign);
    }

    [Fact]
    public async Task Guest_CannotReadPilotProfile()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/pilots/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Judge_CannotAccessPilotOnlyEndpoint()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Judge);

        var response = await client.GetAsync(
            "/api/v1/pilots/me");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Pilot_InvalidCallsign_Returns400()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var response = await client.PutAsJsonAsync(
            "/api/v1/pilots/me",
            new
            {
                callsign = "   ",
                bio = "Testing validation"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private async Task<HttpClient> CreateClientAsync(
        string role)
    {
        var token = await TestAuthentication
            .CreateAccessTokenAsync(factory, role);

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", token);

        return client;
    }

    private sealed record ProfileResponse(
        Guid UserId,
        string Callsign,
        string? Bio,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
