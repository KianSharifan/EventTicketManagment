using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Controllers;

public class VenueController : Controller
{
    private readonly MongoDbContext  _context;

    public VenueController(MongoDbContext context)
    {
        _context = context;
    }
}