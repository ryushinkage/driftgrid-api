using Driftgrid.Domain.Entities;
using Driftgrid.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Driftgrid.Infrastructure.Persistence.Configurations;

public sealed class PilotProfileConfiguration
    : IEntityTypeConfiguration<PilotProfile>
{
    public void Configure(
        EntityTypeBuilder<PilotProfile> builder)
    {
        builder.ToTable("pilot_profiles");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(x => x.Callsign)
            .HasColumnName("callsign")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasColumnName("bio")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<PilotProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}