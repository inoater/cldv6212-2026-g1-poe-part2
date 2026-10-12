using Azure.Storage.Queues;
using System.Text.Json;

namespace CoffeeNChill.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        private QueueService(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public static async Task<QueueService> CreateAsync(string connectionString, string queueName)
        {
            var queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

            await queueClient.CreateIfNotExistsAsync();

            return new QueueService(queueClient);
        }

        public async Task EnqueueMessageAsync<T>(T payload)
        {
            string message = JsonSerializer.Serialize(payload);

            await _queueClient.SendMessageAsync(message);
        }
    }
}
