using Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record CreateAppointmentRequest(Guid Id, DateTime StartTime, DateTime EndTime, Guid AnimalId, Guid CustomerId, Guid VeterinarianId, AppointmentStatus Status, string? Notes);
//{
//    public CreateAppointmentRequest(Guid id, DateTime startTime, DateTime endTime, Guid animalId, Guid customerId, Guid veterinarianId, AppointmentStatus status, string? notes)
//    {
//        Id = id;
//        StartTime = startTime;
//        EndTime = endTime;
//        AnimalId = animalId;
//        CustomerId = customerId;
//        VeterinarianId = veterinarianId;
//        Status = status;
//        Notes = notes;
//    }

//    public required Guid Id { get; set; }

//    public required DateTime StartTime { get; set; }

//    public required DateTime EndTime { get; set; }
//    public required Guid AnimalId { get; set; } 
//    public required Guid CustomerId { get; set; }

//    public required Guid VeterinarianId { get; set; }

//    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
//    [MaxLength(500)]
//    public string? Notes { get; set; }
//}
