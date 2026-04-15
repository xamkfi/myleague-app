// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain.Entities.Common
{
    public class PageContent : BaseEntity
    {
        public string PageSlug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public string? LastModifiedBy { get; set; }
    }
}
