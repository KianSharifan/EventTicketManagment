namespace EventTicketManagement.Interfaces;
using Models;
using Dtos;


public interface IOrderService
{
    public Task<(bool Success, string? Error, Order? Order)> CreateOrderAsync(string userId,
        List<OrderItemDto> requestedItems);
}