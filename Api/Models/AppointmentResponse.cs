using Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record AppointmentResponse(Guid Id, DateTime StartTime, DateTime EndTime, Guid AnimalId, Guid CustomerId, Guid VeterinarianId, AppointmentStatus Status, string? Notes);
