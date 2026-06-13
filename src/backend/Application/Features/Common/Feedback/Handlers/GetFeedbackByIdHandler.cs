// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Feedback.DTOs;
using Application.Features.Common.Feedback.Mappings;
using Application.Features.Common.Feedback.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Feedback.Handlers
{
    /// <summary>
    /// Handler for getting feedback by given id
    /// </summary>
    public class GetFeedbackByIdHandler : IRequestHandler<GetFeedbackByIdQuery, Result<FeedbackDto>>
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ILogger<GetFeedbackByIdHandler> _logger;

        /// <summary>
        /// Initializes a new instance of GetFeedbackByIdHandler
        /// </summary>
        /// <param name="feedbackRepository">The repository used to access and manage feedback data</param>
        /// <param name="logger">The logger</param>
        public GetFeedbackByIdHandler(IFeedbackRepository feedbackRepository, ILogger<GetFeedbackByIdHandler> logger)
        {
            _feedbackRepository = feedbackRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetFeedbackById request
        /// </summary>
        /// <param name="request">The GetFeedbackByIdQuery request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<Result<FeedbackDto>> Handle(GetFeedbackByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Retrieving feedback with ID: {request.id}", request.id);

                FeedbackEntity? feedback = await _feedbackRepository.GetFeedbackByIdAsync(request.id);

                if (feedback == null)
                {
                    _logger.LogWarning("Feedback with ID: {request.id} not found.", request.id);
                    return Result<FeedbackDto>.Failure($"Feedback with ID: {request.id} not found.");
                }
                FeedbackDto feedbackDto = FeedbackMapper.ToDto(feedback);
                _logger.LogInformation($"Successfully retrieved feedback with title: {feedback.Title}");

                return Result<FeedbackDto>.Success(feedbackDto);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Get feedback operation was canceled for ID: {request.id}", request.id);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving feedback with id: {request.id}", request.id);
                return Result<FeedbackDto>.Failure("An error occurred while retrieving the feedback");
            }
        }
    }
}
