using EventTicketManagement.Data;
using EventTicketManagement.Dtos;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using MongoDB.Driver;

namespace EventTicketManagement.Services;

public class OrderService : IOrderService
{
    private readonly MongoDbContext _context;
    private readonly IReservationService _reservationService;
    private readonly BankingService _bankingService;

    public OrderService(MongoDbContext context, IReservationService reservationService, BankingService bankingService)
    {
        _context = context;
        _reservationService = reservationService;
        _bankingService = bankingService;
    }

    public async Task<(bool Success, string? Error, Order? Order)> CreateOrderAsync(string userId, List<OrderItemDto> requestedItems)
    {
        var userExists = await _context.Users.Find(x => x.Id == userId).AnyAsync();
        if (!userExists)
            return (false, "No such user", null);

        var orderItems = new List<OrderItem>();
        var tickets = new List<Ticket>();
        decimal totalAmount = 0;

        foreach (var item in requestedItems)
        {
            if (string.IsNullOrWhiteSpace(item.TicketTypeId) || item.Quantity is null or <= 0)
                return (false, "Invalid order item", null);

            var ticketType = await _context.TicketTypes.Find(x => x.Id == item.TicketTypeId).FirstOrDefaultAsync();
            if (ticketType == null)
                return (false, $"Ticket type {item.TicketTypeId} not found", null);

            if (ticketType.SoldCount + item.Quantity.Value > ticketType.TotalCapacity)
                return (false, $"Not enough capacity for ticket type {ticketType.Name}", null);
            
            orderItems.Add(new OrderItem
            {
                TicketTypeId = ticketType.Id,
                TicketTypeName = ticketType.Name,
                UnitPrice = ticketType.Price,
                Quantity = item.Quantity.Value
            });

            totalAmount += ticketType.Price * item.Quantity.Value;

            for (int i = 0; i < item.Quantity.Value; i++)
            {
                tickets.Add(new Ticket
                {
                    TicketTypeId = ticketType.Id,
                    UniqueCode = Guid.NewGuid().ToString(),
                    CheckedIn = false,
                    OrderId = null!
                });
            }
        }

        var order = new Order
        {
            UserId = userId,
            Items = orderItems,
            TotalAmount = totalAmount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.InsertOneAsync(order);

        foreach (var ticket in tickets)
        {
            ticket.OrderId = order.Id;
        }
        
        // TEMPORARILY reserve tickets in Redis
        var reserved = await _reservationService.
            ReserveAsync(order);
        
        if (!reserved)
        {
            order.Status = "Failed";

            await _context.Orders.ReplaceOneAsync(
                x => x.Id == order.Id,
                order
            );

            return (
                false,
                "Tickets are no longer available",
                null
            );
        }
        
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Payments.InsertOneAsync(payment);
        
        // a fake banking validation that always returns true
        var paymentResult = await _bankingService.Pay(order.TotalAmount, true,order.Id);
        
        if (!paymentResult)
        {
            payment.Status = "Failed";
            order.Status = "Failed";

            await _context.Payments.ReplaceOneAsync(
                x => x.Id == payment.Id,
                payment
            );

            await _context.Orders.ReplaceOneAsync(
                x => x.Id == order.Id,
                order
            );

            await _reservationService
                .ReleaseAsync(order);

            return (
                false,
                "Payment failed",
                order
            );
        }
        
        payment.Status = "Success";
        order.Status = "Confirmed";
        
        if (tickets.Count > 0)
            await _context.Tickets.InsertManyAsync(tickets);

        await _context.Payments.ReplaceOneAsync(
            x => x.Id == payment.Id,
            payment
        );

        await _context.Orders.ReplaceOneAsync(
            x => x.Id == order.Id,
            order
        );

        await _reservationService.ConfirmAsync(order);

        return (true, null, order); 
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        return await _context.Orders.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        return await _context.Orders.Find(x => x.UserId == userId).ToListAsync();
    }
}