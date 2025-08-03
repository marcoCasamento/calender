using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ValidationService : IValidationService
{
    private readonly CalendarDbContext _dbContext;

    public ValidationService(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        return await ValidateAppointmentAsync(request, null, cancellationToken);
    }

    public async Task<ValidationResult> ValidateAppointmentAsync(BaseAppointmentRequest request, Guid? excludeAppointmentId, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        ValidateAppointmentBasicFields(request, result);

        //Sepate validation for database constraints
        await ValidateAppointmentDatabaseConstraintsAsync(request, result, excludeAppointmentId, cancellationToken);

        return result;
    }

    public async Task<ValidationResult> ValidateAnimalAsync(CreateAnimalRequest animal, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };
        ValidateAnimalBasicFields(animal, result);

        // An owner email should be unique.
        var exists = await _dbContext.Animals.AnyAsync(a => a.OwnerEmail == animal.OwnerEmail, cancellationToken);
        if (exists)
        {
            result.AddError("An animal with the same owner email already exists.");
        }
        return result;
    }
    private static void ValidateAnimalBasicFields(CreateAnimalRequest request, ValidationResult result)
    {
        if (request == null)
        {
            result.AddError("Animal cannot be null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            result.AddError("Animal name is required.");
        }

        if (request.OwnerId == Guid.Empty)
        {
            result.AddError("OwnerId is required.");
        }
        if (string.IsNullOrWhiteSpace(request.OwnerEmail))
        {
            result.AddError("Owner email is required.");
        }
    }
    private static void ValidateAppointmentBasicFields(BaseAppointmentRequest request, ValidationResult result)
    {
        if (request.AnimalId == Guid.Empty)
        {
            result.AddError("AnimalId is required.");
        }

        if (request.CustomerId == Guid.Empty)
        {
            result.AddError("CustomerId is required.");
        }

        if (request.VeterinarianId == Guid.Empty)
        {
            result.AddError("VeterinarianId is required.");
        }

        if (request.StartTime == default)
        {
            result.AddError("StartTime is required.");
        }

        if (request.EndTime == default)
        {
            result.AddError("EndTime is required.");
        }

        if (request.StartTime >= request.EndTime)
        {
            result.AddError("EndTime must be after StartTime.");
        }

        // Validate appointment status
        if (!Enum.IsDefined(typeof(AppointmentStatus), request.Status))
        {
            result.AddError("Invalid appointment status.");
        }
    }

    private async Task ValidateAppointmentDatabaseConstraintsAsync(BaseAppointmentRequest request, ValidationResult result, Guid? excludeAppointmentId, CancellationToken cancellationToken)
    {
        // Only proceed with database validation if basic validation passed
        if (!result.IsValid)
            return;

        // Check if animal exists
        var animalExists = await _dbContext.Animals.AnyAsync(x => x.Id == request.AnimalId, cancellationToken);
        if (!animalExists)
        {
            result.AddError("AnimalId does not exist.");
        }

        // Check for conflicting appointments
        var conflictingAppointmentsQuery = _dbContext.Appointments
            .Where(a => a.StartTime < request.EndTime && a.EndTime > request.StartTime)
            .Where(a => a.AnimalId == request.AnimalId || a.VeterinarianId == request.VeterinarianId);

        // Exclude the current appointment if we're updating
        if (excludeAppointmentId.HasValue)
        {
            conflictingAppointmentsQuery = conflictingAppointmentsQuery.Where(a => a.Id != excludeAppointmentId.Value);
        }

        var conflictingAppointments = await conflictingAppointmentsQuery
            .Select(x => new
            {
                IsAnimalBusy = x.AnimalId == request.AnimalId,
                IsVetBusy = x.VeterinarianId == request.VeterinarianId
            })
            .ToArrayAsync(cancellationToken);

        if (conflictingAppointments.Any(x => x.IsAnimalBusy))
        {
            result.AddError("The animal already has an appointment during this time.");
        }

        if (conflictingAppointments.Any(x => x.IsVetBusy))
        {
            result.AddError("The veterinarian already has an appointment during this time.");
        }
    }

    public Task<ValidationResult> ValidateGetAppointmentsForVetRequestAsync(ListVetAppointmentsRequest request, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        if (request.VetId == Guid.Empty)
            result.AddError("VetId is required.");
        if (request.StartDate == default)
            result.AddError("StartDate is required.");
        if (request.EndDate == default)
            result.AddError("EndDate is required.");
        if (request.StartDate > request.EndDate)
            result.AddError("StartDate must be before EndDate.");

        return Task.FromResult(result);
    }
}