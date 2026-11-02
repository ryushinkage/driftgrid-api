
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Driftgrid.Infrastructure.Identity;
using Driftgrid.IntegrationTests.Infrastructure;
using Xunit;

namespace Driftgrid.IntegrationTests.Garage;

public sealed class GarageCarsTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    private const string Url = "/api/v1/garage/cars";

    [Fact]
    public async Task Guest_CannotAccessGarage()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(Url);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Judge_CannotAccessPilotGarage()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Judge);

        var response = await client.GetAsync(Url);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Pilot_CanCreateAndReadOwnCar()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var pilotId = await GetPilotIdAsync(client);

        // Attempt to forge ownerId.
        var response = await client.PostAsJsonAsync(
            Url,
            new
            {
                ownerId = Guid.NewGuid(),
                make = "Nissan",
                model = "Silvia S15",
                year = 2002,
                horsepower = 420,
                notes = "SR20DET"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        var created = await response.Content
            .ReadFromJsonAsync<CarResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(pilotId, created.OwnerId);
        Assert.Equal("Nissan", created.Make);
        Assert.Equal("Silvia S15", created.Model);
        Assert.Equal(420, created.Horsepower);

        var fetched = await client
            .GetFromJsonAsync<CarResponse>(
                $"{Url}/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.OwnerId, fetched.OwnerId);
    }

    [Fact]
    public async Task Pilot_CanUpdateOwnCar()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var car = await CreateCarAsync(client);

        var update = await client.PutAsJsonAsync(
            $"{Url}/{car.Id}",
            new
            {
                make = "Toyota",
                model = "Chaser JZX100",
                year = 1998,
                horsepower = 550,
                notes = "1JZ-GTE"
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            update.StatusCode);

        var updated = await client
            .GetFromJsonAsync<CarResponse>(
                $"{Url}/{car.Id}");

        Assert.NotNull(updated);
        Assert.Equal(car.Id, updated.Id);
        Assert.Equal(car.OwnerId, updated.OwnerId);
        Assert.Equal("Toyota", updated.Make);
        Assert.Equal("Chaser JZX100", updated.Model);
        Assert.Equal(1998, updated.Year);
        Assert.Equal(550, updated.Horsepower);
        Assert.Equal("1JZ-GTE", updated.Notes);
    }

    [Fact]
    public async Task Pilot_CanDeleteOwnCar()
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var car = await CreateCarAsync(client);

        var delete = await client.DeleteAsync(
            $"{Url}/{car.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            delete.StatusCode);

        var read = await client.GetAsync(
            $"{Url}/{car.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            read.StatusCode);
    }

    [Fact]
    public async Task Pilot_CannotReadAnotherPilotsCar()
    {
        using var owner = await CreateClientAsync(
            ApplicationRoles.Pilot);

        using var stranger = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var car = await CreateCarAsync(owner);

        var response = await stranger.GetAsync(
            $"{Url}/{car.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Pilot_CannotUpdateAnotherPilotsCar()
    {
        using var owner = await CreateClientAsync(
            ApplicationRoles.Pilot);

        using var stranger = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var car = await CreateCarAsync(owner);

        var response = await stranger.PutAsJsonAsync(
            $"{Url}/{car.Id}",
            new
            {
                make = "BMW",
                model = "M3 E92",
                year = 2008,
                horsepower = 600,
                notes = "Unauthorized update"
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var unchanged = await owner
            .GetFromJsonAsync<CarResponse>(
                $"{Url}/{car.Id}");

        Assert.NotNull(unchanged);
        Assert.Equal("Nissan", unchanged.Make);
        Assert.Equal("Silvia S15", unchanged.Model);
    }

    [Fact]
    public async Task Pilot_CannotDeleteAnotherPilotsCar()
    {
        using var owner = await CreateClientAsync(
            ApplicationRoles.Pilot);

        using var stranger = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var car = await CreateCarAsync(owner);

        var delete = await stranger.DeleteAsync(
            $"{Url}/{car.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            delete.StatusCode);

        var stillExists = await owner.GetAsync(
            $"{Url}/{car.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            stillExists.StatusCode);
    }

    [Fact]
    public async Task GarageList_ContainsOnlyOwnCars()
    {
        using var first = await CreateClientAsync(
            ApplicationRoles.Pilot);

        using var second = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var ownCar = await CreateCarAsync(first);
        var anotherCar = await CreateCarAsync(second);

        var result = await first
            .GetFromJsonAsync<PagedCarsResponse>(
                $"{Url}?page=1&pageSize=10");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(ownCar.Id, result.Items[0].Id);

        Assert.DoesNotContain(
            result.Items,
            car => car.Id == anotherCar.Id);
    }

    [Theory]
    [InlineData(1949, 420)]
    [InlineData(2002, 0)]
    public async Task CreateCar_InvalidSpecs_Returns400(
        int year,
        int horsepower)
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var response = await client.PostAsJsonAsync(
            Url,
            new
            {
                make = "Nissan",
                model = "Silvia S15",
                year,
                horsepower
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    public async Task GarageList_InvalidPagination_Returns400(
        string query)
    {
        using var client = await CreateClientAsync(
            ApplicationRoles.Pilot);

        var response = await client.GetAsync(
            $"{Url}?{query}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private async Task<HttpClient> CreateClientAsync(
        string role)
    {
        var token = await TestAuthentication.CreateAccessTokenAsync(
            factory,
            role);

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        return client;
    }

    private static async Task<Guid> GetPilotIdAsync(
        HttpClient client)
    {
        var profile = await client
            .GetFromJsonAsync<PilotResponse>(
                "/api/v1/pilots/me");

        Assert.NotNull(profile);

        return profile.UserId;
    }

    private static async Task<CarResponse> CreateCarAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            Url,
            new
            {
                make = "Nissan",
                model = "Silvia S15",
                year = 2002,
                horsepower = 420,
                notes = "Test car"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var car = await response.Content
            .ReadFromJsonAsync<CarResponse>();

        Assert.NotNull(car);
        return car;
    }

    private sealed record PilotResponse(Guid UserId);

    private sealed record CarResponse(
        Guid Id,
        Guid OwnerId,
        string Make,
        string Model,
        int Year,
        int Horsepower,
        string? Notes,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);

    private sealed record PagedCarsResponse(
        List<CarResponse> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
