namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class EventCategory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Name { get; set; }   // "Concert", "Conference", "Workshop", ...
    public string? Description { get; set; }
}