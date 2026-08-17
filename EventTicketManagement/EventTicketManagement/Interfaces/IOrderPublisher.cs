using EventTicketManagement.Models;

namespace EventTicketManagement.Interfaces;

public interface IOrderPublisher
{ 
    Task PublishOrderConfirmedAsync(OrderConfirmation orderEvent);
    public ValueTask DisposeAsync();
}