using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[Route("api/[controller]")]
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
        try
        {
            return Ok(await _context.Events.Find(_ => true).ToListAsync());

        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpGet("/today")]
    public async Task<IActionResult> GetTodayEvents()
    {
        try
        {
            return Ok(await _context.Events.Find(e => e.EventDate.TimeOfDay.CompareTo(DateTime.UtcNow.TimeOfDay)>0 
                                                      && (e.EventDate.Date.CompareTo(DateTime.UtcNow.Date)==0 || e.EventDate.Date.CompareTo(DateTime.UtcNow.Date.AddDays(1))==0)).ToListAsync());
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    //should be tested later with the authentication
    public async Task<IActionResult> CreateEvent([FromBody] EventDto eventDto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(eventDto.Title) || eventDto.StartDate == null || !string.IsNullOrWhiteSpace(eventDto.VenueId)|| !string.IsNullOrWhiteSpace(eventDto.CategoryId))
            {
                //a way to get the id of the organizer from the jwt token
                // string organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                
                // return CreatedAtAction()
            }
            return BadRequest("Not Valid Inputs!");
        }
        catch (Exception)
        {
            return StatusCode(500,"An unexpected error occured");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(string id,[FromBody] EventDto eventDto)
    {
        try
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                return BadRequest("Invalid event ID format");
            var e = await _context.Events.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (e == null)
                return NotFound("No such event");
            if (!string.IsNullOrWhiteSpace(eventDto.Title))
            {
                bool exists = await _context.Events
                    .Find(ev => ev.Title == eventDto.Title && ev.Id != id)
                    .AnyAsync();
                if (exists)
                {
                    return BadRequest("A event with that title already exists");
                }
                e.Title = eventDto.Title;
            }
            if (!string.IsNullOrWhiteSpace(eventDto.Description))
            {
                e.Description = eventDto.Description;
            }
            if (eventDto.StartDate != null)
            {
                if(eventDto.StartDate < DateTime.UtcNow)
                    return BadRequest("Start date cannot be in the past");
                e.EventDate = eventDto.StartDate.Value;
            }
            if (!string.IsNullOrWhiteSpace(eventDto.VenueId))
            {
                if (!MongoDB.Bson.ObjectId.TryParse(eventDto.VenueId, out _))
                    return BadRequest("Invalid Venue ID format");
                var v = await _context.Venues.Find(v => v.Id == eventDto.VenueId).FirstOrDefaultAsync();
                if (v == null)
                    return NotFound("No such venue"); 
                e.VenueId = eventDto.VenueId;
            }
            if (!string.IsNullOrWhiteSpace(eventDto.CategoryId))
            {
                if (!MongoDB.Bson.ObjectId.TryParse(eventDto.CategoryId, out _))
                    return BadRequest("Invalid Category ID format");
                var c = await _context.EventCategories.Find(c => c.Id == eventDto.CategoryId).FirstOrDefaultAsync();
                if (c == null)
                    return NotFound("No such Category"); 
                e.EventCategoryId = eventDto.CategoryId;
            }
            await _context.Events.ReplaceOneAsync(ev => ev.Id == id,e);
            return Ok(e);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}