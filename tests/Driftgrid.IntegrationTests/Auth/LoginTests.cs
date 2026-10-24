
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Auth;

public sealed class LoginTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    private readonly HttpClient _client =
        factory.CreateClient();

    [Fact]
    public async Task Login_ValidCredentials_AllowsAccess()
    {
        var email = $"pilot-{Guid.NewGuid():N}@example.com";
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

        var tokens = await login.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.Equal("Bearer", tokens.TokenType);
        Assert.True(tokens.ExpiresAtUtc > DateTimeOffset.UtcNow);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content
            .ReadFromJsonAsync<MeResponse>();

        Assert.NotNull(profile);
        Assert.Equal(email, profile.Email);
        Assert.Contains("Pilot", profile.Roles);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"pilot-{Guid.NewGuid():N}@example.com";

        var registration = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "SecurePass123" });

        Assert.Equal(
            HttpStatusCode.Created,
            registration.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = "WrongPassword123" });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            login.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync(
            "/api/v1/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_InvalidToken_Returns401()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/v1/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", "invalid.jwt.token");

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private sealed record LoginResponse(
        string AccessToken,
        string TokenType,
        DateTimeOffset ExpiresAtUtc);

    private sealed record MeResponse(
        string Id,
        string Email,
        List<string> Roles);
}
