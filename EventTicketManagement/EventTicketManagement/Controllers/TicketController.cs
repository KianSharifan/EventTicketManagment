using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly MongoDbContext _context;

    public TicketController(MongoDbContext context)
    {
        _context = context;
    }
}