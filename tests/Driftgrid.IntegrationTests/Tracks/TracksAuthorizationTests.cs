
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Driftgrid.Infrastructure.Identity;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Tracks;

public sealed class TracksAuthorizationTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    [Fact]
    public async Task Guest_CanReadTracks()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/tracks?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Guest_CannotModifyTracks(string method)
    {
        using var client = factory.CreateClient();
        using var request = CreateWriteRequest(method);

        var response = await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Pilot_CannotModifyTracks(string method)
    {
        await AssertForbiddenForRoleAsync(
            ApplicationRoles.Pilot, method);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Judge_CannotModifyTracks(string method)
    {
        await AssertForbiddenForRoleAsync(
            ApplicationRoles.Judge, method);
    }

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeleteTrack()
    {
        using var client = await CreateAuthorizedClientAsync(
            ApplicationRoles.Admin);

        var create = await client.PostAsJsonAsync(
            "/api/v1/tracks",
            new
            {
                name = $"Admin Track {Guid.NewGuid():N}",
                location = "Prague"
            });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var track = await create.Content
            .ReadFromJsonAsync<TrackResponse>();

        Assert.NotNull(track);

        var update = await client.PutAsJsonAsync(
            $"/api/v1/tracks/{track.Id}",
            new
            {
                name = "Updated Admin Track",
                location = "Tokyo"
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            update.StatusCode);

        var delete = await client.DeleteAsync(
            $"/api/v1/tracks/{track.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            delete.StatusCode);

        var read = await client.GetAsync(
            $"/api/v1/tracks/{track.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            read.StatusCode);
    }

    private async Task AssertForbiddenForRoleAsync(
        string role,
        string method)
    {
        using var client = await CreateAuthorizedClientAsync(role);
        using var request = CreateWriteRequest(method);

        var response = await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(
        string role)
    {
        var token = await TestAuthentication.CreateAccessTokenAsync(
            factory, role);

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static HttpRequestMessage CreateWriteRequest(
        string method)
    {
        var url = method == "POST"
            ? "/api/v1/tracks"
            : $"/api/v1/tracks/{Guid.NewGuid()}";

        var request = new HttpRequestMessage(
            new HttpMethod(method), url);

        if (method != "DELETE")
        {
            request.Content = JsonContent.Create(new
            {
                name = "Protected Track",
                location = "Prague"
            });
        }

        return request;
    }

    private sealed record TrackResponse(
        Guid Id,
        string Name,
        string Location);
}
