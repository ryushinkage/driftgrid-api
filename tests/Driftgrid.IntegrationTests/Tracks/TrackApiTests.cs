
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

        var search = Uri.EscapeDataString(created.Name);

        var result = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={search}");

        Assert.NotNull(result);
        Assert.Contains(
            result.Items,
            track => track.Id == created.Id);
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
    
    private sealed record PagedTracksResponse(
        List<TrackResponse> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    
    [Fact]
    public async Task GetTracks_Pagination_ReturnsCorrectPages()
    {
        var marker = $"paging-{Guid.NewGuid():N}";

        foreach (var name in new[] { "Zulu", "Alpha", "Mike" })
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/tracks",
                new
                {
                    name = $"{marker}-{name}",
                    location = "Prague"
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var page1 = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={marker}&page=1&pageSize=2");

        Assert.NotNull(page1);
        Assert.Equal(1, page1.Page);
        Assert.Equal(2, page1.PageSize);
        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.TotalPages);
        Assert.Equal(
            new[] { $"{marker}-Alpha", $"{marker}-Mike" },
            page1.Items.Select(t => t.Name));

        var page2 = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={marker}&page=2&pageSize=2");

        Assert.NotNull(page2);
        Assert.Single(page2.Items);
        Assert.Equal($"{marker}-Zulu", page2.Items[0].Name);

        var page3 = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={marker}&page=3&pageSize=2");

        Assert.NotNull(page3);
        Assert.Empty(page3.Items);
        Assert.Equal(3, page3.TotalCount);
    }
    
    [Fact]
    public async Task GetTracks_Search_IsCaseInsensitive()
    {
        var marker = $"SearchMarker{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/tracks",
            new
            {
                name = "Search Test Track",
                location = $"Japan-{marker}"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<TrackResponse>();

        Assert.NotNull(created);

        var result = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={marker.ToUpperInvariant()}");

        Assert.NotNull(result);
        Assert.Contains(result.Items, t => t.Id == created.Id);
    }
    
    [Fact]
    public async Task GetTracks_SortByLocationDescending_Works()
    {
        var marker = $"sort-{Guid.NewGuid():N}";

        foreach (var location in new[] { "Berlin", "Tokyo", "Prague" })
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/tracks",
                new
                {
                    name = $"{marker}-{location}",
                    location
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var result = await _client.GetFromJsonAsync<PagedTracksResponse>(
            $"/api/v1/tracks?search={marker}" +
            "&sortBy=location&descending=true");

        Assert.NotNull(result);

        Assert.Equal(
            new[] { "Tokyo", "Prague", "Berlin" },
            result.Items.Select(t => t.Location));
    }
    
    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("sortBy=horsepower")]
    public async Task GetTracks_InvalidQuery_Returns400(
        string query)
    {
        var response = await _client.GetAsync(
            $"/api/v1/tracks?{query}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

}
