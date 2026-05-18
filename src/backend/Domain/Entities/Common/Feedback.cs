// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    public class Feedback : BaseEntity
    {
        /// <summary>
        /// Gets the title of the feedback.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets the HTML Content of the feedback.
        /// </summary>
        public string FeedbackBody { get; set; } = string.Empty;

        /// <summary>
        /// Gets the optional email of the feedback.
        /// </summary>
        public string? Email { get; set; }
    } 
}
