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
using Driftgrid.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Driftgrid.Application.Abstractions.Identity;
using Driftgrid.Infrastructure.Identity;
using Driftgrid.Infrastructure.Authentication;
using Driftgrid.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Driftgrid.Api.Authorization;




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


builder.Services.AddDataProtection();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DriftgridDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<
    IUserRegistrationService,
    IdentityRegistrationService>();

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


var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    string.IsNullOrWhiteSpace(jwtOptions.SigningKeyBase64))
{
    throw new InvalidOperationException(
        "JWT configuration is incomplete.");
}

byte[] signingKeyBytes;

try
{
    signingKeyBytes = Convert.FromBase64String(
        jwtOptions.SigningKeyBase64);
}
catch (FormatException ex)
{
    throw new InvalidOperationException(
        "JWT signing key must be valid Base64.", ex);
}

if (signingKeyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must be at least 256 bits.");
}

if (jwtOptions.AccessTokenMinutes is < 1 or > 60)
{
    throw new InvalidOperationException(
        "JWT lifetime must be between 1 and 60 minutes.");
}

builder.Services.AddSingleton(jwtOptions);

builder.Services.AddScoped<
    IJwtTokenGenerator,
    JwtTokenGenerator>();

builder.Services.AddScoped<
    IUserLoginService,
    IdentityLoginService>();

builder.Services.AddScoped<
    IRefreshTokenService,
    EfRefreshTokenService>();


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(signingKeyBytes),

                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = "sub",
                RoleClaimType = "role",

                ValidAlgorithms =
                [
                    SecurityAlgorithms.HmacSha256
                ]
            };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.ManageTracks,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(ApplicationRoles.Admin));
});

var app = builder.Build();

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthentication();
app.UseAuthorization();
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
