using Driftgrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace Driftgrid.IntegrationTests.Infrastructure;

public sealed class DriftgridApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("driftgrid_tests")
            .WithUsername("test")
            .WithPassword("test_password")
            .Build();

    private readonly RedisContainer _redis =
        new RedisBuilder("redis:7-alpine")
            .Build();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting( //  передает тестовые connection strings приложению до запуска
            "ConnectionStrings:Postgres",
            _postgres.GetConnectionString());

        builder.UseSetting(
            "ConnectionStrings:Redis",
            _redis.GetConnectionString());
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync());

        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<DriftgridDbContext>();

        await db.Database.MigrateAsync(); // применяет настоящую миграцию InitialCreate к временной PostgreSQL
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();

        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}