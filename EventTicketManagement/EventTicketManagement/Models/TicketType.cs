namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class TicketType
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EventId { get; set; }

    public string Name { get; set; }            // "VIP", "Normal"
    public decimal Price { get; set; }
    public int TotalCapacity { get; set; }
    public int SoldCount { get; set; }           // فعلاً تو Mongo، بعداً منبع حقیقتش با Redis هماهنگ میشه
}