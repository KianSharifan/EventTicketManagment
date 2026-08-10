using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Controllers;

public class PaymentController : Controller
{
    private readonly MongoDbContext _context;

    public PaymentController(MongoDbContext context)
    {
        _context = context;
    }
    
    
}