using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly CalendarDbContext dbContext;
    private readonly IValidationService validationService;

    public AppointmentController(CalendarDbContext dbContext, IValidationService validationService)
    {
        this.dbContext = dbContext;
        this.validationService = validationService;
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest createAppointmentRequest, CancellationToken cancellationToken)
    {
        if (createAppointmentRequest == null)
        {
            return BadRequest("Appointment cannot be null.");
        }

        var validationResult = await validationService.ValidateAppointmentAsync(createAppointmentRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        //implicit operator conversion
        Appointment appointment = createAppointmentRequest;

        //no async here, Id should be created on save by the db engine
        dbContext.Appointments.Add(appointment);

        await dbContext.SaveChangesAsync(cancellationToken);

        //implicit operator conversion 
        AppointmentResponse appointmentResponse = appointment;

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentResponse);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var appointmentResponse = await dbContext.Appointments.Where(a => a.Id == id)
            .Cast<AppointmentResponse>() //implicit operator conversion
            .FirstOrDefaultAsync(cancellationToken);

        if (appointmentResponse == null)
        {
            return NotFound();
        }
        return Ok(appointmentResponse);
    }

    [HttpGet("vet")]
    public async Task<ActionResult<IEnumerable<ListVetAppointmentsResponse>>> ListVetAppointments([FromQuery] ListVetAppointmentsRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validationService.ValidateGetAppointmentsForVetRequestAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        var appointments = await dbContext.Appointments
            .Where(a => a.VeterinarianId == request.VetId && a.StartTime >= request.StartDate && a.StartTime <= request.EndDate)
            .Select(a => new ListVetAppointmentsResponse(a.StartTime, a.EndTime, a.Animal.Name, a.Animal.OwnerName, a.Status))
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }

    //TODO: this method should be restricted to admins or vets only.
    //That would require an authorization mechanism. Built-in RBAC is probably fine, with roles like Admin, Vet, and Customer.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequest request, [FromServices] IMailSender mailSender, CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .Include(a => a.Animal)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (appointment == null)
            return NotFound();

        var validationResult = await validationService.ValidateAppointmentAsync(request, id, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        //TODO: those checks should be moved to the validation service

        // Only allow valid status values
        if (request.Status != AppointmentStatus.Scheduled && request.Status != AppointmentStatus.Completed && request.Status != AppointmentStatus.Cancelled)
            return BadRequest($"Status {request.Status} is not valid. Valid status are Scheduled, Completed, Cancelled");

        // Prevent cancellation within 1 hour of StartTime
        if (appointment.Status != AppointmentStatus.Cancelled && request.Status == AppointmentStatus.Cancelled)
        {
            var now = DateTime.UtcNow;
            if (appointment.StartTime <= now.AddHours(1) && appointment.StartTime > now)
                return BadRequest("Cannot cancel within 1 hour of scheduled start time.");
        }

        if (
            appointment.StartTime == request.StartTime &&
            appointment.EndTime == request.EndTime &&
            appointment.AnimalId == request.AnimalId &&
            appointment.CustomerId == request.CustomerId &&   //TODO: question, should I allow a customer to be changed ?
            appointment.VeterinarianId == request.VeterinarianId &&
            appointment.Status == request.Status &&
            appointment.Notes == request.Notes
            )
        {
            //nothing to update, log it and return NoContent
            return NoContent();
        }
        bool appointmentIsBeingCancelled = appointment.Status != AppointmentStatus.Cancelled && request.Status == AppointmentStatus.Cancelled;

        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;
        appointment.AnimalId = request.AnimalId;
        appointment.CustomerId = request.CustomerId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.Status = request.Status;
        appointment.Notes = request.Notes;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Send email if cancelled
        if (appointmentIsBeingCancelled)
        {
            var ownerEmail = appointment.Animal.OwnerEmail;
            if (!string.IsNullOrWhiteSpace(ownerEmail))
            {
                var mailMessage = new System.Net.Mail.MailMessage("noreply@calendar.local", ownerEmail)
                {
                    Subject = "Appointment Cancelled",
                    Body = $"Dear {appointment.Animal.OwnerName}, your appointment for {appointment.Animal.Name} on {appointment.StartTime:f} has been cancelled."
                };
                await mailSender.SendMailAsync(mailMessage, cancellationToken);
            }
        }

        return NoContent();
    }
}