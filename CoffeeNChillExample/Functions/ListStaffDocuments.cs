using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoffeeNChill.Functions
{

    public class ListStaffDocuments
    {
        private readonly ILogger<ListStaffDocuments> _logger;
        private readonly BlobService _blobService;

        public ListStaffDocuments(
            ILogger<ListStaffDocuments> logger,
            BlobService blobService)
        {
            _logger = logger;
            _blobService = blobService;
        }

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "documents")]
            HttpRequestData req)
        {
            try
            {
                var documents =
                    await _blobService.GetAllFilesAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(documents);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing staff documents.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while listing documents.");

                return response;
            }
        }
    }
}
