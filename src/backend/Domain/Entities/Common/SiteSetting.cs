using Domain.Entities;

namespace Domain.Entities.Common;

/// <summary>
/// General-purpose key-value setting entry for site-level configurable content.
/// </summary>
public class SiteSetting : BaseEntity
{
    /// <summary>
    /// Gets the setting key.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the serialized JSON value of the setting.
    /// </summary>
    public string ValueJson { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user identifier that last modified the setting.
    /// </summary>
    public string? LastModifiedBy { get; private set; }

    private SiteSetting() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSetting"/> class.
    /// </summary>
    public SiteSetting(Guid id, string key, string valueJson, string? lastModifiedBy)
    {
        Id = id;
        Key = ValidateRequired(key, nameof(Key), 150);
        ValueJson = ValidateRequired(valueJson, nameof(ValueJson), int.MaxValue);
        LastModifiedBy = Normalize(lastModifiedBy, 100);
    }

    /// <summary>
    /// Updates the JSON value and modifier metadata.
    /// </summary>
    public void UpdateValue(string valueJson, string? modifiedBy)
    {
        ValueJson = ValidateRequired(valueJson, nameof(ValueJson), int.MaxValue);
        LastModifiedBy = Normalize(modifiedBy, 100);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string ValidateRequired(string value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} cannot be empty.");

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{field} cannot exceed {maxLength} characters.");

        return trimmed;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
