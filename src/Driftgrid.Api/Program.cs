using Driftgrid.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration
                           .GetConnectionString("Postgres")
                       ?? throw new InvalidOperationException(
                           "Postgres connection string is missing.");

builder.Services.AddDbContext<DriftgridDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();