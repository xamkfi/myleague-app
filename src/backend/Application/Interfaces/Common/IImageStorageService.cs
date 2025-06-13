using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    /// <summary>
    /// Service for managing image storage operations
    /// </summary>
    public interface IImageStorageService
    {
        /// <summary>
        /// Saves an image to storage and returns its public URL
        /// </summary>
        Task<Uri> SaveImage(Stream imageStream, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an image from storage by its URL
        /// </summary>
        Task<bool> DeleteImage(Uri url, CancellationToken cancellationToken = default);
    }
}
