using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a footer contact.
/// </summary>
public class CreateFooterContactRequest
{
    /// <summary>Gets or sets the contact title.</summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets optional extra details such as an address.</summary>
    [StringLength(500)]
    public string? Details { get; set; }

    /// <summary>Gets or sets the optional email address.</summary>
    [StringLength(200)]
    public string? Email { get; set; }

    /// <summary>Gets or sets the optional phone number.</summary>
    [StringLength(50)]
    public string? Phone { get; set; }

    /// <summary>Gets or sets the optional website or other http(s) link.</summary>
    [StringLength(500)]
    public string? Url { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}

/// <summary>
/// Request model for updating a footer contact.
/// </summary>
public class UpdateFooterContactRequest
{
    /// <summary>Gets or sets the contact title.</summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets optional extra details such as an address.</summary>
    [StringLength(500)]
    public string? Details { get; set; }

    /// <summary>Gets or sets the optional email address.</summary>
    [StringLength(200)]
    public string? Email { get; set; }

    /// <summary>Gets or sets the optional phone number.</summary>
    [StringLength(50)]
    public string? Phone { get; set; }

    /// <summary>Gets or sets the optional website or other http(s) link.</summary>
    [StringLength(500)]
    public string? Url { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
