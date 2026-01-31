using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Services.ImageStorage
{
    /// <summary>
    /// Saves images to wwwroot/uploads for local development when Azure Blob is not configured.
    /// </summary>
    public class LocalFileImageStorageService : IImageStorageService
    {
        private const string UploadsFolder = "uploads";
        private const string BaseUrlConfigKey = "App:BaseUrl";
        private const string DefaultBaseUrl = "http://localhost:8080";
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileImageStorageService> _logger;

        public LocalFileImageStorageService(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<LocalFileImageStorageService> logger)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Uri> SaveImage(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
        {
            string webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            string uploadsPath = Path.Combine(webRoot, UploadsFolder);
            Directory.CreateDirectory(uploadsPath);

            string filePath = Path.Combine(uploadsPath, fileName);
            await using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await imageStream.CopyToAsync(fileStream, cancellationToken);
            }

            string baseUrl = GetBaseUrl();
            Uri imageUrl = new Uri($"{baseUrl.TrimEnd('/')}/{UploadsFolder}/{fileName}", UriKind.Absolute);
            _logger.LogInformation("Image saved locally: {Path} -> {Url}", filePath, imageUrl);
            return imageUrl;
        }

        public Task<bool> DeleteImage(Uri url, CancellationToken cancellationToken = default)
        {
            string pathSegment = url.AbsolutePath.TrimStart('/');
            if (!pathSegment.StartsWith(UploadsFolder + "/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Delete requested for non-local URL: {Url}", url);
                return Task.FromResult(false);
            }

            string fileName = pathSegment.Substring(UploadsFolder.Length + 1);
            if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
            {
                return Task.FromResult(false);
            }

            string webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            string uploadsPath = Path.Combine(webRoot, UploadsFolder);
            string filePath = Path.Combine(uploadsPath, fileName);
            if (!File.Exists(filePath))
            {
                return Task.FromResult(false);
            }

            try
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted local image: {Path}", filePath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete local image: {Path}", filePath);
                return Task.FromResult(false);
            }
        }

        private string GetBaseUrl()
        {
            HttpContext? context = _httpContextAccessor.HttpContext;
            if (context?.Request != null)
            {
                return $"{context.Request.Scheme}://{context.Request.Host}";
            }
            return _configuration[BaseUrlConfigKey]?.TrimEnd('/') ?? DefaultBaseUrl;
        }
    }
}
