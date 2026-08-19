namespace EventTicketManagement.Data;
using Models;
using Setting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<Event> Events =>
        _database.GetCollection<Event>("events");
    
    public IMongoCollection<TicketType> TicketTypes =>
        _database.GetCollection<TicketType>("ticket_types");
    
    public IMongoCollection<Order> Orders =>
        _database.GetCollection<Order>("orders");
    
    public IMongoCollection<Ticket> Tickets =>
        _database.GetCollection<Ticket>("tickets");
    
    public IMongoCollection<Venue> Venues =>
        _database.GetCollection<Venue>("venues");
    
    public IMongoCollection<EventCategory> EventCategories =>
        _database.GetCollection<EventCategory>("event_categories");
    
    public IMongoCollection<Payment> Payments =>
        _database.GetCollection<Payment>("payments");
    
    public IMongoCollection<TicketPdf> TicketPdfs =>
        _database.GetCollection<TicketPdf>("TicketPdfs");
}