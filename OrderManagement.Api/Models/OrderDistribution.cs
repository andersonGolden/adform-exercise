namespace OrderManagement.Api.Models;

public class OrderDistribution
{
    public string CustomerCity { get; set; } = string.Empty;
    public int NumberOfOrders { get; set; }
    public decimal TotalAmountUsd { get; set; }
} 