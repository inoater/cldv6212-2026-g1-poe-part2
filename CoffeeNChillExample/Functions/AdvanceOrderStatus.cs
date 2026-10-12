using Azure;
using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class AdvanceOrderStatus
    {
        private const int SecondsInEachStatus = 10;

        private readonly ILogger<AdvanceOrderStatus> _logger;
        private readonly OrderService _orderService;

        public AdvanceOrderStatus(
            ILogger<AdvanceOrderStatus> logger,
            OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("AdvanceOrderStatus")]
        public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            List<OrderEntity> activeOrders = await _orderService.GetActiveOrdersAsync();

            foreach (OrderEntity order in activeOrders)
            {
                TimeSpan timeInStatus = DateTimeOffset.UtcNow - order.StatusUpdatedAt;

                if (timeInStatus < TimeSpan.FromSeconds(SecondsInEachStatus))
                {
                    continue;
                }

                string previousStatus = order.Status;

                switch (order.Status)
                {
                    case "Received":
                        order.Status = "Preparing";
                        break;
                    case "Preparing":
                        order.Status = "Ready";
                        break;
                    case "Ready":
                        order.Status = "Collected";
                        break;
                    default:
                        _logger.LogWarning(
                            "Unknown status {status} for order {orderId}.",
                            order.Status,
                            order.RowKey);
                        continue;
                }

                order.StatusUpdatedAt = DateTimeOffset.UtcNow;

                try
                {
                    await _orderService.UpdateOrderAsync(order);

                    _logger.LogInformation(
                        "Order {orderId} moved from {previousStatus} to {newStatus}.",
                        order.RowKey,
                        previousStatus,
                        order.Status);
                }
                catch (RequestFailedException e)
                {
                    _logger.LogWarning(
                        e,
                        "Could not update order {orderId}, it will be retried on the next run.",
                        order.RowKey);
                }
            }
        }
    }
}
