using Api.Controllers;
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Controllers
{
    public class AnimalControllerTests
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

        private AnimalController GetController(CalendarDbContext dbContext)
        {
            var validationService = new ValidationService(dbContext);
            return new AnimalController(dbContext, validationService);
        }

        private Animal SeedAnimal(CalendarDbContext dbContext, string ownerEmail = "owner@example.com")
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid(),
                Name = "Test Animal",
                BirthDate = DateTime.UtcNow.AddYears(-1),
                OwnerId = Guid.NewGuid(),
                OwnerName = "Test Owner",
                OwnerEmail = ownerEmail
            };
            dbContext.Animals.Add(animal);
            dbContext.SaveChanges();
            return animal;
        }

        [Fact]
        public async Task CreateAnimal_ReturnsCreated_WhenValid()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(CreateAnimal_ReturnsCreated_WhenValid));
            var controller = GetController(dbContext);
            var request = new CreateAnimalRequest(
                "Fido",
                "john.doe@example.com",
                "John Doe",
                DateTime.UtcNow.AddYears(-3),
                Guid.NewGuid()
            );

            // Act
            var result = await controller.CreateAnimal(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<AnimalResponse>(createdResult.Value);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.OwnerEmail, response.OwnerEmail);
        }

        [Fact]
        public async Task GetAnimal_ReturnsAnimal_WhenExists()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(GetAnimal_ReturnsAnimal_WhenExists));
            var animal = SeedAnimal(dbContext);
            var controller = GetController(dbContext);

            // Act
            var result = await controller.GetAnimal(animal.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AnimalResponse>(okResult.Value);
            Assert.Equal(animal.Id, response.Id);
        }

        [Fact]
        public async Task GetAnimal_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(GetAnimal_ReturnsNotFound_WhenNotExists));
            var controller = GetController(dbContext);

            // Act
            var result = await controller.GetAnimal(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(DeleteAnimal_ReturnsNoContent_WhenSuccessful));
            var animal = SeedAnimal(dbContext);
            var controller = GetController(dbContext);

            // Act
            var result = await controller.DeleteAnimal(animal.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedAnimal = await dbContext.Animals.FindAsync(animal.Id);
            Assert.Null(deletedAnimal);
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsNotFound_WhenAnimalNotExists()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(DeleteAnimal_ReturnsNotFound_WhenAnimalNotExists));
            var controller = GetController(dbContext);

            // Act
            var result = await controller.DeleteAnimal(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsConflict_WhenAnimalHasAppointments()
        {
            // Arrange
            var dbContext = GetDbContext(nameof(DeleteAnimal_ReturnsConflict_WhenAnimalHasAppointments));
            var animal = SeedAnimal(dbContext);
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                Animal = animal,
                AnimalId = animal.Id,
                CustomerId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3),
                VeterinarianId = Guid.NewGuid(),
                Status = AppointmentStatus.Scheduled,
                Notes = "Test appointment"
            };
            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync();

            var controller = GetController(dbContext);

            // Act
            var result = await controller.DeleteAnimal(animal.Id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Cannot delete animal because it is referenced by other records.", conflictResult.Value);
        }
    }
}
