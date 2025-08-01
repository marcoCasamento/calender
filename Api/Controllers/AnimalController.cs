using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    [HttpPost]
    public ActionResult<Animal> CreateAnimal([FromBody] Animal animal)
    {
        if (animal == null)
        {
            return BadRequest("Animal cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(animal.Name))
        {
            return BadRequest("Animal name is required.");
        }

        animal.Id = Guid.NewGuid();

        AnimalData.Animals.Add(animal);

        return CreatedAtAction(nameof(GetAnimal), new { id = animal.Id }, animal);
    }

    [HttpGet("{id}")]
    public ActionResult<Animal> GetAnimal(Guid id)
    {
        var animal = AnimalData.Animals.FirstOrDefault(a => a.Id == id);
        return Ok(animal);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAnimal(Guid id)
    {
        var animal = AnimalData.Animals.FirstOrDefault(a => a.Id == id);
        if (animal == null)
        {
            return NotFound("Animal not found.");
        }

        AnimalData.Animals.Remove(animal);
        return NoContent();
    }
}