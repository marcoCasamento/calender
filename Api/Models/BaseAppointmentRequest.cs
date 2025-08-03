using Api.Data;

namespace Api.Models;

public record BaseAppointmentRequest(DateTime StartTime, DateTime EndTime, Guid AnimalId, Guid CustomerId, Guid VeterinarianId, AppointmentStatus Status, string? Notes);
