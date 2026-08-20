namespace EventTicketManagement.Interfaces;
using EventTicketManagement.Models;

public interface ITokenService
{
    string GenerateToken(User user);
}