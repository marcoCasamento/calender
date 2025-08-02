using Api.Data;

namespace Api.Models;

public record GetAppointmentsForVetResponse(DateTime StartTime, DateTime EndTime, string AnimalName, string OwnerName, AppointmentStatus Status);