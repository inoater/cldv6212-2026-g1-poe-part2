using CoffeeNChill.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace CoffeeNChill.Functions
{
    public class UploadStaffDocument
    {
        private readonly ILogger<UploadStaffDocument> _logger;
        private readonly BlobService _blobService;

        public UploadStaffDocument(
            ILogger<UploadStaffDocument> logger,
            BlobService blobService)
        {
            _logger = logger;
            _blobService = blobService;
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")]
            HttpRequest req)
        {
            _logger.LogInformation("Uploading a staff document.");

            try
            {
                if (!req.HasFormContentType || req.Form.Files.Count == 0)
                {
                    return new BadRequestObjectResult(
                        "Please attach a file using multipart/form-data with a 'file' key.");
                }

                IFormFile file = req.Form.Files[0];

                if (file.Length == 0)
                {
                    return new BadRequestObjectResult("The uploaded file is empty.");
                }

                using (Stream fileStream = file.OpenReadStream())
                {
                    await _blobService.UploadFileAsync(
                        fileStream,
                        file.FileName,
                        file.ContentType);
                }

                return new OkObjectResult(
                    $"Staff document '{file.FileName}' uploaded successfully.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading staff document.");

                return new ObjectResult(
                    "An error occurred while uploading the document.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
