// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.FeedbackToggle.DTOs;
using Application.Features.Common.FeedbackToggle.Mappings;
using Application.Features.Common.FeedbackToggle.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.FeedbackToggle.Handlers
{
    /// <summary>
    /// Handler for getting the FeedbackToggle
    /// </summary>
    public class GetFeedbackToggleHandler : IRequestHandler<GetFeedbackToggleQuery, Result<FeedbackToggleDto>>
    {
        private readonly IFeedbackToggleRepository _feedbackToggleRepository;
        private readonly ILogger<GetFeedbackToggleHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the GetFeedbackToggleHandler
        /// </summary>
        /// <param name="feedbackToggleRepository">The FeedbackToggle repository</param>
        /// <param name="logger">The logger</param>
        public GetFeedbackToggleHandler(IFeedbackToggleRepository feedbackToggleRepository, ILogger<GetFeedbackToggleHandler> logger)
        {
            _feedbackToggleRepository = feedbackToggleRepository;
            _logger = logger;
        }

        public async Task<Result<FeedbackToggleDto>> Handle(GetFeedbackToggleQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving the FeedbackToggle");

                FeedbackToggleEntity? feedbackToggle = await _feedbackToggleRepository.GetToggleAsync(cancellationToken);
                if (feedbackToggle != null)
                {
                    FeedbackToggleDto toggleDto = FeedbackToggleMapper.ToDto(feedbackToggle);
                    return Result<FeedbackToggleDto>.Success(toggleDto);
                }
                else
                {
                    _logger.LogWarning("Could not find FeedbackToggle in database.");
                    return Result<FeedbackToggleDto>.Failure("FeedbackToggle not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving the toggle");
                return Result<FeedbackToggleDto>.Failure("An error occurred while retrieving the feedback toggle");
            }
        }
    }
}
