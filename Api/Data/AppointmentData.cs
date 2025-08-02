using Api.Models;

namespace Api.Data;

internal static class AppointmentData
{
    internal static List<Appointment> Appointments = new()
    {
        new Appointment
        {
            Id = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
            AnimalId = AnimalData.Animals.First().Id,
            CustomerId = AnimalData.Animals.First().OwnerId,
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Notes = "Vet appointment",
            VeterinarianId = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d481"), //An appointment needs a veterinarian
        },
        new Appointment
        {
            Id = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d480"),
            AnimalId = AnimalData.Animals.First().Id,
            CustomerId = AnimalData.Animals.First().OwnerId,
            Notes = "Follow-up check",
            VeterinarianId = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d481"), //Assuming the same veterinarian for simplicity
            StartTime = DateTime.Now.AddDays(5),
            EndTime = DateTime.Now.AddDays(5).AddHours(1)
        },
    };
}