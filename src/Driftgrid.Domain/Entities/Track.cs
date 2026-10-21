using Driftgrid.Domain.Exceptions;
namespace Driftgrid.Domain.Entities;

public sealed class Track
{
    private Track() { }

    public Track(string name, string location)
    {
        Id = Guid.NewGuid();
        UpdateDetails(name, location);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public void UpdateDetails(string name, string location)
    {
        var validatedName = Validate(
            name, 120, nameof(Name));

        var validatedLocation = Validate(
            location, 200, nameof(Location));

        Name = validatedName;
        Location = validatedLocation;
    }

    private static string Validate(
        string value,
        int maxLength,
        string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(
                $"{propertyName} is required.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException(
                $"{propertyName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }
}
