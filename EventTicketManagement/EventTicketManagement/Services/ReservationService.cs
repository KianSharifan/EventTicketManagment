using EventTicketManagement.Data;
using EventTicketManagement.Interfaces;
using MongoDB.Driver;
using StackExchange.Redis;

namespace EventTicketManagement.Services;

public class ReservationService : IReservationService
{
    private readonly IDatabase _redis;
    private readonly MongoDbContext _context;

    private static readonly TimeSpan ReservationDuration =
        TimeSpan.FromMinutes(10);

    public ReservationService(
        IConnectionMultiplexer redis,
        MongoDbContext context)
    {
        _redis = redis.GetDatabase();
        _context = context;
    }

    public async Task<bool> ReserveAsync(Models.Order order)
    {
        var reservedItems = new List<(string TicketTypeId, int Quantity)>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiresAt = DateTimeOffset.UtcNow.Add(ReservationDuration).ToUnixTimeSeconds();

        foreach (var item in order.Items)
        {
            var ticketType = await _context.TicketTypes
                .Find(x => x.Id == item.TicketTypeId)
                .FirstOrDefaultAsync();

            if (ticketType == null)
            {
                await ReleaseReservedItemsAsync(order.Id, reservedItems);
                return false;
            }

            var soldKey = $"ticketType:{item.TicketTypeId}:sold";
            var setKey = $"ticketType:{item.TicketTypeId}:reservations";
            var member = $"{order.Id}:{item.Quantity}";

            const string reserveScript = """
                local now = tonumber(ARGV[1])
                local capacity = tonumber(ARGV[2])
                local requested = tonumber(ARGV[3])
                local expiresAt = tonumber(ARGV[4])
                local member = ARGV[5]

                -- deleting the expired tickets
                redis.call("ZREMRANGEBYSCORE", KEYS[2], "-inf", now)

                local members = redis.call("ZRANGE", KEYS[2], 0, -1)
                local reserved = 0
                for i, m in ipairs(members) do
                    local qty = tonumber(string.match(m, ":(%d+)$"))
                    reserved = reserved + qty
                end

                local sold = tonumber(redis.call("GET", KEYS[1]) or "0")

                if sold + reserved + requested > capacity then
                    return 0
                end

                redis.call("ZADD", KEYS[2], expiresAt, member)
                return 1
                """;

            var result = await _redis.ScriptEvaluateAsync(
                reserveScript,
                new RedisKey[] { soldKey, setKey },
                new RedisValue[]
                {
                    now, ticketType.TotalCapacity, item.Quantity, expiresAt, member
                }
            );

            if ((int)result == 0)
            {
                await ReleaseReservedItemsAsync(order.Id, reservedItems);
                return false;
            }

            reservedItems.Add((item.TicketTypeId, (int)item.Quantity));
        }

        return true;
    }

    public async Task ReleaseAsync(Models.Order order)
    {
        await ReleaseReservedItemsAsync(
            order.Id,
            order.Items.Select(i => (i.TicketTypeId, (int)i.Quantity)).ToList());
    }

    private async Task ReleaseReservedItemsAsync(string orderId, List<(string TicketTypeId, int Quantity)> items)
    {
        foreach (var item in items)
        {
            var setKey = $"ticketType:{item.TicketTypeId}:reservations";
            var member = $"{orderId}:{item.Quantity}";
            await _redis.SortedSetRemoveAsync(setKey, member);
        }
    }

    public async Task ConfirmAsync(Models.Order order)
    {
        foreach (var item in order.Items)
        {
            var soldKey = $"ticketType:{item.TicketTypeId}:sold";
            var setKey = $"ticketType:{item.TicketTypeId}:reservations";
            var member = $"{order.Id}:{item.Quantity}";

            const string confirmScript = """
                local quantity = tonumber(ARGV[1])
                local member = ARGV[2]

                redis.call("ZREM", KEYS[2], member)

                redis.call("INCRBY", KEYS[1], quantity)

                return 1
                """;

            await _redis.ScriptEvaluateAsync(
                confirmScript,
                new RedisKey[] { soldKey, setKey },
                new RedisValue[] { item.Quantity, member }
            );

            await _context.TicketTypes.UpdateOneAsync(
                x => x.Id == item.TicketTypeId,
                Builders<Models.TicketType>.Update.Inc(x => (int)x.SoldCount, (int)item.Quantity)
            );
        }
    }
}