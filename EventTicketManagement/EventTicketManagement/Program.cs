using EventTicketManagement.Setting;
using EventTicketManagement.Data;
using EventTicketManagement.Interfaces;
using StackExchange.Redis;
using EventTicketManagement.Services;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IOrderPublisher, OrderPublisher>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITicketPdfService, TicketPdfService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<ITicketPdfService, TicketPdfService>();

builder.Services.AddHostedService<NotificationConsumer>();
builder.Services.AddHostedService<TicketGenerationConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await MongoIndexInitializer.InitializeAsync(mongoContext);
}


// app.UseAuthorization();
app.MapControllers();

app.Run();