using EventTicketManagement.Data;
using MongoDB.Driver;
using EventTicketManagement.Models;

namespace EventTicketManagement.Services;
using MongoDB.Bson;

public class BankingService
{
    private readonly MongoDbContext  _context;

    public BankingService(MongoDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Pay(decimal amount, bool success,string orderId)
    {
        if (!ObjectId.TryParse(orderId, out _))
            return false;
        
        var order = await _context.Orders.Find(x => x.Id == orderId).FirstOrDefaultAsync();
        if (order == null)
            return false;

        if (amount <= 0)
            return false;

        if (success)
        {
            await _context.Payments.InsertOneAsync(new Payment
            {
                Amount = amount,
                OrderId = orderId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            return true;
        }
        return false;
    }
}