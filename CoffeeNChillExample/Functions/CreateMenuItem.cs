using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class CreateMenuItem
    {
        private readonly ILogger<CreateMenuItem> _logger;
        private readonly TableService _tableService;

        public CreateMenuItem(
            ILogger<CreateMenuItem> logger,
            TableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")]
            HttpRequestData req)
        {
            _logger.LogInformation("Creating a menu item.");

            try
            {
                var menuItem =
                    await JsonSerializer.DeserializeAsync<MenuItem>(req.Body);

                if (menuItem == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Please enter a valid menu item.");
                    return errorResponse;
                }

                if (string.IsNullOrEmpty(menuItem.PartitionKey) ||
                    string.IsNullOrEmpty(menuItem.RowKey) ||
                    string.IsNullOrEmpty(menuItem.Name))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync(
                        "Category, ID and Name are required.");
                    return errorResponse;
                }

                if (menuItem.Price < 0)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync(
                        "Price cannot be negative.");
                    return errorResponse;
                }

                await _tableService.AddEntityAsync(menuItem);

                var response = req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(menuItem);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating menu item.");

                var response = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while creating the menu item.");

                return response;
            }
        }
    }
}
