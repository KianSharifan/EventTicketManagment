using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : Controller
{   
    private readonly MongoDbContext _context;
    
    public OrderController(MongoDbContext context)
    {
        _context = context;
    }
    
    // [HttpGet("{orderId}")]
    // public async Task<IActionResult> GetByOrderId(string orderId)
    // {
    //     try
    //     {
    //         if (!ObjectId.TryParse(orderId, out _))
    //             return BadRequest("Invalid order ID format");
    //
    //         var payment = await _context.Payments.Find(x => x.OrderId == orderId).FirstOrDefaultAsync();
    //         if (payment == null)
    //             return NotFound("No payment found for that order");
    //
    //         return Ok(payment);
    //     }
    //     catch (Exception)
    //     {
    //         return StatusCode(500, "An unexpected error occured");
    //     }
    // } 
}