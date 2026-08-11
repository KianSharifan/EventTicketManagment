using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly MongoDbContext _context;

    public PaymentController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var payments = await _context.Payments.Find(_ => true).ToListAsync();
            return Ok(payments);
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
                return BadRequest("Invalid payment ID format");

            var payment = await _context.Payments.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (payment == null)
                return NotFound("No such payment");

            return Ok(payment);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentDto paymentDto)
    {
        try
        {
            if (paymentDto.OrderId == null || paymentDto.Amount == null ||
                paymentDto.Status == null)
                return BadRequest("Invalid Inputs");

            if (!ObjectId.TryParse(paymentDto.OrderId, out _))
                return BadRequest("Invalid order ID format");

            var order = await _context.Orders.Find(x => x.Id == paymentDto.OrderId).FirstOrDefaultAsync();
            if (order == null)
                return NotFound("No such order");

            bool alreadyExists = await _context.Payments
                .Find(x => x.OrderId == paymentDto.OrderId)
                .AnyAsync();
            if (alreadyExists)
                return BadRequest("A payment for this order already exists");

            var payment = new Payment
            {
                OrderId = paymentDto.OrderId,
                Amount = paymentDto.Amount.Value,
                Status = paymentDto.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.InsertOneAsync(payment);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(string id, [FromBody] PaymentDto paymentDto)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid payment ID format");

            var existingPayment = await _context.Payments.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (existingPayment == null)
                return NotFound("No such payment");

            if (!string.IsNullOrWhiteSpace(paymentDto.Status))
                existingPayment.Status = paymentDto.Status;

            if (paymentDto.Amount != null)
                existingPayment.Amount = paymentDto.Amount.Value;

            await _context.Payments.ReplaceOneAsync(x => x.Id == id, existingPayment);
            return Ok(existingPayment);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid payment ID format");

            var result = await _context.Payments.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0)
                return NotFound("No such payment");

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}