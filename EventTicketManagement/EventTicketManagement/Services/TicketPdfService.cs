using EventTicketManagement.Data;
using EventTicketManagement.Interfaces;
using MongoDB.Driver;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventTicketManagement.Services;

public interface ITicketPdfService
{
    public Task<byte[]> GenerateAsync(Models.OrderConfirmation orderEvent);
}

public class TicketPdfService : ITicketPdfService
{
    private readonly ILogger<TicketPdfService> _logger;
    private readonly IQrCodeService _qrCodeService;
    private readonly MongoDbContext _context;

    public TicketPdfService(ILogger<TicketPdfService> logger, IQrCodeService qrCodeService, MongoDbContext context)
    {
        _logger = logger;
        _qrCodeService = qrCodeService;
        _context = context;
    }

    public async Task<byte[]> GenerateAsync(Models.OrderConfirmation orderEvent)
    {
        _logger.LogInformation("Generating PDF ticket for order {OrderId}", orderEvent.OrderId);
        var tickets = await _context.Tickets
            .Find(t => t.OrderId == orderEvent.OrderId)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            foreach (var ticket in tickets)
            {
                var qrBytes = _qrCodeService.GenerateQrCode(ticket.UniqueCode);
                var ticketTypeName = orderEvent.Items
                                         .FirstOrDefault(i => i.TicketTypeId == ticket.TicketTypeId)?.TicketTypeName
                                     ?? "Unknown";

                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text("Entry Ticket").FontSize(20).Bold();

                        column.Item().Text($"TicketType: {ticketTypeName}");
                        column.Item().Text($"Order Code: {ticket.UniqueCode}");
                        
                        column.Item().Text("For Entry you should give the qrcode to one of the staff, \nand do not share your code to any other.").FontSize(10).Thin();

                        column.Item().PaddingTop(10).Image(qrBytes).FitWidth();
                    });
                });
            }
        });

        var pdfBytes = document.GeneratePdf();

        _logger.LogInformation(
            "Generated PDF with {Count} ticket(s) for order {OrderId}",
            tickets.Count, orderEvent.OrderId
        );

        return pdfBytes;
    }
}