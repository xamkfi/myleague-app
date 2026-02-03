using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Services.ImageStorage
{
    /// <summary>
    /// Azure Blob Storage service using connection string authentication.
    /// Reads ConnectionStrings:AzureBlobStorage and AzureStorage:ContainerName from configuration.
    /// </summary>
    public class AzureBlobImageStorageService : IImageStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobImageStorageService> _logger;

        public AzureBlobImageStorageService(IConfiguration configuration, ILogger<AzureBlobImageStorageService> logger)
        {
            _logger = logger;

            string connectionString = configuration.GetConnectionString("AzureBlobStorage")
                ?? throw new InvalidOperationException("ConnectionStrings:AzureBlobStorage configuration is required");
            string containerName = configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException("AzureStorage:ContainerName configuration is required");

            // Create the blob service client with connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            _logger.LogInformation("Azure Blob Storage configured with connection string, Container={ContainerName}", containerName);
        }

        public async Task<Uri> SaveImage(Stream imageStream, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);

                // Determine content type from file extension
                string contentType = GetContentTypeFromFileName(fileName);

                BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                };

                // Upload with properties
                await blobClient.UploadAsync(
                    imageStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    },
                    cancellationToken);

                _logger.LogInformation("Image uploaded to Azure Blob Storage: {Uri}", blobClient.Uri);
                return blobClient.Uri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image to Azure Blob Storage: {FileName}", fileName);
                throw new InvalidOperationException("Failed to upload image to Azure blob storage", ex);
            }
        }

        public async Task<bool> DeleteImage(Uri url, CancellationToken cancellationToken)
        {
            try
            {
                // Extract blob name from URL
                string blobName = ExtractBlobNameFromUri(url);
                if (string.IsNullOrEmpty(blobName))
                {
                    _logger.LogWarning("Could not extract blob name from URI: {Uri}", url);
                    return false;
                }

                BlobClient blobClient = _containerClient.GetBlobClient(blobName);
                bool deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                if (deleted)
                {
                    _logger.LogInformation("Image deleted from Azure Blob Storage: {Uri}", url);
                }
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image from Azure Blob Storage: {Uri}", url);
                throw new InvalidOperationException("Failed to delete an image from Azure blob storage", ex);
            }
        }

        private static string GetContentTypeFromFileName(string fileName)
        {
            string? extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        private string ExtractBlobNameFromUri(Uri uri)
        {
            // URI format: https://{account}.blob.core.windows.net/{container}/{blobName}
            // We need to extract the blob name (everything after the container)
            string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                // Skip the container name (first segment), return the blob name (second segment)
                return segments[1];
            }
            return string.Empty;
        }
    }
}
