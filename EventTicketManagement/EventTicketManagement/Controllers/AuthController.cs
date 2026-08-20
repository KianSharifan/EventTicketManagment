using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.AspNetCore.RateLimiting;

namespace EventTicketManagement.Controllers;

[ApiController]
[EnableRateLimiting("AuthPolicy")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(MongoDbContext context, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("attendee/register")]
    public async Task<IActionResult> AttendeeRegister(RegisterDto dto)
    {
        try
        {
            var existingUser = await _context.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Attendee"
            };

            await _context.Users.InsertOneAsync(user);

            var token = _tokenService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(AttendeeRegister), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    
    [HttpPost("organizer/register")]
    public async Task<IActionResult> OrganizerRegister(RegisterDto dto)
    {
        try
        {
            var existingUser = await _context.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Organizer"
            };

            await _context.Users.InsertOneAsync(user);

            var token = _tokenService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(AttendeeRegister), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var user = await _context.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized("Invalid email or password.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Invalid email or password.");

            var token = _tokenService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}