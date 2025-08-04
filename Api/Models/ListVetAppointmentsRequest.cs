using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// Request model for listing appointments for a specific veterinarian within a date range.
/// </summary>
/// <param name="VetId">The unique identifier of the veterinarian.</param>
/// <param name="StartDate">The start date of the appointment search range.</param>
/// <param name="EndDate">The end date of the appointment search range.</param>
public record ListVetAppointmentsRequest(
    [Required]
        Guid VetId,
    [Required]
        DateTime StartDate,
    [Required]
        DateTime EndDate);
