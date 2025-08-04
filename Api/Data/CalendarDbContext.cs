using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class CalendarDbContext : DbContext
{
    public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options) { }

    public DbSet<Animal> Animals { get; set; }

    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed Animals
        modelBuilder.Entity<Animal>().HasData(
            AnimalData.Animals.Select(a => new Animal
            {
                Id = a.Id,
                Name = a.Name,
                BirthDate = a.BirthDate,
                OwnerId = a.OwnerId,
                OwnerName = a.OwnerName,
                OwnerEmail = a.OwnerEmail
            }).ToArray()
        );

        // Seed Appointments
        modelBuilder.Entity<Appointment>().HasData(
            AppointmentData.Appointments.Select(ap => new Appointment
            {
                Id = ap.Id,
                StartTime = ap.StartTime,
                EndTime = ap.EndTime,
                Animal = null!,
                AnimalId = ap.AnimalId, // Navigation property to Animal is not seeded directly
                CustomerId = ap.CustomerId,
                VeterinarianId = ap.VeterinarianId,
                Status = ap.Status,
                Notes = ap.Notes
            }).ToArray()
        );
    }
}
