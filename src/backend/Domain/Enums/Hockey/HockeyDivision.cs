// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums.Hockey
{
    /// <summary>
    /// Represents the different division levels in hockey
    /// </summary>
    public enum HockeyDivision
    {
        /// <summary>
        /// No division assigned
        /// </summary>
        None = 0,

        /// <summary>
        /// Professional top-tier division
        /// </summary>
        Premier = 1,

        /// <summary>
        /// Second division level
        /// </summary>
        Division1 = 2,

        /// <summary>
        /// Third division level
        /// </summary>
        Division2 = 3,

        /// <summary>
        /// Fourth division level
        /// </summary>
        Division3 = 4,

        /// <summary>
        /// Fifth division level
        /// </summary>
        Division4 = 5,

        /// <summary>
        /// Youth division level
        /// </summary>
        Youth = 6,

        /// <summary>
        /// Junior division level
        /// </summary>
        Junior = 7,

        /// <summary>
        /// Veterans division level
        /// </summary>
        Veterans = 8

    }
}
