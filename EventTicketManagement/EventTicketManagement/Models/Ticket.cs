namespace EventTicketManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }  = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public required string OrderId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string TicketTypeId { get; set; }
    
    public required string UniqueCode { get; set; }          // کد یکتا برای QR
    public bool CheckedIn { get; set; }
    public DateTime? CheckedInAt { get; set; }
}