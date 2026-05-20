// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Feedback.Commands;
using Application.Features.Common.Feedback.DTOs;
using Domain.Entities.Common;

namespace Application.Features.Common.Feedback.Mappings
{
    /// <summary>
    /// Mapper class for feedback entity and DTOs
    /// </summary>
    public static class FeedbackMapper
    {
        public static FeedbackDto ToDto(FeedbackEntity feedback)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));

            return new FeedbackDto(
                feedback.Id,
                feedback.Title,
                feedback.FeedbackBody,
                feedback.Email?.ToString(),
                feedback.CreatedAt);
        }

        public static FeedbackEntity ToEntity(CreateFeedbackCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            Guid feedbackId = Guid.NewGuid();
            FeedbackEntity feedbackEntity = new FeedbackEntity(feedbackId,
                command.Title,
                command.FeedbackBody);

            if (!string.IsNullOrEmpty(command.Email))
            {
                feedbackEntity.SetEmail(command.Email);
            }

            return feedbackEntity;
        }

        public static FeedbackListDto ToListDto(FeedbackEntity feedback)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));

            return new FeedbackListDto(
                feedback.Id,
                feedback.Title,
                feedback.Email?.ToString(),
                feedback.CreatedAt);
        }

        public static IEnumerable<FeedbackListDto> ToListDtos(IEnumerable<FeedbackEntity> feedbackList)
        {
            if (feedbackList == null) throw new ArgumentNullException(nameof(feedbackList));
            return feedbackList.Select(feedback => ToListDto(feedback));
        }
    }
}
