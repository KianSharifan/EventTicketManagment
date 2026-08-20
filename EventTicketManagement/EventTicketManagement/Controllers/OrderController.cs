using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using EventTicketManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly MongoDbContext _context;

    public OrderController(IOrderService orderService, MongoDbContext context)
    {
        _orderService = orderService;
        _context = context;
    }
    
     [HttpGet]
     [Authorize(Roles = "Admin")]
     public async Task<IActionResult> GetAllOrders()
     {
         try
         {
             return Ok(await _context.Orders.Find(_ => true).ToListAsync());
         }
         catch (Exception)
         {
             return StatusCode(500, "An unexpected error occured");
         }
     }
     
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid order ID format");

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound("No such order");

            return Ok(order);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        try
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("Invalid user ID format");

            return Ok(await _orderService.GetByUserIdAsync(userId));
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Attendee,Organizer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderDto.UserId) || orderDto.Items == null || orderDto.Items.Count == 0)
                return BadRequest("Not Valid Inputs!");

            if (!ObjectId.TryParse(orderDto.UserId, out _))
                return BadRequest("Invalid user ID format");

            var (success, error, order) = await _orderService.CreateOrderAsync(orderDto.UserId, orderDto.Items);

            if (!success)
                return BadRequest(error);

            return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Invalid ID format"); 
        
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
            return NotFound("No such order");
        
        await _context.Orders.DeleteOneAsync(o => o.Id == id);
        return NoContent();
    }
}