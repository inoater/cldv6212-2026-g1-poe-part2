using Azure;
using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Azure.Data.Tables;
using System;

namespace CoffeeNChill.Models
{
    public class MenuItem : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public bool IsAvailable { get; set; }

        public override string ToString()
        {
            return $"[{PartitionKey} | {RowKey}] : {Name} - R{Price}";
        }
    }
}


 
     