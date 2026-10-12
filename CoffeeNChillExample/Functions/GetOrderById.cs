using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class GetOrderById
    {
        private readonly ILogger<GetOrderById> _logger;
        private readonly OrderService _orderService;

        public GetOrderById(
            ILogger<GetOrderById> logger,
            OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("GetOrderById")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}")]
            HttpRequestData req,
            string orderId)
        {
            _logger.LogInformation("Getting order {orderId}.", orderId);

            try
            {
                if (string.IsNullOrEmpty(orderId))
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "Order ID is required.");

                    return errorResponse;
                }

                var order =
                    await _orderService.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "Order was not found.");

                    return errorResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(order);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting order.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while getting the order.");

                return response;
            }
        }
    }
}
