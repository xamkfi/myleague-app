// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Floorball
{
    /// <summary>
    /// Lightweight DTO for returning just the team ID and name for quick search purposes
    /// </summary>
    public class FloorballTeamNameDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
