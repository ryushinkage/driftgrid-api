using Driftgrid.Domain.Entities;
using Driftgrid.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Driftgrid.Infrastructure.Persistence;

public sealed class DriftgridDbContext(
    DbContextOptions<DriftgridDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>(options)
{
    public DbSet<Track> Tracks => Set<Track>();
    
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DriftgridDbContext).Assembly);
    }
}