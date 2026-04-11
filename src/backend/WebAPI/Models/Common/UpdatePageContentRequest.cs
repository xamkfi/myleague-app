// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace WebAPI.Models.Common
{
    /// <summary>
    /// Request model for updating page content
    /// </summary>
    public class UpdatePageContentRequest
    {
        /// <summary>
        /// Page title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Page content in HTML format
        /// </summary>
        public string ContentHtml { get; set; } = string.Empty;
    }
}
