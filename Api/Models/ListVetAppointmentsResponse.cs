using Api.Data;

namespace Api.Models;

/// <summary>
/// Represents a response containing details of a veterinary appointment.
/// </summary>
/// <param name="StartTime">The start time of the appointment.</param>
/// <param name="EndTime">The end time of the appointment.</param>
/// <param name="AnimalName">The name of the animal for the appointment.</param>
/// <param name="OwnerName">The name of the animal's owner.</param>
/// <param name="Status">The status of the appointment.</param>
public record ListVetAppointmentsResponse(
    DateTime StartTime,
    DateTime EndTime,
    string AnimalName,
    string OwnerName,
    AppointmentStatus Status
);