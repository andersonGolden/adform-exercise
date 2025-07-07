using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using OrderManagement.Api.Controllers;
using Microsoft.Extensions.Configuration;
using OrderManagement.Api.Services.Interfaces;
using OrderManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Tests;

public class ApiTests //: IClassFixture<WebApplicationFactory<Program>>
{
    //private readonly WebApplicationFactory<Program> _factory;

    //public ApiTests(WebApplicationFactory<Program> factory)
    //{
    //    _factory = factory;
    //}
    private readonly IConfiguration _configuration;
    protected OrderManagementController _controller;
    private readonly IOrderManagementService _orderManagementService;

    public ApiTests()
    {
          _configuration = new ConfigurationBuilder()  
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        // instantiate order mgnt service
        _orderManagementService = new OrderManagementService(_configuration);

        // instantiate order mgnt controller
        _controller = new OrderManagementController(_orderManagementService);
    }
    [Fact]
    public async Task GetInvoice_WithValidOrderId_ReturnsOk()
    {
        // Arrange
        var orderId = 45; 

        // Act
        var result = await _controller.GetInvoice(orderId, string.Empty);

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<Invoice>)actionValue.Value;
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task GetInvoice_WithInvalidOrderId_ReturnsEmptyList()
    {
        // Arrange
        var orderId = 999999; // Non-existent order

        // Act
        var result = await _controller.GetInvoice(orderId, string.Empty);

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<Invoice>)actionValue.Value;
        Assert.Equal(0, response.Count());
    }

    [Fact]
    public async Task GetInvoice_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var orderId = 45;
        var searchTerm = "Storage";

        // Act
        var result = await _controller.GetInvoice(orderId, searchTerm);

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<Invoice>)actionValue.Value;
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task GetOrdersDistributionReport_ReturnsOk()
    {
        // Arrange

        // Act
        var result = await _controller.GetOrdersDistributionReport(string.Empty, string.Empty);// get all orders regardless of city

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<OrderDistribution>)actionValue.Value;
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task GetOrdersDistributionReport_WithCityFilter_ReturnsFilteredResults()
    {
        // Arrange
        var city = "Vilnius";

        // Act
        var result = await _controller.GetOrdersDistributionReport(city, string.Empty);// get for a specific city without sort direction

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<OrderDistribution>)actionValue.Value;
        Assert.Equal(1, response.Count());
    }

    [Fact]
    public async Task GetOrdersDistributionReport_WithSortDirection_ReturnsSortedResults()
    {
        // Arrange
        var sortDirection = "asc";

        // Act
        var result = await _controller.GetOrdersDistributionReport(string.Empty, sortDirection);// get all orders with sort direction

        // Assert
        var actionValue = Assert.IsType<OkObjectResult>(result);
        var response = (IEnumerable<OrderDistribution>)actionValue.Value;
        Assert.NotEmpty(response);
    }

} 