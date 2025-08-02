namespace Api.Models;

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
    public required Guid Id { get; set; }

    public required DateTime StartTime { get; set; }

    public required DateTime EndTime { get; set; }

    public required Guid AnimalId { get; set; }

    public required Guid CustomerId { get; set; }

    public required Guid VeterinarianId { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public string? Notes { get; set; }
}