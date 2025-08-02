using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    [HttpPost]
    public ActionResult<Appointment> CreateAppointment([FromBody] Appointment appointment)
    {
        if (appointment == null)
        {
            return BadRequest("Appointment cannot be null.");
        }

        if (appointment.AnimalId == Guid.Empty || appointment.CustomerId == Guid.Empty)
        {
            return BadRequest("AnimalId and CustomerId are required.");
        }
        if (appointment.StartTime == default || appointment.EndTime == default)
        {
            return BadRequest("StartTime and EndTime are required.");
        }
        if (appointment.StartTime >= appointment.EndTime)
        {
            return BadRequest("EndTime must be after StartTime.");
        }
        if (appointment.VeterinarianId == Guid.Empty)
        {
            return BadRequest("VeterinarianId is required.");
        }
        if (AnimalData.Animals.All(a => a.Id != appointment.AnimalId))
        {
            return BadRequest("AnimalId does not exist.");
        }
        if (AppointmentData.Appointments.Any(a => a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime && a.AnimalId == appointment.AnimalId))
        {
            return BadRequest("The animal already has an appointment during this time.");
        }
        if (AppointmentData.Appointments.Any(a => a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime && a.VeterinarianId == appointment.VeterinarianId))
        {
            return BadRequest("The Veterinarian already has an appointment during this time.");
        }

        appointment.Id = Guid.NewGuid();

        AppointmentData.Appointments.Add(appointment);

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    [HttpGet("{id}")]
    public ActionResult<Appointment> GetAppointment(Guid id)
    {
        var appointment = AppointmentData.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
        {
            return NotFound();
        }
        return Ok(appointment);
    }
}