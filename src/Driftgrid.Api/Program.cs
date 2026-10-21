using Driftgrid.Infrastructure.Health;
using Driftgrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Features.Tracks;
using Driftgrid.Infrastructure.Persistence.Repositories;
using Driftgrid.Api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// postgreSQL
var postgresConnection = builder.Configuration
    .GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string is missing.");

builder.Services.AddDbContext<DriftgridDbContext>(
    options => options.UseNpgsql(postgresConnection));


builder.Services.AddScoped<ITrackRepository, TrackRepository>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(
        typeof(CreateTrackCommand).Assembly);
});


// redis
var redisConnection = builder.Configuration
    .GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "Redis connection string is missing.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(redisConnection);

    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});

// health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DriftgridDbContext>(
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddCheck<RedisHealthCheck>(
        name: "redis",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(3));


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

// liveness: the API process is responding
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// readiness: PostgreSQL and Redis
var readinessOptions = new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
};

app.MapHealthChecks("/health/ready", readinessOptions);
app.MapHealthChecks("/health", readinessOptions);

app.Run();

static Task WriteHealthResponse(
    HttpContext context,
    HealthReport report)
{
    return context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString())
    });
}

public partial class Program
{
    
}
