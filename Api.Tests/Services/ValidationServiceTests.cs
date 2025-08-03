using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Services;

public class ValidationServiceTests
{
    private CalendarDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CalendarDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var dbContext = new CalendarDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private Animal SeedAnimal(CalendarDbContext dbContext)
    {
        var animal = new Animal
        {
            Id = Guid.NewGuid(),
            Name = "Fluffy",
            BirthDate = DateTime.UtcNow.AddYears(-2),
            OwnerId = Guid.NewGuid(),
            OwnerName = "John Doe",
            OwnerEmail = "john@example.com"
        };
        dbContext.Animals.Add(animal);
        dbContext.SaveChanges();
        return animal;
    }

    [Fact]
    public async Task ValidateAppointmentAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var dbContext = GetDbContext(nameof(ValidateAppointmentAsync_ReturnsSuccess_WhenRequestIsValid));
        var animal = SeedAnimal(dbContext);
        var validationService = new ValidationService(dbContext);

        var request = new BaseAppointmentRequest(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );

        var result = await validationService.ValidateAppointmentAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAppointmentAsync_ReturnsError_WhenAnimalIdIsEmpty()
    {
        var dbContext = GetDbContext(nameof(ValidateAppointmentAsync_ReturnsError_WhenAnimalIdIsEmpty));
        var validationService = new ValidationService(dbContext);

        var request = new BaseAppointmentRequest(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            Guid.Empty, // Invalid AnimalId
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );

        var result = await validationService.ValidateAppointmentAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("AnimalId is required.", result.Errors);
    }

    [Fact]
    public async Task ValidateAppointmentAsync_ReturnsError_WhenStartTimeAfterEndTime()
    {
        var dbContext = GetDbContext(nameof(ValidateAppointmentAsync_ReturnsError_WhenStartTimeAfterEndTime));
        var animal = SeedAnimal(dbContext);
        var validationService = new ValidationService(dbContext);

        var request = new BaseAppointmentRequest(
            DateTime.UtcNow.AddHours(2), // Start time after end time
            DateTime.UtcNow.AddHours(1),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );

        var result = await validationService.ValidateAppointmentAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("EndTime must be after StartTime.", result.Errors);
    }

    [Fact]
    public async Task ValidateAppointmentAsync_ReturnsError_WhenAnimalDoesNotExist()
    {
        var dbContext = GetDbContext(nameof(ValidateAppointmentAsync_ReturnsError_WhenAnimalDoesNotExist));
        var validationService = new ValidationService(dbContext);

        var request = new BaseAppointmentRequest(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            Guid.NewGuid(), // Non-existent AnimalId
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );

        var result = await validationService.ValidateAppointmentAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("AnimalId does not exist.", result.Errors);
    }

    [Fact]
    public async Task ValidateAppointmentAsync_ReturnsError_WhenAnimalHasConflictingAppointment()
    {
        var dbContext = GetDbContext(nameof(ValidateAppointmentAsync_ReturnsError_WhenAnimalHasConflictingAppointment));
        var animal = SeedAnimal(dbContext);
        
        // Create existing appointment
        var existingAppointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Animal = animal,
            AnimalId = animal.Id,
            CustomerId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            VeterinarianId = Guid.NewGuid(),
            Status = AppointmentStatus.Scheduled,
            Notes = "Existing appointment"
        };
        dbContext.Appointments.Add(existingAppointment);
        dbContext.SaveChanges();

        var validationService = new ValidationService(dbContext);

        var request = new BaseAppointmentRequest(
            DateTime.UtcNow.AddHours(1).AddMinutes(30), // Overlapping time
            DateTime.UtcNow.AddHours(2).AddMinutes(30),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Conflicting appointment"
        );

        var result = await validationService.ValidateAppointmentAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("The animal already has an appointment during this time.", result.Errors);
    }

    [Fact]
    public async Task ValidateAnimalAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var dbContext = GetDbContext(nameof(ValidateAnimalAsync_ReturnsSuccess_WhenRequestIsValid));
        var validationService = new ValidationService(dbContext);

        var request = new CreateAnimalRequest(
            "Fido",
            "fido@example.com",
            "Jane Doe",
            DateTime.UtcNow.AddYears(-5),
            Guid.NewGuid()
        );

        var result = await validationService.ValidateAnimalAsync(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAnimalAsync_ReturnsError_WhenNameIsEmpty()
    {
        var dbContext = GetDbContext(nameof(ValidateAnimalAsync_ReturnsError_WhenNameIsEmpty));
        var validationService = new ValidationService(dbContext);

        var request = new CreateAnimalRequest(
            "", // Invalid name
            "fido@example.com",
            "Jane Doe",
            DateTime.UtcNow.AddYears(-5),
            Guid.NewGuid()
        );

        var result = await validationService.ValidateAnimalAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("Animal name is required.", result.Errors);
    }

    [Fact]
    public async Task ValidateAnimalAsync_ReturnsError_WhenOwnerEmailExists()
    {
        var dbContext = GetDbContext(nameof(ValidateAnimalAsync_ReturnsError_WhenOwnerEmailExists));
        var existingAnimal = SeedAnimal(dbContext); // Seeds an animal with "john@example.com"
        var validationService = new ValidationService(dbContext);

        var request = new CreateAnimalRequest(
            "Fido",
            existingAnimal.OwnerEmail, // Duplicate email
            "Jane Doe",
            DateTime.UtcNow.AddYears(-5),
            Guid.NewGuid()
        );

        var result = await validationService.ValidateAnimalAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains("An animal with the same owner email already exists.", result.Errors);
    }
}