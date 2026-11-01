
using System.Net;
using System.Net.Http.Json;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Auth;

public sealed class RefreshTokenTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    private readonly HttpClient _client =
        factory.CreateClient();

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewPair()
    {
        var original = await CreateSessionAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = original.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rotated = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(rotated);
        Assert.NotEqual(
            original.RefreshToken,
            rotated.RefreshToken);

        Assert.False(string.IsNullOrWhiteSpace(
            rotated.AccessToken));

        Assert.True(
            rotated.RefreshTokenExpiresAtUtc >
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesFamily()
    {
        var original = await CreateSessionAsync();

        var first = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = original.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var rotated = await first.Content
            .ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(rotated);

        // Replay the consumed token.
        var replay = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = original.RefreshToken });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            replay.StatusCode);

        // Even the replacement must now be unusable.
        var afterReplay = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = rotated.RefreshToken });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            afterReplay.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshSession()
    {
        var session = await CreateSessionAsync();

        var logout = await _client.PostAsJsonAsync(
            "/api/v1/auth/logout",
            new { refreshToken = session.RefreshToken });

        Assert.Equal(
            HttpStatusCode.NoContent,
            logout.StatusCode);

        var refresh = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = session.RefreshToken });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refresh.StatusCode);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = new string('A', 64) });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private async Task<TokenResponse> CreateSessionAsync()
    {
        var email = $"refresh-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass123";

        var registration = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registration.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var result = await login.Content
            .ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(result);
        return result;
    }

    private sealed record TokenResponse(
        string AccessToken,
        string TokenType,
        DateTimeOffset ExpiresAtUtc,
        string RefreshToken,
        DateTimeOffset RefreshTokenExpiresAtUtc);
}
