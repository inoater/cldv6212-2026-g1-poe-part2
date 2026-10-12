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
    public class GetMenuItemsByCategory
    {
        private readonly ILogger<GetMenuItemsByCategory> _logger;
        private readonly TableService _tableService;

        public GetMenuItemsByCategory(
            ILogger<GetMenuItemsByCategory> logger,
            TableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu/category/{category}")]
            HttpRequestData req,
            string category)
        {
            _logger.LogInformation(
                "Getting menu items for category {category}.",
                category);

            if (string.IsNullOrEmpty(category))
            {
                var errorResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await errorResponse.WriteStringAsync(
                    "Category is required.");

                return errorResponse;
            }

            try
            {
                var menuItems =
                    await _tableService.GetEntitiesByCategoryAsync(category);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(menuItems);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error filtering menu items.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while filtering menu items.");

                return response;
            }
        }
    }
}
