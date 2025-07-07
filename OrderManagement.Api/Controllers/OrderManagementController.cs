using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Services.Interfaces;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderManagementController : ControllerBase
{
    private readonly IOrderManagementService _service;

    public OrderManagementController(IOrderManagementService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns invoice details for a given order.
    /// </summary>
    [HttpGet("invoice/{orderId:int}")]
    public async Task<IActionResult> GetInvoice(int orderId, [FromQuery] string? search)
    {
        try
        {
            var invoice = await _service.GetInvoiceAsync(orderId, search);
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
            var report = await _service.GetOrdersDistributionReportAsync(city, sort);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
} 