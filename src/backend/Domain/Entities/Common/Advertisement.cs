using Domain.Enums.Common;

namespace Domain.Entities.Common;

/// <summary>
/// An image advertisement shown in one of the public home page ad slots.
/// </summary>
public class Advertisement : BaseEntity
{
    public const int NameMaxLength = 100;
    public const int AltTextMaxLength = 200;
    public const int UrlMaxLength = 500;

    public string Name { get; private set; } = string.Empty;
    public Uri ImageUrl { get; private set; } = null!;
    public Uri? LinkUrl { get; private set; }
    public string? AltText { get; private set; }
    public AdSlot Slot { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }

    private Advertisement()
    {
    }

    public Advertisement(
        Guid id,
        string name,
        Uri imageUrl,
        Uri? linkUrl,
        string? altText,
        AdSlot slot,
        bool isActive,
        int sortOrder,
        DateTime? startsAt,
        DateTime? endsAt)
        : base(id)
    {
        Apply(name, imageUrl, linkUrl, altText, slot, isActive, sortOrder, startsAt, endsAt);
    }

    public void Update(
        string name,
        Uri imageUrl,
        Uri? linkUrl,
        string? altText,
        AdSlot slot,
        bool isActive,
        int sortOrder,
        DateTime? startsAt,
        DateTime? endsAt)
    {
        Apply(name, imageUrl, linkUrl, altText, slot, isActive, sortOrder, startsAt, endsAt);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// True when the advertisement is active and its schedule includes <paramref name="utcNow"/>.
    /// The start is inclusive and the end is exclusive.
    /// </summary>
    public bool IsVisibleAt(DateTime utcNow)
    {
        return IsActive
            && (StartsAt is null || StartsAt.Value <= utcNow)
            && (EndsAt is null || EndsAt.Value > utcNow);
    }

    private void Apply(
        string name,
        Uri imageUrl,
        Uri? linkUrl,
        string? altText,
        AdSlot slot,
        bool isActive,
        int sortOrder,
        DateTime? startsAt,
        DateTime? endsAt)
    {
        DateTime? start = ToUtc(startsAt);
        DateTime? end = ToUtc(endsAt);

        if (start.HasValue && end.HasValue && end.Value <= start.Value)
        {
            throw new ArgumentException("Advertisement end time must be after its start time", nameof(endsAt));
        }

        if (!Enum.IsDefined(slot))
        {
            throw new ArgumentException("Advertisement slot is not valid", nameof(slot));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));
        }

        Name = ValidateName(name);
        ImageUrl = ValidateHttpUrl(imageUrl, nameof(imageUrl))
            ?? throw new ArgumentException("Advertisement image is required", nameof(imageUrl));
        LinkUrl = ValidateHttpUrl(linkUrl, nameof(linkUrl));
        AltText = NormalizeOptional(altText, AltTextMaxLength, nameof(altText));
        Slot = slot;
        IsActive = isActive;
        SortOrder = sortOrder;
        StartsAt = start;
        EndsAt = end;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Advertisement name cannot be empty", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Advertisement name cannot exceed {NameMaxLength} characters", nameof(name));
        }

        return trimmed;
    }

    private static Uri? ValidateHttpUrl(Uri? url, string paramName)
    {
        if (url is null)
        {
            return null;
        }

        if (!url.IsAbsoluteUri
            || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(url.Host))
        {
            throw new ArgumentException("Url must be an http or https address", paramName);
        }

        if (url.OriginalString.Length > UrlMaxLength)
        {
            throw new ArgumentException($"Url cannot exceed {UrlMaxLength} characters", paramName);
        }

        return url;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters", paramName);
        }

        return trimmed;
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
        };
    }
}
