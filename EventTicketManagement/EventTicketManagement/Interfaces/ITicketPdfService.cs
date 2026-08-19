namespace EventTicketManagement.Interfaces;


public interface ITicketPdfService
{
    public Task<byte[]> GenerateAsync(Models.OrderConfirmation orderEvent);
}