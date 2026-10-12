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
    public class GetAllMenuItems
    {
        private readonly ILogger<GetAllMenuItems> _logger;
        private readonly TableService _tableService;

        public GetAllMenuItems(
            ILogger<GetAllMenuItems> logger,
            TableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")]
            HttpRequestData req)
        {
            _logger.LogInformation("Getting all menu items.");

            try
            {
                var menuItems =
                    await _tableService.GetAllEntitiesAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(menuItems);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting menu items.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while getting menu items.");

                return response;
            }
        }
    }
}
