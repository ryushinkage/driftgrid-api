
using System.Net;
using System.Net.Http.Json;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Tracks;

public sealed class TracksApiTests
    : IClassFixture<DriftgridApiFactory>
{
    private readonly HttpClient _client;

    public TracksApiTests(DriftgridApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTrack_Returns201_AndCanBeRead()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/tracks",
            new
            {
                name = "Ebisu Minami",
                location = "Fukushima, Japan"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content
            .ReadFromJsonAsync<TrackResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Ebisu Minami", created.Name);

        var getResponse = await _client.GetAsync(
            $"/api/v1/tracks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<TrackResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
    }

    [Fact]
    public async Task GetTracks_ContainsCreatedTrack()
    {
        var created = await CreateTrackAsync();

        var tracks = await _client.GetFromJsonAsync<
            List<TrackResponse>>("/api/v1/tracks");

        Assert.NotNull(tracks);
        Assert.Contains(tracks, x => x.Id == created.Id);
    }

    [Fact]
    public async Task UpdateTrack_PersistsNewValues()
    {
        var created = await CreateTrackAsync();

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/tracks/{created.Id}",
            new
            {
                name = "Updated Ebisu",
                location = "Japan"
            });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await _client.GetFromJsonAsync<TrackResponse>(
            $"/api/v1/tracks/{created.Id}");

        Assert.NotNull(updated);
        Assert.Equal("Updated Ebisu", updated.Name);
        Assert.Equal("Japan", updated.Location);
    }

    [Fact]
    public async Task DeleteTrack_Returns404OnNextGet()
    {
        var created = await CreateTrackAsync();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/v1/tracks/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/v1/tracks/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateTrack_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/tracks",
            new
            {
                name = "",
                location = "Japan"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task MissingTrack_Returns404()
    {
        var id = Guid.NewGuid();

        var get = await _client.GetAsync(
            $"/api/v1/tracks/{id}");

        var delete = await _client.DeleteAsync(
            $"/api/v1/tracks/{id}");

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Readiness_PostgresAndRedis_Returns200()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<TrackResponse> CreateTrackAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/tracks",
            new
            {
                name = $"Test Track {Guid.NewGuid():N}",
                location = "Prague"
            });

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TrackResponse>()
            ?? throw new InvalidOperationException(
                "Track response was empty.");
    }

    private sealed record TrackResponse(
        Guid Id,
        string Name,
        string Location);
}
