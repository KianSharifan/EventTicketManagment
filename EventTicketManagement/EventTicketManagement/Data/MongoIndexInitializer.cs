namespace EventTicketManagement.Data;
using Models;
using MongoDB.Driver;

public class MongoIndexInitializer
{
    public static async Task InitializeAsync(MongoDbContext context)
    {
        var emailIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        await context.Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));
        
        var ticketCodeIndexKeys = Builders<Ticket>.IndexKeys.Ascending(t => t.UniqueCode);
        var ticketCodeIndexOptions = new CreateIndexOptions { Unique = true };
        await context.Tickets.Indexes.CreateOneAsync(new CreateIndexModel<Ticket>(ticketCodeIndexKeys, ticketCodeIndexOptions));

        var eventTitleIndexKeys = Builders<Event>.IndexKeys.Ascending(e => e.Title);
        await context.Events.Indexes.CreateOneAsync(new CreateIndexModel<Event>(eventTitleIndexKeys));
        
        var eventCategoryTitleIndexKeys = Builders<EventCategory>.IndexKeys.Ascending(e => e.Name);
        await context.EventCategories.Indexes.CreateOneAsync(new CreateIndexModel<EventCategory>(eventCategoryTitleIndexKeys));
    }
}