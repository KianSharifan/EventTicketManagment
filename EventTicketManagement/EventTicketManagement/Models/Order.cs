namespace EventTicketManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }  = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }
    
    public required List<OrderItem> Items { get; set; }   // embedded — چون همیشه با هم خونده میشن
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }   // "Pending" | "Confirmed" | "Failed"
    public DateTime CreatedAt { get; set; }
}

public class OrderItem
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string TicketTypeId { get; set; }
    
    public required string TicketTypeName { get; set; }
    public decimal UnitPrice { get; set; }
    public uint Quantity { get; set; }
}