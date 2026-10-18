using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Driftgrid.Infrastructure.Persistence;

public sealed class DriftgridDbContext(
    DbContextOptions<DriftgridDbContext> options)
    : DbContext(options)
{
    public DbSet<Track> Tracks => Set<Track>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DriftgridDbContext).Assembly);
    }
}