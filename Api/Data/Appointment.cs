using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.Data;

public enum AppointmentStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public class Appointment
{
    [Key]
    public required Guid Id { get; set; }

    public required DateTime StartTime { get; set; }

    public required DateTime EndTime { get; set; }
    public required Animal Animal { get; set; }
    public Guid AnimalId { get; set; } //ease seed and query. not "required", as this should be handled by the ORM
    public required Guid CustomerId { get; set; }

    public required Guid VeterinarianId { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Implicit conversion from CreateAppointmentRequest to Appointment
    /// </summary>
    public static implicit operator Appointment(CreateAppointmentRequest request)
    {
        return new Appointment
        {
            Id = Guid.Empty, // Will be set by the database
            Animal = null!, // Will be loaded by the ORM
            AnimalId = request.AnimalId,
            CustomerId = request.CustomerId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            VeterinarianId = request.VeterinarianId,
            Notes = request.Notes,
            Status = request.Status
        };
    }
}