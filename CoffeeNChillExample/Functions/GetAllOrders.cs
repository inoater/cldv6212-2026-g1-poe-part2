using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class GetAllOrders
    {
        private readonly ILogger<GetAllOrders> _logger;
        private readonly OrderService _orderService;

        public GetAllOrders(
            ILogger<GetAllOrders> logger,
            OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("GetAllOrders")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")]
            HttpRequestData req)
        {
            _logger.LogInformation("Getting all orders.");

            try
            {
                var orders =
                    await _orderService.GetAllOrdersAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(orders);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting orders.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while getting orders.");

                return response;
            }
        }
    }
}
