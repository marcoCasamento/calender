using Api.Data;

namespace Api.Models;

public record AnimalResponse(Guid Id, string Name, Guid OwnerId, string OwnerEmail, DateTime BirthDate, string OwnerName)
{
    /// <summary>
    /// Implicit conversion from Animal to AnimalResponse
    /// </summary>
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