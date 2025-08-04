using Api.Data;

namespace Api.Models;

/// <summary>
/// Response model for an animal.
/// </summary>
public record AnimalResponse(
    /// <summary>
    /// Unique identifier for the animal.
    /// </summary>
    Guid Id,
    /// <summary>
    /// Name of the animal.
    /// </summary>
    string Name,
    /// <summary>
    /// Unique identifier for the owner.
    /// </summary>
    Guid OwnerId,
    /// <summary>
    /// Email address of the owner.
    /// </summary>
    string OwnerEmail,
    /// <summary>
    /// Birth date of the animal.
    /// </summary>
    DateTime BirthDate,
    /// <summary>
    /// Name of the owner.
    /// </summary>
    string OwnerName)
{
    /// <summary>
    /// Implicit conversion from Animal to AnimalResponse.
    /// </summary>
    /// <param name="animal">The Animal entity to convert.</param>
    /// <returns>An AnimalResponse containing the animal's data.</returns>
    public static implicit operator AnimalResponse(Animal animal)
    {
        return new AnimalResponse(
            animal.Id,
            animal.Name,
            animal.OwnerId,
            animal.OwnerEmail,
            animal.BirthDate,
            animal.OwnerName
        );
    }
}