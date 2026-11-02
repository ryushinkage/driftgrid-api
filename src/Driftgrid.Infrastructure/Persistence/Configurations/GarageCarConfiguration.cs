using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Driftgrid.Infrastructure.Persistence.Configurations;

public sealed class GarageCarConfiguration
    : IEntityTypeConfiguration<GarageCar>
{
    public void Configure(EntityTypeBuilder<GarageCar> builder)
    {
        builder.ToTable("garage_cars");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(x => x.Make)
            .HasColumnName("make")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(x => x.Horsepower)
            .HasColumnName("horsepower")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasOne<PilotProfile>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.CreatedAtUtc
        });
    }
}