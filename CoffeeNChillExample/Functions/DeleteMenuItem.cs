using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoffeeNChill.Functions
{
    public class DeleteMenuItem
    {
        private readonly ILogger<DeleteMenuItem> _logger;
        private readonly TableService _tableService;

        public DeleteMenuItem(
            ILogger<DeleteMenuItem> logger,
            TableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
                Route = "menu/{category}/{id}")]
            HttpRequestData req,
            string category,
            string id)
        {
            try
            {
                if (string.IsNullOrEmpty(category) ||
                    string.IsNullOrEmpty(id))
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "Category and ID are required.");

                    return errorResponse;
                }

                var existingItem =
                    await _tableService.GetEntityAsync(category, id);

                if (existingItem == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "Menu item was not found.");

                    return errorResponse;
                }

                await _tableService.DeleteEntityAsync(category, id);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Menu item deleted successfully.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting menu item.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while deleting the menu item.");

                return response;
            }
        }
    }
}
