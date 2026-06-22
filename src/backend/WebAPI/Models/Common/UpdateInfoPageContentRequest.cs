// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for updating info page content
/// </summary>
public class UpdateInfoPageContentRequest
{
    /// <summary>
    /// Gets or sets the page title
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page HTML content
    /// </summary>
    [Required]
    public string ContentHtml { get; set; } = string.Empty;
}
