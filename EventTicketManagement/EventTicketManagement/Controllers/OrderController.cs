using EventTicketManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Controllers;

public class OrderController : Controller
{   
    private readonly MongoDbContext _context;
    
    public OrderController(MongoDbContext context)
    {
    _context = context;
    }
 
    
    
}