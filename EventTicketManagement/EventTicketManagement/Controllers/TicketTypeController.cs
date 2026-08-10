using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Controllers;

public class TicketTypeController : Controller
{
    private readonly MongoDbContext  _context;

    public TicketTypeController(MongoDbContext context)
    {
        _context = context;
    }
}