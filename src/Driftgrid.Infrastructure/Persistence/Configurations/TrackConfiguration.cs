using Driftgrid.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Driftgrid.Infrastructure.Persistence.Configurations;

public sealed class TrackConfiguration
    : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");

        builder.HasKey(track => track.Id);

        builder.Property(track => track.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(track => track.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(track => track.Location)
            .HasColumnName("location")
            .HasMaxLength(200)
            .IsRequired();
    }
}