using Api.Models;

namespace Api.Services;

/// <summary>
/// Provides methods for validating appointment and animal requests.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates an appointment request.
    /// </summary>
    /// <param name="request">The appointment request to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the result of the validation.</returns>
    Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an appointment request, optionally excluding a specific appointment by ID.
    /// </summary>
    /// <param name="request">The appointment request to validate.</param>
    /// <param name="excludeAppointmentId">The ID of the appointment to exclude from validation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the result of the validation.</returns>
    Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, Guid? excludeAppointmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a request to create a new animal.
    /// </summary>
    /// <param name="animal">The animal creation request to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the result of the validation.</returns>
    Task<ValidationResult> ValidateAnimalAsync(CreateAnimalRequest animal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a request to list appointments for a veterinarian.
    /// </summary>
    /// <param name="request">The request to list vet appointments.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the result of the validation.</returns>
    Task<ValidationResult> ValidateGetAppointmentsForVetRequestAsync(ListVetAppointmentsRequest request, CancellationToken cancellationToken = default);
}
