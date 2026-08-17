using System.Text;
using System.Text.Json;
using EventTicketManagement.Interfaces;
using EventTicketManagement.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventTicketManagement.Services;

public class TicketGenerationConsumer : BackgroundService
{
    private const string ExchangeName = "order_confirmed";
    private const string QueueName = "ticket-generation";

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketGenerationConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public TicketGenerationConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<TicketGenerationConsumer> logger)
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
                var ticketPdfService = scope.ServiceProvider.GetRequiredService<ITicketPdfService>();
                var pdfBytes = await ticketPdfService.GenerateAsync(orderEvent);
                _logger.LogInformation("PDF generated: {Size} bytes for order {OrderId}", pdfBytes.Length, orderEvent.OrderId);

                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate ticket PDF");
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