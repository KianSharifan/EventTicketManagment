using Microsoft.AspNetCore.Mvc;
using EventTicketManagement.Data;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using EventTicketManagement.Services;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Test : Controller
{
    private readonly IEmailService  _emailService;
    private readonly MongoDbContext _context;

    public Test(MongoDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // [HttpGet]
    // public async Task<IActionResult> Get()
    // {
    //     await _context.Users.InsertOneAsync(new User
    //     {
    //         Id = "68a8c7e5f1a2b3c4d5e6f789", Email = "kian@gmia.com", PasswordHash = "kkk", FullName = "kina",
    //         Role = "org", CreatedAt = DateTime.UtcNow
    //     });
    //     return Ok();
    // }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await _context.TicketTypes.InsertOneAsync(new TicketType{Id = "68a8c7e5f1a2b3c4d5e6f789",EventId = "68a8c7e5f1a2b3c4d5e6f789",Price = 1,TotalCapacity = 100,SoldCount = 0,Name = "hihihi"});
        return Ok();
    }
    
    [HttpPost("test-publish")]
    public async Task<IActionResult> TestPublish([FromServices] IOrderPublisher orderPublisher)
    {
        await orderPublisher.PublishOrderConfirmedAsync(new OrderConfirmation
        {
            OrderId = "test-order-123",
            Email = "your-personal-email@gmail.com",
            Items = new List<OrderConfirmedItem>
            {
                new() { TicketTypeId = "t1", TicketTypeName = "VIP", Quantity = 2 }
            }
        });

        return Ok("Published");
    }
    
    [HttpGet("test-qr")]
    public IActionResult TestQrCode([FromServices] IQrCodeService qrCodeService)
    {
        var qrBytes = qrCodeService.GenerateQrCode("kian");
        return File(qrBytes, "image/png");
    }
    
    [HttpGet("test-pdf")]
    public async Task<IActionResult> TestPdf(
        [FromServices] ITicketPdfService ticketPdfService,
        [FromServices] MongoDbContext context)
    {
        // یه سفارش واقعی که از قبل توی Mongo داری رو پیدا کن
        // (باید حداقل یه Order با Status=Confirmed و چند تا Ticket مرتبط داشته باشی)
        var order = await context.Orders.Find(o => o.Status == "Confirmed").FirstOrDefaultAsync();
        if (order == null)
            return NotFound("No confirmed order found to test with");

        var orderConfirmation = new OrderConfirmation
        {
            OrderId = order.Id,
            Email = "sharifankina@gmail.com",
            Items = order.Items.Select(i => new OrderConfirmedItem
            {
                TicketTypeId = i.TicketTypeId,
                TicketTypeName = i.TicketTypeName,
                Quantity = (int)i.Quantity
            }).ToList()
        };

        var pdfBytes = await ticketPdfService.GenerateAsync(orderConfirmation);

        return File(pdfBytes, "application/pdf", "test-ticket.pdf");
    }
    
    // [HttpGet]
    // public async Task<IActionResult> Get()
    // {
    //     await _context.Events;
    //     return Ok();
    // } 
}
