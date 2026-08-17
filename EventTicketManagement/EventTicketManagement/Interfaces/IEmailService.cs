namespace EventTicketManagement.Interfaces;

public interface IEmailService
{
    public Task SendAsync(string toEmail, string subject, string body, byte[]? attachmentBytes = null,
        string? attachmentFileName = null);
}