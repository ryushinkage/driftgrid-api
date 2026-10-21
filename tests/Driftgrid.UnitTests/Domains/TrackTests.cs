using Driftgrid.Domain.Entities;
using Driftgrid.Domain.Exceptions;
using Xunit;

namespace Driftgrid.UnitTests.Domain;

public sealed class TrackTests
{
    [Fact]
    public void UpdateDetails_ValidData_UpdatesProperties()
    {
        // Arrange
        var track = new Track("Old Name", "Old Location");

        // Act
        track.UpdateDetails("Ebisu Minami", "Japan");

        // Assert
        Assert.Equal("Ebisu Minami", track.Name);
        Assert.Equal("Japan", track.Location);
    }

    [Fact]
    public void UpdateDetails_EmptyName_ThrowsException()
    {
        var track = new Track("Ebisu", "Japan");

        Assert.Throws<DomainValidationException>(
            () => track.UpdateDetails("", "Japan"));

        Assert.Equal("Ebisu", track.Name);
    }

    [Fact]
    public void UpdateDetails_InvalidLocation_DoesNotModifyTrack()
    {
        var track = new Track("Ebisu", "Japan");

        Assert.Throws<DomainValidationException>(
            () => track.UpdateDetails("New Name", "   "));

        Assert.Equal("Ebisu", track.Name);
        Assert.Equal("Japan", track.Location);
    }

    [Fact]
    public void Constructor_NameTooLong_ThrowsException()
    {
        var name = new string('A', 121);

        Assert.Throws<DomainValidationException>(
            () => new Track(name, "Japan"));
    }
}