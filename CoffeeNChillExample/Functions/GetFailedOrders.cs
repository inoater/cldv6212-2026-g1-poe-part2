using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class GetFailedOrders
    {
        private readonly ILogger<GetFailedOrders> _logger;
        private readonly FailedOrderService _failedOrderService;

        public GetFailedOrders(
            ILogger<GetFailedOrders> logger,
            FailedOrderService failedOrderService)
        {
            _logger = logger;
            _failedOrderService = failedOrderService;
        }

        [Function("GetFailedOrders")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "failed-orders")]
            HttpRequestData req)
        {
            _logger.LogInformation("Getting failed orders.");

            try
            {
                var failedOrders =
                    await _failedOrderService.GetAllFailedOrdersAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(failedOrders);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting failed orders.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while getting failed orders.");

                return response;
            }
        }
    }
}
