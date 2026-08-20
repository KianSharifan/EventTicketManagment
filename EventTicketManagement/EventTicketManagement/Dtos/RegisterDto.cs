using System.ComponentModel.DataAnnotations;
namespace EventTicketManagement.Dtos;

public class RegisterDto
{
    public required string FullName { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    public required string Password { get; set; }
}