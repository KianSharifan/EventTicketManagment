namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string OrganizerId { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public required string EventCategoryId { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public required string VenueId { get; set; }

    public DateTime CreatedAt { get; set; }
}