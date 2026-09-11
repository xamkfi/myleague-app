// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for InfoPageContent entity
/// </summary>
public class InfoPageContentDto
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the page slug identifier
    /// </summary>
    public string PageSlug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page HTML content
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the last modifier
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
