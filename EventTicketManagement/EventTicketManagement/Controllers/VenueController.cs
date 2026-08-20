using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace EventTicketManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenueController : ControllerBase
{
    private readonly MongoDbContext _context;

    public VenueController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var venues = await _context.Venues.Find(_ => true).ToListAsync();
            return Ok(venues);
        }
        catch (Exception)
        {
            return StatusCode(500,"An unexpected error occured");
        }
    }

    [HttpGet("city/{cityName}")]
    public async Task<IActionResult> Get(string cityName)
    {
        try
        {
            var r = await _context.Venues.Find(x => x.City.ToLower() == cityName.ToLower()).ToListAsync();
            return Ok(r);
        }
        catch (Exception)
        {
            return StatusCode(500,"An unexpected error occured");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid venue ID format");

            var venue = await _context.Venues.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (venue == null)
                return NotFound("No such venue");

            return Ok(venue);
        }
        catch (Exception)
        {
            return StatusCode(500,"An unexpected error occured");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateVenue([FromBody] VenueDto venueDto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(venueDto.Name) && !string.IsNullOrWhiteSpace(venueDto.Address) && venueDto.Capacity != null && !string.IsNullOrWhiteSpace(venueDto.City))
            {
                bool exists = await _context.Venues
                    .Find(x => x.Name == venueDto.Name)
                    .AnyAsync();
                if (exists)
                    return BadRequest("A venue with that name already exists");

                var venue = new Venue
                {
                    Name = venueDto.Name!,
                    Address = venueDto.Address!,
                    City = venueDto.City!,
                    Capacity = venueDto.Capacity!.Value
                };

                await _context.Venues.InsertOneAsync(venue);
                return CreatedAtAction(nameof(GetById), new { id = venue.Id }, venue);
            }
            return BadRequest("Invalid Inputs");
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVenue(string id, [FromBody] VenueDto venueDto)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid venue ID format");

            var existingVenue = await _context.Venues.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (existingVenue == null)
                return NotFound("No such venue");

            if (!string.IsNullOrWhiteSpace(venueDto.Name))
            {
                bool exists = await _context.Venues
                    .Find(x => x.Name == venueDto.Name && x.Id != id)
                    .AnyAsync();
                if (exists)
                    return BadRequest("A venue with that name already exists");

                existingVenue.Name = venueDto.Name;
            }

            if (!string.IsNullOrWhiteSpace(venueDto.Address))
                existingVenue.Address = venueDto.Address;

            if (!string.IsNullOrWhiteSpace(venueDto.City))
                existingVenue.City = venueDto.City;

            if (venueDto.Capacity.HasValue)
                existingVenue.Capacity = venueDto.Capacity.Value;

            await _context.Venues.ReplaceOneAsync(x => x.Id == id, existingVenue);
            return Ok(existingVenue);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVenue(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid venue ID format");
        
            var result = await _context.Venues.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount == 0)
                return NotFound("No such venue");

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}