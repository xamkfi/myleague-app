// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application.DTOs.Common;

public class PageContentDto
{
    public Guid Id { get; set; }
    public string PageSlug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
