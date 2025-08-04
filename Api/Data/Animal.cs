using Api.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Data;

public class Animal
{
    /// <summary>
    /// Primary key for the Animal entity.
    /// </summary>
    [Key]
    public required Guid Id { get; set; }

    /// <summary>
    /// Name of the animal.
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; } //I see from AnimalController that Name is required, so I make it required here as well

    /// <summary>
    /// Birth date of the animal.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Owner's unique identifier.
    /// </summary>
    public required Guid OwnerId { get; set; } //I'm assuming that every animal has an owner, so I make OwnerId required. AppointmentData take that field as granted, so the "required" keyword is probably appropriate here.

    /// <summary>
    /// Name of the owner. Optional.
    /// </summary>
    [MaxLength(100)]
    public string OwnerName { get; set; } = string.Empty; //that's questionable, but let's aI assume that an animal can exist without an owner name, so I make it optional

    /// <summary>
    /// Email address of the owner.
    /// </summary>
    [MaxLength(100)]
    public required string OwnerEmail { get; set; } //Need a way to contact the owner, so this is required

    /// <summary>
    /// Collection of appointments associated with the animal.
    /// </summary>
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Implicit conversion from CreateAnimalRequest to Animal
    /// </summary>
    public static implicit operator Animal(CreateAnimalRequest request)
    {
        return new Animal
        {
            Id = Guid.Empty, //ORM should take care of it
            Name = request.Name,
            OwnerEmail = request.OwnerEmail,
            OwnerId = request.OwnerId,
            BirthDate = request.BirthDate,
            OwnerName = request.OwnerName,
        };
    }
}
