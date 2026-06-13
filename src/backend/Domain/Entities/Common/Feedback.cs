// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    public class FeedbackEntity : BaseEntity
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

        /// <summary>
        /// Creates a FeedbackEntity with specified parameters
        /// </summary>
        /// <param name="id">Id of the feedback</param>
        /// <param name="title">Title of the feedback with max 255 characters</param>
        /// <param name="feedbackBody">HTML content of the feedback</param>
        /// <param name="email">Optional email of the feedback with max 255 characters</param>
        public FeedbackEntity(Guid id, string title, string feedbackBody, string? email = null)
        {
            Id = id;
            Title = title;
            FeedbackBody = feedbackBody;
            Email = email;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the email.
        /// </summary>
        /// <param name="email">The new email to be set</param>
        public void SetEmail(string email)
        {
            Email = email;
        }
    } 
}
