using Azure.Storage.Queues.Models;
using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class ProcessOrderQueue
    {
        private readonly ILogger<ProcessOrderQueue> _logger;
        private readonly OrderService _orderService;

        public ProcessOrderQueue(
            ILogger<ProcessOrderQueue> logger,
            OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("ProcessOrderQueue")]
        public async Task Run(
            [QueueTrigger("order-processing-queue")] QueueMessage message)
        {
            _logger.LogInformation(
                "Processing order message {messageId} (attempt {dequeueCount}).",
                message.MessageId,
                message.DequeueCount);

            try
            {
                var queuedOrder =
                    JsonSerializer.Deserialize<OrderQueueMessage>(message.Body);

                if (queuedOrder == null || string.IsNullOrWhiteSpace(queuedOrder.OrderId))
                {
                    throw new InvalidOperationException(
                        "The order message is empty or is missing an OrderId.");
                }

                DateTime orderTimestamp = queuedOrder.OrderTimestamp == default
                    ? DateTime.UtcNow
                    : DateTime.SpecifyKind(queuedOrder.OrderTimestamp, DateTimeKind.Utc);

                var newOrder = new OrderEntity
                {
                    PartitionKey = orderTimestamp.ToString("yyyy-MM-dd"),
                    RowKey = queuedOrder.OrderId,
                    CustomerName = queuedOrder.CustomerName,
                    SelectedItemSKUs = string.Join(",", queuedOrder.SelectedItemSKUs),
                    TotalPrice = queuedOrder.TotalPrice,
                    OrderTimestamp = new DateTimeOffset(orderTimestamp),
                    Status = "Received",
                    StatusUpdatedAt = DateTimeOffset.UtcNow
                };

                await _orderService.AddOrderAsync(newOrder);

                _logger.LogInformation(
                    "Order {orderId} was logged with status {status}.",
                    newOrder.RowKey,
                    newOrder.Status);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Error processing order message {messageId} on attempt {dequeueCount}.",
                    message.MessageId,
                    message.DequeueCount);

                throw;
            }
        }
    }
}
