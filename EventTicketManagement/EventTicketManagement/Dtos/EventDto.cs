namespace EventTicketManagement.Dtos;

public class EventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public string? VenueId { get; set; }
    public string? CategoryId { get; set; }
}