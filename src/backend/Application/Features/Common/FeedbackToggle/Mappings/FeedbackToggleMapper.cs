// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.FeedbackToggle.Commands;
using Application.Features.Common.FeedbackToggle.DTOs;
using Domain.Entities.Common;

namespace Application.Features.Common.FeedbackToggle.Mappings
{
    /// <summary>
    /// Mapper class for the FeedbackToggle entity
    /// </summary>
    public static class FeedbackToggleMapper
    {
        /// <summary>
        /// Maps a FeedbackToggleEntity to a FeedbackToggleDto
        /// </summary>
        /// <param name="feedbackToggle">The entity to map</param>
        /// <returns>A FeedbackToggleDto representing a FeedbackToggle entity</returns>
        /// <exception cref="ArgumentNullException">Throw if FeedbackToggle is null</exception>
        public static FeedbackToggleDto ToDto(FeedbackToggleEntity feedbackToggle)
        {
            if (feedbackToggle == null) throw new ArgumentNullException(nameof(feedbackToggle));

            return new FeedbackToggleDto(
                feedbackToggle.Id,
                feedbackToggle.IsEnabled,
                feedbackToggle.CreatedAt,
                feedbackToggle.UpdatedAt);
        }
        /// <summary>
        /// Updates the FeedbackToggle with values from the UpdateFeedbackToggleCommand
        /// </summary>
        /// <param name="feedbackToggle">The feedback toggle to update</param>
        /// <param name="command">The command containing updated values</param>
        /// <exception cref="ArgumentNullException">Throw if FeedbackToggle or UpdateFeedbackToggleCommand is null</exception>
        public static void UpdateFromCommand(FeedbackToggleEntity feedbackToggle, SaveFeedbackToggleCommand command)
        {
            if(feedbackToggle == null) throw new ArgumentNullException(nameof(feedbackToggle));
            if(command == null) throw new ArgumentNullException(nameof(command));

            feedbackToggle.Update(command.IsEnabled);
        }

        /// <summary>
        /// Maps the values of the UpdateFeedbackToggleCommand to a FeedbackToggle entity
        /// </summary>
        /// <param name="command">The UpdateFeedbackToggle command</param>
        /// <returns></returns>
        public static FeedbackToggleEntity ToEntity(SaveFeedbackToggleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            Guid id = Guid.NewGuid();
            FeedbackToggleEntity toggle = new FeedbackToggleEntity(
                id,
                command.IsEnabled);

            return toggle;
        }
    }
}
