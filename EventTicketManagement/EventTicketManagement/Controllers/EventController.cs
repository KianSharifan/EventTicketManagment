using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[Route("api/events")]
[ApiController]
public class EventController : Controller
{
    private readonly MongoDbContext _context;

    public EventController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        return Ok(await _context.Events.Find(_ => true).ToListAsync());
    }

    [HttpGet("/today")]
    public async Task<IActionResult> GetToday()
    {
        return Ok(await _context.Events.Find(e => e.EventDate.TimeOfDay.CompareTo(DateTime.UtcNow.TimeOfDay)>0 
                                                  && (e.EventDate.Date.CompareTo(DateTime.UtcNow.Date)==0 || e.EventDate.Date.CompareTo(DateTime.UtcNow.Date.AddDays(1))==0)).ToListAsync());
    }
    
    
}