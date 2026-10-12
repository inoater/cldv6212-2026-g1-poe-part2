using CoffeeNChill.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _blobContainerClient;

        private BlobService(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public static async Task<BlobService> CreateAsync(string connectionString, string containerName)
        {
            var blobContainerClient = new BlobContainerClient(connectionString, containerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            return new BlobService(blobContainerClient);
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName, string? contentType)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrEmpty(contentType)
                        ? "application/octet-stream"
                        : contentType
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);
        }

        public async Task<List<StaffDocument>> GetAllFilesAsync()
        {
            var allFiles = new List<StaffDocument>();

            await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync())
            {
                StaffDocument document = new StaffDocument();

                document.FileName = blobItem.Name;
                document.Size = blobItem.Properties.ContentLength ?? 0;
                document.LastModified = blobItem.Properties.LastModified;

                allFiles.Add(document);
            }

            return allFiles;
        }

        public async Task<(Stream? Content, string? ContentType)> DownloadFileAsync(string fileName)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return (null, null);
            }

            BlobDownloadStreamingResult download = await blobClient.DownloadStreamingAsync();

            return (download.Content, download.Details.ContentType);
        }
    }
}