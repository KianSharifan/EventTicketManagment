using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EventTicketManagement.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    [MaxLength(200)]
    public string Role { get; set; }     // "Organizer" | "Attendee"
    public DateTime CreatedAt { get; set; }
}