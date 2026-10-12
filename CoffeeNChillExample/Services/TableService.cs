using CoffeeNChill.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace CoffeeNChill.Services
{
    public class TableService
    {
        private readonly TableClient _tableClient;

        private TableService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public static async Task<TableService> CreateAsync(string connectionString, string tableName)
        {
            var tableClient = new TableClient(connectionString, tableName);

            await tableClient.CreateIfNotExistsAsync();

            return new TableService(tableClient);
        }

        public async Task AddEntityAsync(MenuItem newItem)
        {
            await _tableClient.AddEntityAsync(newItem);
        }

        public async Task<List<MenuItem>> GetAllEntitiesAsync()
        {
            var allRecords = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>())
            {
                allRecords.Add(item);
            }

            return allRecords;
        }

        public async Task<List<MenuItem>> GetEntitiesByCategoryAsync(string category)
        {
            var allRecords = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>(
                item => item.PartitionKey == category))
            {
                allRecords.Add(item);
            }

            return allRecords;
        }

        public async Task<MenuItem?> GetEntityAsync(string category, string id)
        {
            try
            {
                MenuItem item = await _tableClient.GetEntityAsync<MenuItem>(
                    category,
                    id);

                return item;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateEntityAsync(MenuItem item)
        {
            await _tableClient.UpdateEntityAsync(
                item,
                item.ETag,
                TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync(string category, string id)
        {
            await _tableClient.DeleteEntityAsync(category, id);
        }   
    }
}