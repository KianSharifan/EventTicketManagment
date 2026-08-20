using System.IdentityModel.Tokens.Jwt;
using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet("today")]
    public async Task<IActionResult> GetUpcomingEvents()
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid event ID format");

            var e = await _context.Events.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (e == null)
                return NotFound("No such event");

            return Ok(e);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> CreateEvent([FromBody] EventDto eventDto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(eventDto.Title) && eventDto.StartDate != null && !string.IsNullOrWhiteSpace(eventDto.VenueId) && !string.IsNullOrWhiteSpace(eventDto.CategoryId))
            {
                var organizerId= User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(organizerId))
                    return Unauthorized();
                if (!ObjectId.TryParse(eventDto.CategoryId, out _))
                    return BadRequest("Invalid category ID format"); 
                var c = await _context.EventCategories.Find(c => c.Id == eventDto.CategoryId).FirstOrDefaultAsync();
                if (c == null)
                    return NotFound("Category not found");
                if (!ObjectId.TryParse(eventDto.VenueId, out _))
                    return BadRequest("Invalid venue ID format");
                var v = await _context.Venues.Find(v => v.Id == eventDto.VenueId).FirstOrDefaultAsync();
                if (v == null)
                    return NotFound("Venue not found");

                var createdEvent = new Event
                {
                    Title = eventDto.Title!,
                    Description = eventDto.Description,
                    EventDate = eventDto.StartDate!.Value,
                    EventCategoryId = c.Id,
                    VenueId = v.Id,
                    CreatedAt = DateTime.UtcNow,
                    OrganizerId = organizerId
                };
                
                await _context.Events.InsertOneAsync(createdEvent);
                return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
            }
            return BadRequest("Not Valid Inputs!");
        }
        catch (Exception)
        {
            return StatusCode(500,"An unexpected error occured");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> UpdateEvent(string id,[FromBody] EventDto eventDto)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
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
                if (!ObjectId.TryParse(eventDto.VenueId, out _))
                    return BadRequest("Invalid Venue ID format");
                var v = await _context.Venues.Find(v => v.Id == eventDto.VenueId).FirstOrDefaultAsync();
                if (v == null)
                    return NotFound("No such venue"); 
                e.VenueId = eventDto.VenueId;
            }
            if (!string.IsNullOrWhiteSpace(eventDto.CategoryId))
            {
                if (!ObjectId.TryParse(eventDto.CategoryId, out _))
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid event ID format");
        
        var result = await _context.Events.DeleteOneAsync(x => x.Id == id);
        if(result.DeletedCount == 0)
            return NotFound("No such event");

        return NoContent();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetEventCategories()
    {
        try
        {
            var list = await _context.EventCategories.Find(_ => true).ToListAsync();
            return Ok(list);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetEventCategory(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid event ID format");
        
            var ec = await _context.EventCategories.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (ec == null)
                return NotFound("No such event category");
            return Ok(ec);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEventCategory([FromBody] EventCategoryDto categoryDto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                if (await _context.EventCategories.Find(c => c.Name.ToLower() == categoryDto.Name!.ToLower())
                        .FirstOrDefaultAsync() != null)
                    return BadRequest("Category already exists");

                var ec = new EventCategory
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };
                await _context.EventCategories.InsertOneAsync(ec);
                return CreatedAtAction(nameof(GetEventCategory),new { id = ec.Id }, ec);
            }
            return BadRequest("Not Valid Inputs");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEventCategory(string id, [FromBody] EventCategoryDto categoryDto)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid event ID format");
            var ec = await _context.EventCategories.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (ec == null)
                return NotFound("No such event category");
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                if(_context.EventCategories.Find(c => c.Name.ToLower() == categoryDto.Name.ToLower()) != null)
                    return BadRequest("Category already exists");
                ec.Name = categoryDto.Name;
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Description))
                ec.Description = categoryDto.Description;
            await _context.EventCategories.ReplaceOneAsync(e => e.Id == id, ec);
            return Ok(ec);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEventCategory(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid event ID format"); 
        var result = await _context.EventCategories.DeleteOneAsync(e => e.Id == id);
        if (result.DeletedCount == 0)
            return NotFound("No such event category");
        return NoContent();
    }
}