using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record CreateAnimalRequest(
    [Required(ErrorMessage = "Animal name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Animal Name length should be between 2 and 50 chars")]
    string Name,

    [Required(ErrorMessage = "Owner email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string OwnerEmail,
    
    string OwnerName,
    
    [Required(ErrorMessage = "Birth date is required")]
    DateTime BirthDate,

    [Required(ErrorMessage = "OwnerId is required")]
    Guid OwnerId
);

