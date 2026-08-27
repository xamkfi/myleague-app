using Domain.Enums.Common;

namespace Domain.Entities.Common;

/// <summary>
/// An entry shown in the public site footer (contacts, seasonal sports, or other activities).
/// </summary>
public class FooterContact : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Url { get; private set; }
    public int SortOrder { get; private set; }
    public FooterSection Section { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private FooterContact()
    {
    }

    public FooterContact(
        Guid id,
        string title,
        string? details,
        string? email,
        string? phone,
        string? url,
        int sortOrder,
        FooterSection section = FooterSection.Contact,
        string? lastModifiedBy = null)
    {
        Id = id;
        Title = ValidateTitle(title);
        Details = NormalizeOptional(details, 500, nameof(details));
        Email = ValidateEmail(email);
        Phone = NormalizeOptional(phone, 50, nameof(phone));
        Url = ValidateUrl(url);
        SortOrder = sortOrder;
        Section = ValidateSection(section);
        LastModifiedBy = NormalizeOptional(lastModifiedBy, 256, nameof(lastModifiedBy));
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        string? details,
        string? email,
        string? phone,
        string? url,
        int sortOrder,
        FooterSection section,
        string? lastModifiedBy = null)
    {
        Title = ValidateTitle(title);
        Details = NormalizeOptional(details, 500, nameof(details));
        Email = ValidateEmail(email);
        Phone = NormalizeOptional(phone, 50, nameof(phone));
        Url = ValidateUrl(url);
        SortOrder = sortOrder;
        Section = ValidateSection(section);
        LastModifiedBy = NormalizeOptional(lastModifiedBy, 256, nameof(lastModifiedBy));
        UpdatedAt = DateTime.UtcNow;
    }

    private static FooterSection ValidateSection(FooterSection section)
    {
        if (!Enum.IsDefined(section))
        {
            throw new ArgumentException("Footer section is not valid", nameof(section));
        }

        return section;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Contact title cannot be empty", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Contact title cannot exceed 200 characters", nameof(title));
        }

        return title.Trim();
    }

    private static string? ValidateEmail(string? email)
    {
        string? normalized = NormalizeOptional(email, 200, nameof(email));

        if (normalized is null)
        {
            return null;
        }

        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must contain '@'", nameof(email));
        }

        return normalized;
    }

    private static string? ValidateUrl(string? url)
    {
        string? normalized = NormalizeOptional(url, 500, nameof(url));

        if (normalized is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException("Url must be an http or https address", nameof(url));
        }

        return normalized;
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
}
