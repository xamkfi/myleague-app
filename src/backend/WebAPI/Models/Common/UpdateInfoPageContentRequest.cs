// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

public class UpdateInfoPageContentRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ContentHtml { get; set; } = string.Empty;
}
