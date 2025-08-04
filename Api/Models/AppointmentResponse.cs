using Api.Data;

namespace Api.Models;

/// <summary>
/// Represents a response model for an appointment.
/// </summary>
public record AppointmentResponse(
    /// <summary>
    /// Unique identifier for the appointment.
    /// </summary>
    Guid Id,
    /// <summary>
    /// Start time of the appointment.
    /// </summary>
    DateTime StartTime,
    /// <summary>
    /// End time of the appointment.
    /// </summary>
    DateTime EndTime,
    /// <summary>
    /// Unique identifier of the animal associated with the appointment.
    /// </summary>
    Guid AnimalId,
    /// <summary>
    /// Unique identifier of the customer who booked the appointment.
    /// </summary>
    Guid CustomerId,
    /// <summary>
    /// Unique identifier of the veterinarian assigned to the appointment.
    /// </summary>
    Guid VeterinarianId,
    /// <summary>
    /// Status of the appointment.
    /// </summary>
    AppointmentStatus Status,
    /// <summary>
    /// Optional notes for the appointment.
    /// </summary>
    string? Notes)
{
    /// <summary>
    /// Implicit conversion from Appointment to AppointmentResponse.
    /// </summary>
    /// <param name="appointment">The Appointment entity to convert.</param>
    /// <returns>An AppointmentResponse containing the appointment details.</returns>
    public static implicit operator AppointmentResponse(Appointment appointment)
    {
        return new AppointmentResponse(
            appointment.Id,
            appointment.StartTime,
            appointment.EndTime,
            appointment.AnimalId,
            appointment.CustomerId,
            appointment.VeterinarianId,
            appointment.Status,
            appointment.Notes
        );
    }
}
