namespace EventTicketManagement.Dtos;

public class CreateOrderDto
{
    public string? UserId { get; set; }
    public List<OrderItemDto>? Items { get; set; }
}

public class OrderItemDto
{
    public string? TicketTypeId { get; set; }
    public uint? Quantity { get; set; }
}