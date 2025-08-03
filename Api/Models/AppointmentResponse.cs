using Api.Data;

namespace Api.Models;

public record AppointmentResponse(Guid Id, DateTime StartTime, DateTime EndTime, Guid AnimalId, Guid CustomerId, Guid VeterinarianId, AppointmentStatus Status, string? Notes)
{
    /// <summary>
    /// Implicit conversion from Appointment to AppointmentResponse
    /// </summary>
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
