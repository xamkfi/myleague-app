// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Search.DTOs
{
    public record GlobalSearchResultTeamDto(
        Guid TeamId,
        string TeamName,
        Guid? ClubId,
        string? ClubName
        );
}
