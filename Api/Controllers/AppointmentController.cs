using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly CalendarDbContext dbContext;
    private readonly ILogger<AppointmentController> logger;

    public AppointmentController(CalendarDbContext dbContext, ILogger<AppointmentController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest createAppointmentRequest, CancellationToken cancellationToken)
    {
        if (createAppointmentRequest == null)
        {
            return BadRequest("Appointment cannot be null.");
        }
        if (createAppointmentRequest.AnimalId == Guid.Empty)
        {
            return BadRequest("AnimalId is required.");
        }
        if (createAppointmentRequest.CustomerId == Guid.Empty)
        {
            return BadRequest("CustomerId is required.");
        }
        if (createAppointmentRequest.StartTime == default || createAppointmentRequest.EndTime == default)
        {
            return BadRequest("StartTime and EndTime are required.");
        }
        if (createAppointmentRequest.StartTime >= createAppointmentRequest.EndTime)
        {
            return BadRequest("EndTime must be after StartTime.");
        }
        if (createAppointmentRequest.VeterinarianId == Guid.Empty)
        {
            return BadRequest("VeterinarianId is required.");
        }

        //load animal to save on db hit
        var animal = await dbContext.Animals.FirstOrDefaultAsync(x => x.Id == createAppointmentRequest.AnimalId, cancellationToken: cancellationToken);

        if (animal is null)
        {
            return BadRequest("AnimalId does not exist.");
        }
        //load conflictingAppointments before hands save a query to db and consequent context switch due to async
        var conflictingAppointments = await dbContext.Appointments
            .Where(a => a.StartTime < createAppointmentRequest.EndTime && a.EndTime > createAppointmentRequest.StartTime && a.Animal.Id == createAppointmentRequest.AnimalId)
            .Select(x =>
                new
                {
                    IsAnimalBusy = x.Animal.Id == createAppointmentRequest.AnimalId,
                    IsVetBusy = x.VeterinarianId == createAppointmentRequest.VeterinarianId
                })
            .ToArrayAsync(cancellationToken: cancellationToken);

        if (conflictingAppointments.Any(x => x.IsAnimalBusy))
        {
            return BadRequest("The animal already has an appointment during this time.");
        }
        if (conflictingAppointments.Any(x => x.IsVetBusy))
        {
            return BadRequest("The Veterinarian already has an appointment during this time.");
        }

        //automapper or other similar libraries can avoid manual mapping
        var appointment = new Appointment()
        {
            Id = Guid.Empty,
            Animal = animal,
            CustomerId = createAppointmentRequest.CustomerId,
            StartTime = createAppointmentRequest.StartTime,
            EndTime = createAppointmentRequest.EndTime,
            VeterinarianId = createAppointmentRequest.VeterinarianId,
            Notes = createAppointmentRequest.Notes,
            Status = createAppointmentRequest.Status
        };

        //no async here, Id should be created on save by the db engine
        dbContext.Appointments.Add(appointment);

        await dbContext.SaveChangesAsync(cancellationToken);

        //automapper or similar can helps here. otherwise, an helper class to avoid repetition is recommendable
        var appointmentResponse = new AppointmentResponse(
            appointment.Id,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Animal.Id,
            appointment.CustomerId,
            appointment.VeterinarianId,
            appointment.Status,
            appointment.Notes
            );


        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentResponse);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var appointmentResponse = await dbContext.Appointments.Where(a => a.Id == id)
            .Select(x => new AppointmentResponse(
                x.Id,
                x.StartTime,
                x.EndTime,
                x.Animal.Id,
                x.CustomerId,
                x.VeterinarianId,
                x.Status,
                x.Notes
                ))
            .FirstOrDefaultAsync(cancellationToken);

        if (appointmentResponse == null)
        {
            return NotFound();
        }
        return Ok(appointmentResponse);
    }

    [HttpGet("vet")]
    public async Task<ActionResult<IEnumerable<GetAppointmentsForVetResponse>>> GetAppointmentsForVet([FromQuery] GetAppointmentsForVetRequest request, CancellationToken cancellationToken)
    {
        if (request.VetId == Guid.Empty)
            return BadRequest("VetId is required.");
        if (request.StartDate == default || request.EndDate == default)
            return BadRequest("StartDate and EndDate are required.");
        if (request.StartDate > request.EndDate)
            return BadRequest("StartDate must be before EndDate.");

        var appointments = await dbContext.Appointments
            .Where(a => a.VeterinarianId == request.VetId && a.StartTime >= request.StartDate && a.StartTime <= request.EndDate)
            .Select(a => new GetAppointmentsForVetResponse(a.StartTime, a.EndTime, a.Animal.Name, a.Animal.OwnerName, a.Status))
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }
}