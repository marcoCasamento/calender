using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    private readonly CalendarDbContext dbContext;
    private readonly IValidationService validationService;

    public AnimalController(CalendarDbContext dbContext, IValidationService validationService)
    {
        this.dbContext = dbContext;
        this.validationService = validationService;
    }

    [HttpPost]
    public async Task<ActionResult<AnimalResponse>> CreateAnimal([FromBody] CreateAnimalRequest createAnimalRequest)
    {
        var validationResult = await validationService.ValidateAnimalAsync(createAnimalRequest);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        //implicit conversion
        Animal animal = createAnimalRequest;
        dbContext.Animals.Add(animal);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnimal), new { id = animal.Id }, (AnimalResponse)animal);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnimalResponse>> GetAnimal(Guid id)
    {
        var animal = await dbContext.Animals.FindAsync(id);
        if (animal == null)
            return NotFound();
        return Ok((AnimalResponse)animal);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimal(Guid id)
    {
        //there are a few issues with this approach, but I think they are opinable and I'd like to discuss them:
        //1. I'm loading the whole animal with all appointments, which is not necessary for deletion, but is useful to check if the animal has appointments
        //2. I'm validating the request in the controller, which is not the best practice, I should validate in a ValidationService,
        //   but that imply that the ValidationService will make a DB call to check if the animal exists, which is not ideal for performance.
        //   The alternative to stick loading Animal+Appointments here and then pass it to the ValidationService stinks too because it means that the controller
        //   is aware of the ValidationService implementation details and it should not be.
        //3. As explaineed in the comment below, the DB should be the source of truth for referential integrity, but that assumptions breaks immediately
        //   with testability, because the in-memory database does not enforce referential integrity
        var animal = await dbContext.Animals.Include(a => a.Appointments).FirstOrDefaultAsync(a => a.Id == id);
        if (animal == null)
        {
            return NotFound("Animal not found.");
        }

        if (animal.Appointments.Any())
        {
            return Conflict("Cannot delete animal because it is referenced by other records.");
        }

        try
        {
            dbContext.Animals.Remove(animal);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
        //I'm letting the DB handle referential integrity, because I think the DB should be the source of truth (think concurrency)
        //but that means I need to handle the exception here and let DB specific implementation *leaks* into the controller through ORM Abstraction
        //One way to handle it could be to write an interceptor that catches the exception and returns a more generic error message but I let this idea
        //to future improvements
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx)
        {
            if (sqliteEx.SqliteErrorCode == 19) // TODO: check for SQLite foreign key constraint violation, it should be 19 https://www.sqlite.org/rescode.html#constraint
            {
                return Conflict("Cannot delete animal because it is referenced by other records.");
            }
            //TODO: add logging
            throw;
        }

    }
}