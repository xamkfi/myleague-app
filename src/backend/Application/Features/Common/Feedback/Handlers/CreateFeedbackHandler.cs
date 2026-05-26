// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Feedback.Commands;
using Application.Features.Common.Feedback.DTOs;
using Application.Features.Common.Feedback.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using MediatR.Wrappers;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Feedback.Handlers
{
    /// <summary>
    /// Handler for creating feedback
    /// </summary>
    public class CreateFeedbackHandler : IRequestHandler<CreateFeedbackCommand,Result<FeedbackDto>>
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateFeedbackHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the CreateFeedbackHandler class.
        /// </summary>
        /// <param name="feedbackRepository">The repository used to access and manage feedback data.</param>
        /// <param name="unitOfWork">Unit of work used to commit repository changes as a single transaction.</param>
        /// <param name="logger">Logger for recording handler operations and diagnostic information.</param>
        public CreateFeedbackHandler(IFeedbackRepository feedbackRepository, IUnitOfWork unitOfWork, ILogger<CreateFeedbackHandler> logger)
        {
            _feedbackRepository = feedbackRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the CreateFeedbackCommand request
        /// </summary>
        /// <param name="request">The CreateFeedbackCommand with the values for the new FeedbackEntity</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The new FeedbackEntity as a data transfer object</returns>
        public async Task<Result<FeedbackDto>> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Check for cancellation before starting
                cancellationToken.ThrowIfCancellationRequested();

                
                _logger.LogInformation("Creating new feedback with title: {title}", request.Title);

                //Map the request into FeedbackEntity with mapper
                FeedbackEntity feedback = FeedbackMapper.ToEntity(request);

                _logger.LogInformation("Creating feedback with Id: {id}", feedback.Id);

                //Check for cancellation before database operations
                cancellationToken.ThrowIfCancellationRequested();

                //Save the entity
                await _feedbackRepository.SaveAsync(feedback, cancellationToken);

                //Check for cancellation before completing the transaction
                cancellationToken.ThrowIfCancellationRequested();

                //save changes explicitly to trigger domain events
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //Map the feedback back into the DTO and return
                FeedbackDto feedbackDto = FeedbackMapper.ToDto(feedback);

                _logger.LogInformation("Successfully created new feedback with ID: {id}", feedback.Id);

                return Result<FeedbackDto>.Success(feedbackDto);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Feedback creation was cancelled for title: {title}", request.Title);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operational error occurred while creating the feedback: {title}", request.Title);
                return Result<FeedbackDto>.Failure("An error occurred while creating the feedback");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid feedback data provided for title: {title}", request.Title);
                return Result<FeedbackDto>.Failure("An error occurred while creating the feedback");
            }
            catch(Exception ex) when (ex is not SystemException)
            {
                _logger.LogError(ex, "Unexpected non-system error occurred while creating the feedback: {title}", request.Title);
                throw;
            }
        }
    }
}
