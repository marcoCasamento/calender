namespace Api.Models;

public record GetAppointmentsForVetRequest(Guid VetId, DateTime StartDate, DateTime EndDate);
