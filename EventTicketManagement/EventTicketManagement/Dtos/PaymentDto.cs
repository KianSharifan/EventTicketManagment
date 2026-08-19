namespace EventTicketManagement.Dtos;

public class PaymentDto
{
    public decimal? Amount { get; set; }
    public string? OrderId { get; set; }
}