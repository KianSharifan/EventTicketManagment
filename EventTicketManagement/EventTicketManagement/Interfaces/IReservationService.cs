namespace EventTicketManagement.Interfaces;
using Models;

public interface IReservationService
{
    Task<bool> ReserveAsync(Order order);
    Task ReleaseAsync(Order order);
    Task ConfirmAsync(Order order);
}