using Api.Data;

namespace Api.Models;

public record ListVetAppointmentsResponse(DateTime StartTime, DateTime EndTime, string AnimalName, string OwnerName, AppointmentStatus Status);