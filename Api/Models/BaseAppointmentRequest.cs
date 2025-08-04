using Api.Data;

namespace Api.Models;


/// <summary>
/// Represents the base data required to create or update an appointment.
/// </summary>
public record BaseAppointmentRequest(
    /// <summary>
    /// The start time of the appointment.
    /// </summary>
    DateTime StartTime,

    /// <summary>
    /// The end time of the appointment.
    /// </summary>
    DateTime EndTime,

    /// <summary>
    /// The unique identifier of the animal for the appointment.
    /// </summary>
    Guid AnimalId,

    /// <summary>
    /// The unique identifier of the customer (owner) for the appointment.
    /// </summary>
    Guid CustomerId,

    /// <summary>
    /// The unique identifier of the veterinarian assigned to the appointment.
    /// </summary>
    Guid VeterinarianId,

    /// <summary>
    /// The status of the appointment.
    /// </summary>
    AppointmentStatus Status,

    /// <summary>
    /// Optional notes for the appointment.
    /// </summary>
    string? Notes
);
