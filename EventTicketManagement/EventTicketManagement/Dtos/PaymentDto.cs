namespace EventTicketManagement.Dtos;

public class PaymentDto
{
    public string? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Status { get; set; }
}