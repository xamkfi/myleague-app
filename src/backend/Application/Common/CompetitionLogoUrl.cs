namespace Application.Common;

/// <summary>
/// Parses optional competition logo URLs for season and tournament commands.
/// </summary>
public static class CompetitionLogoUrl
{
    public static Uri? Parse(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(logoUrl.Trim(), UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException("Logo url must be an http or https address", nameof(logoUrl));
        }

        if (uri.OriginalString.Length > 500)
        {
            throw new ArgumentException("Logo url cannot exceed 500 characters", nameof(logoUrl));
        }

        return uri;
    }

    public static string? ToPublicString(Uri? logoUrl) => logoUrl?.ToString();

    public static bool IsValidOptional(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
        {
            return true;
        }

        try
        {
            Parse(logoUrl);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
