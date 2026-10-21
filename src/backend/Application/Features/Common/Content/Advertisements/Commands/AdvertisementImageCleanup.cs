using Application.Interfaces.Common;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Content.Advertisements.Commands;

/// <summary>
/// Removes an advertisement image that is no longer referenced. The database change has
/// already been saved, so a storage failure is logged and does not fail the request.
/// </summary>
internal static class AdvertisementImageCleanup
{
    public static async Task TryDeleteAsync(
        IImageStorageService imageStorageService,
        ILogger logger,
        Uri imageUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            await imageStorageService.DeleteImage(imageUrl, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Could not delete advertisement image {ImageUrl}", imageUrl);
        }
    }
}
