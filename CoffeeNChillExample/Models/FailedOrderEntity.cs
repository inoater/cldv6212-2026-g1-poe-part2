using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Models
{
    public class FailedOrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "PoisonQueue";

        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string Payload { get; set; } = string.Empty;

        public string FailureReason { get; set; } = string.Empty;

        public long DequeueCount { get; set; }

        public DateTimeOffset? EnqueuedTime { get; set; }
    }
}
