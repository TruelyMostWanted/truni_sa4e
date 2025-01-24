using Microsoft.AspNetCore.Mvc;
using XmasWishes.Models.persons;

namespace XmasWishes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly PersonDbContext _context;

    // Dependency Injection des DbContext
    public PersonController(PersonDbContext context)
    {
        _context = context;
    }

    // HTTP GET: api/person
    [HttpGet]
    public ActionResult<IEnumerable<Person>> GetAllPersons()
    {
        // Abrufen aller Einträge aus der Tabelle
        var persons = _context.Persons.ToList();

        // Rückgabe als JSON
        return Ok(persons);
    }
}