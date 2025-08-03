using Api.Controllers;
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

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

    private AppointmentController GetController(CalendarDbContext dbContext)
    {
        var validationService = new ValidationService(dbContext);
        return new AppointmentController(dbContext, validationService);
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

    private class TestMailSender : IMailSender
    {
        public List<MailMessage> SentMessages { get; } = new();

        public Task SendMailAsync(MailMessage mailMessage, CancellationToken cancellationToken)
        {
            SentMessages.Add(mailMessage);
            return Task.CompletedTask;
        }
    }

    private Appointment SeedAppointment(CalendarDbContext dbContext, Animal animal, AppointmentStatus status = AppointmentStatus.Scheduled, DateTime? startTime = null)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Animal = animal,
            AnimalId = animal.Id,
            CustomerId = Guid.NewGuid(),
            StartTime = startTime ?? DateTime.UtcNow.AddHours(2),
            EndTime = startTime?.AddHours(1) ?? DateTime.UtcNow.AddHours(3),
            VeterinarianId = Guid.NewGuid(),
            Status = status,
            Notes = "Test appointment"
        };
        dbContext.Appointments.Add(appointment);
        dbContext.SaveChanges();
        return appointment;
    }

    [Fact]
    public async Task CreateAppointment_ReturnsCreated_WhenValid()
    {
        var dbContext = GetDbContext(nameof(CreateAppointment_ReturnsCreated_WhenValid));
        var animal = SeedAnimal(dbContext);
        var controller = GetController(dbContext);
        var request = new CreateAppointmentRequest(
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
        var controller = GetController(dbContext);
        var request = new CreateAppointmentRequest(
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
        Assert.Contains("The animal already has an appointment during this time", badRequest.Value!.ToString());
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
        var controller = GetController(dbContext);
        var result = await controller.GetAppointment(appointment.Id, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AppointmentResponse>(okResult.Value);
        Assert.Equal(appointment.Id, response.Id);
    }

    [Fact]
    public async Task GetAppointment_ReturnsNotFound_WhenNotExists()
    {
        var dbContext = GetDbContext(nameof(GetAppointment_ReturnsNotFound_WhenNotExists));
        var controller = GetController(dbContext);
        var result = await controller.GetAppointment(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ListVetAppointments_ReturnsAppointments()
    {
        var dbContext = GetDbContext(nameof(ListVetAppointments_ReturnsAppointments));
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
        var controller = GetController(dbContext);
        var request = new ListVetAppointmentsRequest(
            vetId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        );
        var result = await controller.ListVetAppointments(request, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var appointments = Assert.IsAssignableFrom<IEnumerable<ListVetAppointmentsResponse>>(okResult.Value);
        Assert.Single(appointments);
        Assert.Equal(animal.Name, appointments.First().AnimalName);
    }

    [Fact]
    public async Task UpdateAppointment_ReturnsNoContent_WhenValid()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_ReturnsNoContent_WhenValid));
        var animal = SeedAnimal(dbContext);
        var appointment = SeedAppointment(dbContext, animal);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(4),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Completed,
            "Updated notes"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        
        var updatedAppointment = await dbContext.Appointments.FindAsync(appointment.Id);
        Assert.Equal(AppointmentStatus.Completed, updatedAppointment!.Status);
        Assert.Equal("Updated notes", updatedAppointment.Notes);
        Assert.Empty(mailSender.SentMessages); // No email for non-cancellation updates
    }

    [Fact]
    public async Task UpdateAppointment_ReturnsNotFound_WhenAppointmentNotExists()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_ReturnsNotFound_WhenAppointmentNotExists));
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);
        var nonExistentId = Guid.NewGuid();

        var request = new UpdateAppointmentRequest(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.Scheduled,
            "Notes"
        );

        var result = await controller.UpdateAppointment(nonExistentId, request, mailSender, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateAppointment_ReturnsBadRequest_WhenInvalidStatus()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_ReturnsBadRequest_WhenInvalidStatus));
        var animal = SeedAnimal(dbContext);
        var appointment = SeedAppointment(dbContext, animal);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            animal.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentStatus.InProgress, // Invalid status
            "Notes"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Status InProgress is not valid", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task UpdateAppointment_ReturnsBadRequest_WhenCancellingWithinOneHour()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_ReturnsBadRequest_WhenCancellingWithinOneHour));
        var animal = SeedAnimal(dbContext);
        var startTime = DateTime.UtcNow.AddMinutes(30); // Within 1 hour
        var appointment = SeedAppointment(dbContext, animal, AppointmentStatus.Scheduled, startTime);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            appointment.StartTime,
            appointment.EndTime,
            animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            AppointmentStatus.Cancelled,
            "Emergency cancellation"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot cancel within 1 hour of scheduled start time.", badRequest.Value);
        Assert.Empty(mailSender.SentMessages);
    }

    [Fact]
    public async Task UpdateAppointment_SendsEmail_WhenCancelledSuccessfully()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_SendsEmail_WhenCancelledSuccessfully));
        var animal = SeedAnimal(dbContext);
        var startTime = DateTime.UtcNow.AddHours(2); // More than 1 hour away
        var appointment = SeedAppointment(dbContext, animal, AppointmentStatus.Scheduled, startTime);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            appointment.StartTime,
            appointment.EndTime,
            animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            AppointmentStatus.Cancelled,
            "Owner requested cancellation"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        
        var updatedAppointment = await dbContext.Appointments.FindAsync(appointment.Id);
        Assert.Equal(AppointmentStatus.Cancelled, updatedAppointment!.Status);
        
        Assert.Single(mailSender.SentMessages);
        var sentEmail = mailSender.SentMessages.First();
        Assert.Equal(animal.OwnerEmail, sentEmail.To.First().Address);
        Assert.Equal("Appointment Cancelled", sentEmail.Subject);
        Assert.Contains(animal.OwnerName, sentEmail.Body);
        Assert.Contains(animal.Name, sentEmail.Body);
    }

    [Fact]
    public async Task UpdateAppointment_DoesNotSendEmail_WhenOwnerEmailIsEmpty()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_DoesNotSendEmail_WhenOwnerEmailIsEmpty));
        var animal = new Animal
        {
            Id = Guid.NewGuid(),
            Name = "Fido",
            BirthDate = DateTime.UtcNow.AddYears(-2),
            OwnerId = Guid.NewGuid(),
            OwnerName = "Marco Casamento",
            OwnerEmail = "" // Empty email
        };
        dbContext.Animals.Add(animal);
        dbContext.SaveChanges();

        var startTime = DateTime.UtcNow.AddHours(2);
        var appointment = SeedAppointment(dbContext, animal, AppointmentStatus.Scheduled, startTime);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            appointment.StartTime,
            appointment.EndTime,
            animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            AppointmentStatus.Cancelled,
            "Owner requested cancellation"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(mailSender.SentMessages); // No email sent due to empty email
    }

    [Fact]
    public async Task UpdateAppointment_DoesNotSendEmail_WhenAlreadyCancelled()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_DoesNotSendEmail_WhenAlreadyCancelled));
        var animal = SeedAnimal(dbContext);
        var startTime = DateTime.UtcNow.AddHours(2);
        var appointment = SeedAppointment(dbContext, animal, AppointmentStatus.Cancelled, startTime); // Already cancelled
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            appointment.StartTime,
            appointment.EndTime,
            animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            AppointmentStatus.Cancelled, // Still cancelled
            "Updated cancellation reason"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(mailSender.SentMessages); // No email sent since it was already cancelled
    }

    [Fact]
    public async Task UpdateAppointment_AllowsCancellation_WhenStartTimeIsInPast()
    {
        var dbContext = GetDbContext(nameof(UpdateAppointment_AllowsCancellation_WhenStartTimeIsInPast));
        var animal = SeedAnimal(dbContext);
        var startTime = DateTime.UtcNow.AddHours(-1); // Past appointment
        var appointment = SeedAppointment(dbContext, animal, AppointmentStatus.Scheduled, startTime);
        var mailSender = new TestMailSender();
        var controller = GetController(dbContext);

        var request = new UpdateAppointmentRequest(
            appointment.StartTime,
            appointment.EndTime,
            animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            AppointmentStatus.Cancelled,
            "Late cancellation"
        );

        var result = await controller.UpdateAppointment(appointment.Id, request, mailSender, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        
        var updatedAppointment = await dbContext.Appointments.FindAsync(appointment.Id);
        Assert.Equal(AppointmentStatus.Cancelled, updatedAppointment!.Status);
    }
}
