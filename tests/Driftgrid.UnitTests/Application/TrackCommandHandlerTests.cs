using Driftgrid.Application.Abstractions.Persistence;
using Driftgrid.Application.Features.Tracks;
using Driftgrid.Domain.Entities;
using Moq;
using Xunit;

namespace Driftgrid.UnitTests.Application;

public sealed class TrackCommandHandlerTests
{
    [Fact]
    public async Task Update_ExistingTrack_SavesChanges()
    {
        var track = new Track("Old", "Japan");
        var repository = new Mock<ITrackRepository>();

        repository
            .Setup(x => x.GetTrackedByIdAsync(
                track.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        var handler = new UpdateTrackCommandHandler(
            repository.Object);

        var result = await handler.Handle(
            new UpdateTrackCommand(
                track.Id, "Ebisu Minami", "Fukushima"),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal("Ebisu Minami", track.Name);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_MissingTrack_DoesNotSave()
    {
        var repository = new Mock<ITrackRepository>();

        repository
            .Setup(x => x.GetTrackedByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Track?)null);

        var handler = new UpdateTrackCommandHandler(
            repository.Object);

        var result = await handler.Handle(
            new UpdateTrackCommand(
                Guid.NewGuid(), "Ebisu", "Japan"),
            CancellationToken.None);

        Assert.False(result);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingTrack_RemovesAndSaves()
    {
        var track = new Track("Ebisu", "Japan");
        var repository = new Mock<ITrackRepository>();

        repository
            .Setup(x => x.GetTrackedByIdAsync(
                track.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        var handler = new DeleteTrackCommandHandler(
            repository.Object);

        var result = await handler.Handle(
            new DeleteTrackCommand(track.Id),
            CancellationToken.None);

        Assert.True(result);

        repository.Verify(
            x => x.Remove(track),
            Times.Once);

        repository.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
