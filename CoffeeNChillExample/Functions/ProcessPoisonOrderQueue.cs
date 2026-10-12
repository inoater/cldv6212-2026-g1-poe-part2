using Azure.Storage.Queues.Models;
using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class ProcessPoisonOrderQueue
    {
        private readonly ILogger<ProcessPoisonOrderQueue> _logger;
        private readonly FailedOrderService _failedOrderService;

        public ProcessPoisonOrderQueue(
            ILogger<ProcessPoisonOrderQueue> logger,
            FailedOrderService failedOrderService)
        {
            _logger = logger;
            _failedOrderService = failedOrderService;
        }

        [Function("ProcessPoisonOrderQueue")]
        public async Task Run(
            [QueueTrigger("order-processing-queue-poison")] QueueMessage message)
        {
            string rawMessage = message.Body.ToString();

            _logger.LogCritical("Poisoned order message arrived: {messageText}", rawMessage);

            string failureReason = "Reached max dequeue attempts";

            try
            {
                var order = JsonSerializer.Deserialize<OrderQueueMessage>(rawMessage);

                if (order == null || string.IsNullOrWhiteSpace(order.OrderId))
                {
                    failureReason = "Message was missing required order fields";
                }
            }
            catch (JsonException)
            {
                failureReason = "Message was not valid order JSON";
            }

            var failedOrder = new FailedOrderEntity
            {
                Payload = rawMessage,
                FailureReason = failureReason,
                DequeueCount = message.DequeueCount,
                EnqueuedTime = message.InsertedOn
            };

            await _failedOrderService.AddFailedOrderAsync(failedOrder);

            _logger.LogInformation(
                "Poisoned order message {messageId} was stored in the FailedOrders table.",
                message.MessageId);
        }
    }
}
