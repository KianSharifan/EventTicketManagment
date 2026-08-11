namespace EventTicketManagement.Dtos;

public class TicketTypeDto
{
    public string? EventId { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? TotalCapacity { get; set; }
}