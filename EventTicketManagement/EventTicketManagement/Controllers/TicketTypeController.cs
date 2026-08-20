using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketTypeController : Controller
{
    private readonly MongoDbContext _context;

    public TicketTypeController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEventTicketTypes()
    {
        try
        {
            return Ok(await _context.TicketTypes.Find(_ => true).ToListAsync());
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketTypeById(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ticket type ID format");

            var ticketType = await _context.TicketTypes.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (ticketType == null)
                return NotFound("No such ticket type");

            return Ok(ticketType);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpGet("event/{id}")]
    public async Task<IActionResult> GetEventTicketTypes(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid event ID format"); 
            
            var e = _context.Events.Find(x => x.Id == id).FirstOrDefault();
            if (e == null)
                return NotFound("No such event");
            
            var types = await _context.TicketTypes.Find(t => t.EventId == id).ToListAsync();
            
            return Ok(types);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> CreateTicketType([FromBody] TicketTypeDto ticketTypeDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ticketTypeDto.EventId) || string.IsNullOrWhiteSpace(ticketTypeDto.Name) ||
                ticketTypeDto.Price == null || ticketTypeDto.TotalCapacity == null)
                return BadRequest("Not Valid Inputs!");

            if (!ObjectId.TryParse(ticketTypeDto.EventId, out _))
                return BadRequest("Invalid event ID format");

            var eventExists = await _context.Events.Find(x => x.Id == ticketTypeDto.EventId).AnyAsync();
            if (!eventExists)
                return NotFound("No such event");

            if (ticketTypeDto.Price < 0)
                return BadRequest("Price cannot be negative");

            if (ticketTypeDto.TotalCapacity == 0)
                return BadRequest("Total capacity must be greater than zero");

            var ticketType = new TicketType
            {
                EventId = ticketTypeDto.EventId,
                Name = ticketTypeDto.Name,
                Price = ticketTypeDto.Price.Value,
                TotalCapacity = ticketTypeDto.TotalCapacity.Value,
                SoldCount = 0
            };

            await _context.TicketTypes.InsertOneAsync(ticketType);
            return CreatedAtAction(nameof(GetTicketTypeById), new { id = ticketType.Id }, ticketType);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> UpdateTicketType(string id, [FromBody] TicketTypeDto ticketTypeDto)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ticket type ID format");

            var existing = await _context.TicketTypes.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                return NotFound("No such ticket type");

            if (!string.IsNullOrWhiteSpace(ticketTypeDto.Name))
            {
                if(_context.TicketTypes.Find(t => t.Name.ToLower() == ticketTypeDto.Name.ToLower() && t.EventId == existing.EventId).Any())
                    return BadRequest("Ticket type already exists");
                existing.Name = ticketTypeDto.Name;
            }

            if (ticketTypeDto.Price.HasValue)
            {
                if (ticketTypeDto.Price < 0)
                    return BadRequest("Price cannot be negative");
                existing.Price = ticketTypeDto.Price.Value;
            }

            if (ticketTypeDto.TotalCapacity.HasValue)
            {
                if (ticketTypeDto.TotalCapacity < existing.SoldCount)
                    return BadRequest($"Total capacity cannot be less than already sold count ({existing.SoldCount})");
                existing.TotalCapacity = ticketTypeDto.TotalCapacity.Value;
            }
            
            await _context.TicketTypes.ReplaceOneAsync(x => x.Id == id, existing);
            return Ok(existing);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> DeleteTicketType(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ticket type ID format");

            var result = await _context.TicketTypes.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0)
                return NotFound("No such ticket type");

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}