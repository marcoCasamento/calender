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
    /// <summary>
    /// Primary key for the appointment.
    /// </summary>
    [Key]
    public required Guid Id { get; set; }

    /// <summary>
    /// Start time of the appointment.
    /// </summary>
    public required DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the appointment.
    /// </summary>
    public required DateTime EndTime { get; set; }

    /// <summary>
    /// The animal associated with the appointment.
    /// </summary>
    public required Animal Animal { get; set; }

    /// <summary>
    /// Foreign key for the animal. Used for seeding and querying.
    /// </summary>
    public Guid AnimalId { get; set; } //ease seed and query. not "required", as this should be handled by the ORM

    /// <summary>
    /// The customer who booked the appointment.
    /// </summary>
    public required Guid CustomerId { get; set; }

    /// <summary>
    /// The veterinarian assigned to the appointment.
    /// </summary>
    public required Guid VeterinarianId { get; set; }

    /// <summary>
    /// Status of the appointment.
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// Optional notes for the appointment.
    /// </summary>
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