using Api.Controllers;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Controllers;

public class AppointmentControllerTests
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
    public async Task CreateAppointment_ReturnsCreated_WhenValid()
    {
        var dbContext = GetDbContext(nameof(CreateAppointment_ReturnsCreated_WhenValid));
        var animal = SeedAnimal(dbContext);
        var controller = new AppointmentController(dbContext);
        var request = new CreateAppointmentRequest(
            Guid.Empty,
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );
        var result = await controller.CreateAppointment(request, CancellationToken.None);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<AppointmentResponse>(createdResult.Value);
        Assert.Equal(request.AnimalId, response.AnimalId);
        Assert.Equal(request.CustomerId, response.CustomerId);
        Assert.Equal(request.VeterinarianId, response.VeterinarianId);
        Assert.Equal(request.Status, response.Status);
    }

    [Fact]
    public async Task CreateAppointment_ReturnsBadRequest_WhenAnimalBusy()
    {
        var dbContext = GetDbContext(nameof(CreateAppointment_ReturnsBadRequest_WhenAnimalBusy));
        var animal = SeedAnimal(dbContext);
        var vetId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Animal = animal,
            AnimalId = animal.Id,
            CustomerId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            VeterinarianId = vetId,
            Status = AppointmentStatus.Scheduled,
            Notes = ""
        };
        dbContext.Appointments.Add(appointment);
        dbContext.SaveChanges();
        var controller = new AppointmentController(dbContext);
        var request = new CreateAppointmentRequest(
            Guid.Empty,
            appointment.StartTime.AddMinutes(30),
            appointment.EndTime.AddMinutes(30),
            animal.Id,
            Guid.NewGuid(),
            vetId,
            AppointmentStatus.Scheduled,
            "Routine checkup"
        );
        var result = await controller.CreateAppointment(request, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("The animal already has an appointment during this time.", badRequest.Value);
    }

    [Fact]
    public async Task GetAppointment_ReturnsAppointment_WhenExists()
    {
        var dbContext = GetDbContext(nameof(GetAppointment_ReturnsAppointment_WhenExists));
        var animal = SeedAnimal(dbContext);
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Animal = animal,
            AnimalId = animal.Id,
            CustomerId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            VeterinarianId = Guid.NewGuid(),
            Status = AppointmentStatus.Scheduled,
            Notes = "Checkup"
        };
        dbContext.Appointments.Add(appointment);
        dbContext.SaveChanges();
        var controller = new AppointmentController(dbContext);
        var result = await controller.GetAppointment(appointment.Id, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AppointmentResponse>(okResult.Value);
        Assert.Equal(appointment.Id, response.Id);
    }

    [Fact]
    public async Task GetAppointment_ReturnsNotFound_WhenNotExists()
    {
        var dbContext = GetDbContext(nameof(GetAppointment_ReturnsNotFound_WhenNotExists));
        var controller = new AppointmentController(dbContext);
        var result = await controller.GetAppointment(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAppointmentsForVet_ReturnsAppointments()
    {
        var dbContext = GetDbContext(nameof(GetAppointmentsForVet_ReturnsAppointments));
        var animal = SeedAnimal(dbContext);
        var vetId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Animal = animal,
            AnimalId = animal.Id,
            CustomerId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            VeterinarianId = vetId,
            Status = AppointmentStatus.Scheduled,
            Notes = "Checkup"
        };
        dbContext.Appointments.Add(appointment);
        dbContext.SaveChanges();
        var controller = new AppointmentController(dbContext);
        var request = new GetAppointmentsForVetRequest(
            vetId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        );
        var result = await controller.GetAppointmentsForVet(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var appointments = Assert.IsAssignableFrom<IEnumerable<GetAppointmentsForVetResponse>>(okResult.Value);
        Assert.Single(appointments);
        Assert.Equal(animal.Name, appointments.First().AnimalName);
    }
}
