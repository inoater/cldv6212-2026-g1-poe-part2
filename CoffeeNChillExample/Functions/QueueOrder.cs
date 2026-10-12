using Azure;
using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class QueueOrder
    {
        private readonly ILogger<QueueOrder> _logger;
        private readonly QueueService _queueService;

        public QueueOrder(
            ILogger<QueueOrder> logger,
            QueueService queueService)
        {
            _logger = logger;
            _queueService = queueService;
        }

        [Function("QueueOrder")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")]
            HttpRequestData req)
        {
            _logger.LogInformation("Queueing a new order.");

            try
            {
                var order =
                    await JsonSerializer.DeserializeAsync<OrderQueueMessage>(req.Body);

                if (order == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Please enter a valid order.");
                    return errorResponse;
                }

                string? validationError = ValidateOrder(order);

                if (validationError != null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync(validationError);
                    return errorResponse;
                }

                if (string.IsNullOrWhiteSpace(order.OrderId))
                {
                    order.OrderId =
                        $"ORD-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
                }

                order.OrderId = order.OrderId.Trim();
                order.CustomerName = order.CustomerName.Trim();
                order.SelectedItemSKUs = order.SelectedItemSKUs.Select(sku => sku.Trim()).ToList();

                if (order.OrderTimestamp == default)
                {
                    order.OrderTimestamp = DateTime.UtcNow;
                }
                else if (order.OrderTimestamp.Kind == DateTimeKind.Unspecified)
                {
                    order.OrderTimestamp = DateTime.SpecifyKind(order.OrderTimestamp, DateTimeKind.Utc);
                }
                else
                {
                    order.OrderTimestamp = order.OrderTimestamp.ToUniversalTime();
                }

                await _queueService.EnqueueMessageAsync(order);

                _logger.LogInformation("Order {orderId} was placed on the order queue.", order.OrderId);

                var response = req.CreateResponse(HttpStatusCode.Accepted);

                await response.WriteAsJsonAsync(order);

                return response;
            }
            catch (JsonException)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);

                await response.WriteStringAsync("The order must be sent as valid JSON.");

                return response;
            }
            catch (Exception e) when (e is RequestFailedException || e is AggregateException)
            {
                _logger.LogError(e, "Error connecting to the order queue.");

                var response = req.CreateResponse(
                    HttpStatusCode.ServiceUnavailable);

                await response.WriteStringAsync(
                    "The order queue is currently unavailable. Please try again shortly.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error queueing order.");

                var response = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while queueing the order.");

                return response;
            }
        }

        private static string? ValidateOrder(OrderQueueMessage order)
        {
            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                return "Customer name is required.";
            }

            if (order.SelectedItemSKUs == null || order.SelectedItemSKUs.Count == 0)
            {
                return "At least one item SKU is required.";
            }

            if (order.SelectedItemSKUs.Any(sku => string.IsNullOrWhiteSpace(sku)))
            {
                return "Item SKUs cannot be empty.";
            }

            if (order.TotalPrice <= 0)
            {
                return "Total price must be greater than zero.";
            }

            if (!string.IsNullOrWhiteSpace(order.OrderId) &&
                order.OrderId.IndexOfAny(new[] { '/', '\\', '#', '?' }) >= 0)
            {
                return "Order ID cannot contain the characters / \\ # ?";
            }

            return null;
        }
    }
}
