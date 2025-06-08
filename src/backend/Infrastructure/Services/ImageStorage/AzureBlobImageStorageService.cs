using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace MyLeague.Infrastructure.Services.ImageStorage
{
    public class AzureBlobImageStorageService : IImageStorageService
    {
        private readonly string _blobSasKey;
        public AzureBlobImageStorageService(IConfiguration configuration)
        {
            _blobSasKey = configuration.GetConnectionString("AzureBlobSasUrl")
                ?? throw new InvalidOperationException("AzureBlobSasUrl connection string is required");
        }

        public async Task<Uri> SaveImage(Stream imageStream, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                // Create container client from SAS URL
                BlobContainerClient containerClient = new BlobContainerClient(new Uri(_blobSasKey));

                // Get blob client for the specific file
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpeg",
                    CacheControl = "public, max-age=31536000"  // 1 year cache
                };

                // Upload with properties
                await blobClient.UploadAsync(
                    imageStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    },
                    cancellationToken);

                return blobClient.Uri;

            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Failed to upload image to Azure blob storage", ex);
            }
        }

        public async Task<bool> DeleteImage(Uri url, CancellationToken cancellationToken)
        {
            try
            {
                BlobClient blobClient = new BlobClient(url);

                bool response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                return response;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Failed to delete an image from Azure blob storage");
            }
        }

    }
}
