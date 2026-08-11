namespace EventTicketManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    public required List<OrderItem> Items { get; set; }   // embedded — چون همیشه با هم خونده میشن

    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }            // "Pending" | "Confirmed" | "Failed"
    public DateTime CreatedAt { get; set; }
}

// It is Embedded because it has been used only and only in Order Class
public class OrderItem
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string TicketTypeId { get; set; }

    public required string TicketTypeName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}