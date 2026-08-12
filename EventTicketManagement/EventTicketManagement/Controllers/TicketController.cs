using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly MongoDbContext _context;

    public TicketController(MongoDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllTickets()
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
    public async Task<IActionResult> GetTicketById(string id)
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
    
    [HttpPut("{id}/check-in")]
    public async Task<IActionResult> CheckIn(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ticket ID format");

            var ticket = await _context.Tickets.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (ticket == null)
                return NotFound("No such ticket");

            if (ticket.CheckedIn)
                return BadRequest("Ticket already checked in");

            ticket.CheckedIn = true;
            ticket.CheckedInAt = DateTime.UtcNow;

            await _context.Tickets.ReplaceOneAsync(x => x.Id == id, ticket);
            return Ok(ticket);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ticket ID format");

            var result = await _context.Tickets.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0)
                return NotFound("No such ticket");

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}