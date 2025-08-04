using Api.Data;

namespace Api.Models;

/// <inheritdoc />
public record UpdateAppointmentRequest(
    /// <inheritdoc cref="BaseAppointmentRequest.StartTime"/>
    DateTime StartTime,
    /// <inheritdoc cref="BaseAppointmentRequest.EndTime"/>
    DateTime EndTime,
    /// <inheritdoc cref="BaseAppointmentRequest.AnimalId"/>
    Guid AnimalId,
    /// <inheritdoc cref="BaseAppointmentRequest.CustomerId"/>
    Guid CustomerId,
    /// <inheritdoc cref="BaseAppointmentRequest.VeterinarianId"/>
    Guid VeterinarianId,
    /// <inheritdoc cref="BaseAppointmentRequest.Status"/>
    AppointmentStatus Status,
    /// <inheritdoc cref="BaseAppointmentRequest.Notes"/>
    string? Notes)
    : BaseAppointmentRequest(StartTime, EndTime, AnimalId, CustomerId, VeterinarianId, Status, Notes);