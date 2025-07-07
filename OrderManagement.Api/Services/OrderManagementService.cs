using Dapper;
using Npgsql;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services.Interfaces;

namespace OrderManagement.Api.Services;

public class OrderManagementService : IOrderManagementService
{
    private readonly string _connectionString;

    public OrderManagementService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=order_management;Username=admin;Password=password123";
    }

    public async Task<IEnumerable<Invoice>> GetInvoiceAsync(int orderId, string? searchTerm = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            WITH order_summary AS (
                SELECT 
                    o.id as order_id,
                    o.created_at,
                    c.first_name,
                    c.last_name,
                    c.email,
                    c.details->>'city' as customer_city,
                    c.details->>'country' as customer_country
                FROM orders o
                JOIN customers c ON o.customer_id = c.id
                WHERE o.id = @OrderId
            )
            SELECT 
                os.order_id as OrderId,
                os.created_at as CreatedAt,
                os.first_name || ' ' || os.last_name as CustomerName,
                os.email as CustomerEmail,
                os.customer_city as CustomerCity,
                os.customer_country as CustomerCountry,
                p.name as ProductName,
                p.category as ProductCategory,
                oi.quantity as Quantity,
                p.price as UnitPrice,
                (oi.quantity * p.price) as LineTotal,
                SUM(oi.quantity * p.price) OVER (PARTITION BY os.order_id) as OrderTotal
            FROM order_summary os
            JOIN order_items oi ON os.order_id = oi.order_id
            JOIN products p ON oi.product_id = p.id";

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += @" WHERE p.name ILIKE @SearchPattern OR p.category ILIKE @SearchPattern";
        }

        sql += " ORDER BY p.name";

        var parameters = new
        {
            OrderId = orderId,
            SearchPattern = $"%{searchTerm}%"
        };

        return await connection.QueryAsync<Invoice>(sql, parameters);
    }

    public async Task<IEnumerable<OrderDistribution>> GetOrdersDistributionReportAsync(string? cityFilter = null, string? sortDirection = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            WITH city_stats AS (
                SELECT 
                    c.details->>'city' as customer_city,
                    COUNT(DISTINCT o.id) as number_of_orders,
                    SUM(oi.quantity * p.price) as total_amount
                FROM customers c
                JOIN orders o ON c.id = o.customer_id
                JOIN order_items oi ON o.id = oi.order_id
                JOIN products p ON oi.product_id = p.id";

        if (!string.IsNullOrWhiteSpace(cityFilter))
        {
            sql += " WHERE c.details->>'city' ILIKE @CityPattern";
        }

        sql += @"
                GROUP BY c.details->>'city'
            )
            SELECT 
                customer_city as CustomerCity,
                number_of_orders as NumberOfOrders,
                ROUND(total_amount::NUMERIC, 2) as TotalAmountUsd
            FROM city_stats
            ORDER BY ";

        // Handle sort direction
        if (sortDirection?.ToLower() == "asc")
        {
            sql += "number_of_orders ASC";
        }
        else
        {
            sql += "number_of_orders DESC";
        }

        var parameters = new
        {
            CityPattern = $"%{cityFilter}%"
        };

        return await connection.QueryAsync<OrderDistribution>(sql, parameters);
    }
} 