namespace EventTicketManagement.Services;

public static class BankingService
{
    public static bool Pay(decimal amount, bool success)
    {
        if (success)
            return true;
        return false;
    }
}