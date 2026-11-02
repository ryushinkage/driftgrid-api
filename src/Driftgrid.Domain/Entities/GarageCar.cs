
using Driftgrid.Domain.Exceptions;

namespace Driftgrid.Domain.Entities;

public sealed class GarageCar
{
    private GarageCar() { } // EF Core

    public GarageCar(
        Guid ownerId,
        string make,
        string model,
        int year,
        int horsepower,
        string? notes)
    {
        if (ownerId == Guid.Empty)
        {
            throw new DomainValidationException(
                "OwnerId cannot be empty.");
        }

        Id = Guid.NewGuid();
        OwnerId = ownerId;

        UpdateDetails(make, model, year, horsepower, notes);

        CreatedAtUtc = UpdatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }

    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public int Horsepower { get; private set; }
    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void UpdateDetails(
        string make,
        string model,
        int year,
        int horsepower,
        string? notes)
    {
        var validatedMake = ValidateRequired(
            make, 60, nameof(Make));

        var validatedModel = ValidateRequired(
            model, 80, nameof(Model));

        if (year is < 1950 or > 2100)
        {
            throw new DomainValidationException(
                "Year must be between 1950 and 2100.");
        }

        if (horsepower is < 1 or > 3000)
        {
            throw new DomainValidationException(
                "Horsepower must be between 1 and 3000.");
        }

        var validatedNotes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();

        if (validatedNotes?.Length > 1000)
        {
            throw new DomainValidationException(
                "Notes cannot exceed 1000 characters.");
        }

        Make = validatedMake;
        Model = validatedModel;
        Year = year;
        Horsepower = horsepower;
        Notes = validatedNotes;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string ValidateRequired(
        string value,
        int maxLength,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(
                $"{field} is required.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException(
                $"{field} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }
}
