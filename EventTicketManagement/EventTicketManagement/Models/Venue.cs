namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Venue
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required uint Capacity { get; set; }
}