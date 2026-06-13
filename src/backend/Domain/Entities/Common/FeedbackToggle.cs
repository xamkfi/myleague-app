// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents wether or not you can submit feedback
    /// This class manages the functionality of toggling wether or not you can submit feedback
    /// </summary>
    public class FeedbackToggleEntity : BaseEntity
    {
        /// <summary>
        /// Gets wether or not feedback is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// initializes a new instance of the FeedbackToggle
        /// </summary>
        /// <param name="isEnabled">Determines if feedback is enabled</param>
        public FeedbackToggleEntity(Guid id, bool isEnabled)
        {
            Id = id;
            IsEnabled = isEnabled;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the state of the FeedbackToggle entity
        /// </summary>
        /// <param name="isEnabled">The new state</param>
        public void Update(bool isEnabled)
        {
            IsEnabled = isEnabled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
