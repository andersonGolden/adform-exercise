using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services.Interfaces;

public interface IOrderManagementService
{
    Task<IEnumerable<Invoice>> GetInvoiceAsync(int orderId, string? searchTerm = null);
    Task<IEnumerable<OrderDistribution>> GetOrdersDistributionReportAsync(string? cityFilter = null, string? sortDirection = null);
}