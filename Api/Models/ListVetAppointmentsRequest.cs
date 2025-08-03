using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record ListVetAppointmentsRequest(
    [Required]
    Guid VetId, 
    [Required]
    DateTime StartDate, 
    [Required]
    DateTime EndDate);
