namespace EventTicketManagement.Models;

public class OrderConfirmation
{
    public required string OrderId { get; set; }
    public required string Email { get; set; }
    public List<OrderConfirmedItem> Items { get; set; } = new();
}

public class OrderConfirmedItem
{
    public string TicketTypeId { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}