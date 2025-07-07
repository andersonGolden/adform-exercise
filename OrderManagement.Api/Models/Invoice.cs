namespace OrderManagement.Api.Models;

public class Invoice
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerCountry { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal OrderTotal { get; set; }
} 