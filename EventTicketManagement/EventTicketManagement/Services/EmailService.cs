using System.Net;
using System.Net.Mail;
using EventTicketManagement.Interfaces;

namespace EventTicketManagement.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(string toEmail, string subject, string body ,byte[]? attachmentBytes = null, string? attachmentFileName = null)
    {
        var smtpHost = _configuration["Smtp:Host"]!;
        var smtpPort = int.Parse(_configuration["Smtp:Port"]!);
        var smtpUser = _configuration["Smtp:Username"]!;
        var smtpPass = _configuration["Smtp:Password"]!;
        var fromAddress = _configuration["Smtp:FromAddress"]!;

        using var client = new SmtpClient(smtpHost, smtpPort);
        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        
        client.EnableSsl = true;
        
        using var message = new MailMessage(fromAddress, toEmail, subject, body);
        message.IsBodyHtml = true;

        MemoryStream? attachmentStream = null;

        if (attachmentBytes != null && attachmentFileName != null)
        {
            attachmentStream = new MemoryStream(attachmentBytes);
            message.Attachments.Add(new Attachment(attachmentStream, attachmentFileName, "application/pdf"));
        }

        try
        {
            await client.SendMailAsync(message);
        }
        finally
        {
            attachmentStream?.Dispose();
        }    
    }
}