using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using EventTicketManagement.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IOrderService _orderService;
    private readonly ITicketPdfService _ticketPdfService;

    public TicketController(MongoDbContext context, IOrderService orderService, ITicketPdfService ticketPdfService)
    {
        _context = context;
        _orderService = orderService;
        _ticketPdfService = ticketPdfService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllTickets()
    {
        try
        {
            // return Ok(await _context.TicketTypes.Find(_ => true).ToListAsync());
            return Ok(await _context.Tickets.Find(_ => true).ToListAsync());
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
    
    [HttpGet("orders/{orderId}/ticket-pdf")]
    public async Task<IActionResult> DownloadTicketPdf(string orderId)
    {
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null) 
            return NotFound("No such order");

        // چک کن این کاربر واقعاً صاحب این orderه (auth بعداً که اضافه شد)

        var content = await _context.TicketPdfs.Find(t => t.OrderId == orderId).FirstOrDefaultAsync();

        return File(content!.Content, "application/pdf", $"ticket-{orderId}.pdf");
    }
}