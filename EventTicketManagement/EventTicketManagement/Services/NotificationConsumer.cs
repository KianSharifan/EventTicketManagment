using System.Text;
using System.Text.Json;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventTicketManagement.Services;

public class NotificationConsumer : BackgroundService
{
    private const string ExchangeName = "order_confirmed";
    private const string QueueName = "notifications";

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public NotificationConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken
        );

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "",
            cancellationToken: cancellationToken
        );

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken
        );

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                var orderEvent = JsonSerializer.Deserialize<OrderConfirmation>(json);
                if (orderEvent == null)
                {
                    await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var itemsSummary = string.Join(", ",
                    orderEvent.Items.Select(i => $"{i.TicketTypeName} x{i.Quantity}"));

                await emailService.SendAsync(
                    orderEvent.Email,
                    "Order confirmed",
                    $"order with ID:{orderEvent.OrderId}with items:{itemsSummary} wad completed."
                );

                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order-confirmed notification");
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await _channel!.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}