namespace Application.Common;

/// <summary>
/// Drops placeholder logo URLs so the UI can show a short name instead of a broken image.
/// </summary>
public static class PublicLogoUrl
{
    public static Uri? OmitPlaceholder(Uri? uri)
    {
        if (uri is null)
        {
            return null;
        }

        string host = uri.Host;
        if (host.Equals("example.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".example.com", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return uri;
    }
}
