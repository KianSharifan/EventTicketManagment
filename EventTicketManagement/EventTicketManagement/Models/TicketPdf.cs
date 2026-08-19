using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventTicketManagement.Models;

public class TicketPdf
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public byte[] Content { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
}