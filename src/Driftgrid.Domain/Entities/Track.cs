namespace Driftgrid.Domain.Entities;

public sealed class Track
{
    private Track() { }

    public Track(string name, string location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Track name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException(
                "Track location is required.", nameof(location));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Location = location.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
}