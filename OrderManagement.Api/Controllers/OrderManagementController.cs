using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Services.Interfaces;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderManagementController : ControllerBase
{
    private readonly IOrderManagementService _db;

    public OrderManagementController(IOrderManagementService db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns invoice details for a given order.
    /// </summary>
    [HttpGet("invoice/{orderId:int}")]
    public async Task<IActionResult> GetInvoice(int orderId, [FromQuery] string? search)
    {
        try
        {
            var invoice = await _db.GetInvoiceAsync(orderId, search);
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Returns orders distribution report by city.
    /// </summary>
    [HttpGet("ordersdistributionreport")]
    public async Task<IActionResult> GetOrdersDistributionReport([FromQuery] string? city, [FromQuery] string? sort)
    {
        try
        {
            var report = await _db.GetOrdersDistributionReportAsync(city, sort);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
} 