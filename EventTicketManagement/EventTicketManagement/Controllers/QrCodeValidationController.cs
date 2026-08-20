using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;


[ApiController]
[Route("api/[controller]")]
public class QrCodeValidationController : Controller
{
    private readonly MongoDbContext _context;

    public QrCodeValidationController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet("{uniqueCode}")]
    public async Task<IActionResult> Get(string uniqueCode)
    {
        var ticket = await _context.Tickets.Find(t => t.UniqueCode == uniqueCode).FirstOrDefaultAsync();
        if (ticket == null)
            return NotFound("Ticket not found");
        
        if(ticket.CheckedIn)
            return BadRequest("Ticket has been checked in");
        
        ticket.CheckedIn = true;
        ticket.CheckedInAt = DateTime.UtcNow;
        
        var type =await _context.TicketTypes.Find(t => t.Id == ticket.TicketTypeId).FirstOrDefaultAsync();
        return Ok(type);
    }
}