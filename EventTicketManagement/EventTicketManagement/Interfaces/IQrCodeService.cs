namespace EventTicketManagement.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content);
}