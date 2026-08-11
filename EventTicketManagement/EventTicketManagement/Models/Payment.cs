namespace EventTicketManagement.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public required string OrderId { get; set; }

    public decimal Amount { get; set; }
    public required string Status { get; set; }          // "Pending", "Success", "Failed"
    public DateTime CreatedAt { get; set; }
}