using Api.ControllersModel;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentData _appointmentData;
    private readonly IAnimalData _animalData;

    public AppointmentController(IAppointmentData appointmentData, IAnimalData animalData)
    {
        _appointmentData = appointmentData;
        _animalData = animalData;
    }

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
        if (_animalData.Animals.All(a => a.Id != appointment.AnimalId))
        {
            return BadRequest("AnimalId does not exist.");
        }
        if (_appointmentData.Appointments.Any(a => a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime && a.AnimalId == appointment.AnimalId))
        {
            return BadRequest("The animal already has an appointment during this time.");
        }
        if (_appointmentData.Appointments.Any(a => a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime && a.VeterinarianId == appointment.VeterinarianId))
        {
            return BadRequest("The Veterinarian already has an appointment during this time.");
        }

        appointment.Id = Guid.NewGuid();

        _appointmentData.Appointments.Add(appointment);

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    [HttpGet("{id}")]
    public ActionResult<Appointment> GetAppointment(Guid id)
    {
        var appointment = _appointmentData.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
        {
            return NotFound();
        }
        return Ok(appointment);
    }

    [HttpGet("vet")]
    public ActionResult<IEnumerable<GetAppointmentsForVetResponse>> GetAppointmentsForVet([FromQuery] GetAppointmentsForVetRequest request)
    {
        if (request.VetId == Guid.Empty)
            return BadRequest("VetId is required.");
        if (request.StartDate == default || request.EndDate == default)
            return BadRequest("StartDate and EndDate are required.");
        if (request.StartDate > request.EndDate)
            return BadRequest("StartDate must be before EndDate.");

        var appointments = _appointmentData.Appointments
            .Where(a => a.VeterinarianId == request.VetId && a.StartTime >= request.StartDate && a.StartTime <= request.EndDate)
            .Select(a => {
                var animal = _animalData.Animals.FirstOrDefault(an => an.Id == a.AnimalId);
                return new GetAppointmentsForVetResponse
                {
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    AnimalName = animal?.Name ?? "Unknown",
                    OwnerName = animal?.OwnerName ?? "Unknown",
                    Status = a.Status.ToString()
                };
            })
            .ToList();

        return Ok(appointments);
    }
}
