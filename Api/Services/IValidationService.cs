using Api.Models;

namespace Api.Services;

public interface IValidationService
{
    Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, Guid? excludeAppointmentId, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateAnimalAsync(CreateAnimalRequest animal, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateGetAppointmentsForVetRequestAsync(ListVetAppointmentsRequest request, CancellationToken cancellationToken = default);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public ValidationResult()
    {
    }

    public ValidationResult(bool isValid, List<string>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<string>();
    }

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(params string[] errors) => new(false, errors.ToList());
}