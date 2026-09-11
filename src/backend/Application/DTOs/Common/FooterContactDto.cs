using Domain.Enums.Common;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for a footer contact entry.
/// </summary>
public class FooterContactDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Url { get; set; }
    public int SortOrder { get; set; }
    public FooterSection Section { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
