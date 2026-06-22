using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for updating footer contact settings.
/// </summary>
public record UpdateFooterContactRequest
{
    /// <summary>
    /// Organization display name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string OrganizationName { get; init; } = string.Empty;

    /// <summary>
    /// Organization address block.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string OrganizationAddress { get; init; } = string.Empty;

    /// <summary>
    /// Contact persons shown in footer.
    /// </summary>
    [Required]
    public IReadOnlyList<UpdateFooterContactPersonRequest> ContactPersons { get; init; } = [];
}

/// <summary>
/// Request model for a single footer contact person.
/// </summary>
public record UpdateFooterContactPersonRequest
{
    /// <summary>
    /// Person name or role text.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string NameOrRole { get; init; } = string.Empty;

    /// <summary>
    /// Contact email.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contact phone number.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Phone { get; init; } = string.Empty;
}
