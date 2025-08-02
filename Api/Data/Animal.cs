using System.ComponentModel.DataAnnotations;

namespace Api.Data;

public class Animal
{
    [Key]
    public required Guid Id { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; } //I see from AnimalController that Name is required, so I make it required here as well

    public DateTime BirthDate { get; set; }

    public required Guid OwnerId { get; set; } //I'm assuming that every animal has an owner, so I make OwnerId required. AppointmentData take that field as granted, so the "required" keyword is probably appropriate here.
    [MaxLength(100)]
    public string OwnerName { get; set; } = string.Empty; //that's questionable, but let's aI assume that an animal can exist without an owner name, so I make it optional
    [MaxLength(100)]
    public required string OwnerEmail { get; set; } //Need a way to contact the owner, so this is required
}