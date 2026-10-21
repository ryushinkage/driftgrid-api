using Driftgrid.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Driftgrid.Infrastructure.Persistence.Configurations;

public sealed class IdentityRoleConfiguration
    : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(
        EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse(
                    "55d60a64-5fdf-49c1-960b-5e56f44d93a3"),
                Name = ApplicationRoles.Pilot,
                NormalizedName = "PILOT",
                ConcurrencyStamp =
                    "a4635d16-3917-48e9-b947-638a67f679cb"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse(
                    "07f6efff-de9e-4ee3-bc12-f974c6e1bb51"),
                Name = ApplicationRoles.Judge,
                NormalizedName = "JUDGE",
                ConcurrencyStamp =
                    "cb72e897-b41f-466b-af49-cbb494e388c8"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse(
                    "71df2496-da90-4f7d-99b1-58fad505bc9f"),
                Name = ApplicationRoles.Admin,
                NormalizedName = "ADMIN",
                ConcurrencyStamp =
                    "813525c4-625c-40d0-96ce-901baf45e972"
            }
        );
    }
}