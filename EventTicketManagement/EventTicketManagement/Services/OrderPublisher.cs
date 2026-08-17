using System.Text;
using System.Text.Json;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using RabbitMQ.Client;

namespace EventTicketManagement.Services;

public class OrderPublisher : IOrderPublisher, IAsyncDisposable
{
    private const string ExchangeName = "order_confirmed";

    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public OrderPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_channel != null) return;

        await _initLock.WaitAsync();
        try
        {
            if (_channel != null) return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true
            );
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishOrderConfirmedAsync(OrderConfirmation orderEvent)
    {
        await EnsureInitializedAsync();

        var json = JsonSerializer.Serialize(orderEvent);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { Persistent = true };

        await _channel!.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "",
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}