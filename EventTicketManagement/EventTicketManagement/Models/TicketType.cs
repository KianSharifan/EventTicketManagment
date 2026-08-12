namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class TicketType
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }  = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public required string EventId { get; set; }

    public required string Name { get; set; }            // "VIP", "Normal"
    public decimal Price { get; set; }
    public uint TotalCapacity { get; set; }
    public uint SoldCount { get; set; }           // فعلاً تو Mongo، بعداً منبع حقیقتش با Redis هماهنگ میشه
}