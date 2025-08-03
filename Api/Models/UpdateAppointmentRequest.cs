using Api.Data;

namespace Api.Models;

public record UpdateAppointmentRequest(DateTime StartTime, DateTime EndTime, Guid AnimalId, Guid CustomerId, Guid VeterinarianId, AppointmentStatus Status, string? Notes)
    : BaseAppointmentRequest(StartTime, EndTime, AnimalId, CustomerId, VeterinarianId, Status, Notes);