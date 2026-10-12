using Azure.Data.Tables;
using CoffeeNChill.Models;

namespace CoffeeNChill.Services
{
    public class FailedOrderService
    {
        private readonly TableClient _tableClient;

        private FailedOrderService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public static async Task<FailedOrderService> CreateAsync(string connectionString, string tableName)
        {
            var tableClient = new TableClient(connectionString, tableName);

            await tableClient.CreateIfNotExistsAsync();

            return new FailedOrderService(tableClient);
        }

        public async Task AddFailedOrderAsync(FailedOrderEntity failedOrder)
        {
            await _tableClient.AddEntityAsync(failedOrder);
        }

        public async Task<List<FailedOrderEntity>> GetAllFailedOrdersAsync()
        {
            var allFailedOrders = new List<FailedOrderEntity>();

            await foreach (FailedOrderEntity failedOrder in _tableClient.QueryAsync<FailedOrderEntity>())
            {
                allFailedOrders.Add(failedOrder);
            }

            return allFailedOrders;
        }
    }
}
