namespace EventTicketManagement.Dtos;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}