// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.FeedbackToggle.Commands;
using Application.Features.Common.FeedbackToggle.DTOs;
using Application.Features.Common.FeedbackToggle.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.FeedbackToggle.Handlers
{
    /// <summary>
    /// Handler for saving the FeedbackToggle state
    /// </summary>
    public class SaveFeedbackToggleHandler : IRequestHandler<SaveFeedbackToggleCommand, Result<FeedbackToggleDto>>
    {
        private readonly IFeedbackToggleRepository _feedbackToggleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaveFeedbackToggleHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the SaveFeedbackToggleHandler class
        /// </summary>
        /// <param name="feedbackToggleRepository">The repository used to access and manage the FeedbackToggle</param>
        /// <param name="unitOfWork"></param>
        /// <param name="logger"></param>
        public SaveFeedbackToggleHandler(IFeedbackToggleRepository feedbackToggleRepository, IUnitOfWork unitOfWork, ILogger<SaveFeedbackToggleHandler> logger)
        {
            _feedbackToggleRepository = feedbackToggleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles the SaveFeedbackToggle request
        /// </summary>
        /// <param name="command">The SaveFeedbackToggle command</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated FeedbackToggle as a DTO wrapped in a result</returns>
        public async Task<Result<FeedbackToggleDto>> Handle(SaveFeedbackToggleCommand command, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Updating FeedbackToggle state to :{command.IsEnabled}", command.IsEnabled);

                FeedbackToggleEntity? feedbackToggle = await _feedbackToggleRepository.GetToggleAsync(cancellationToken);

                //Create new toggle into database if none exist
                if(feedbackToggle == null)
                {
                    _logger.LogWarning("No FeedbackToggle found, Creating new toggle with given toggle state: {command.IsEnabled}", command.IsEnabled);
                    FeedbackToggleEntity newToggle = FeedbackToggleMapper.ToEntity(command);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _feedbackToggleRepository.SaveAsync(newToggle, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    FeedbackToggleDto toggleDto = FeedbackToggleMapper.ToDto(newToggle);

                    _logger.LogInformation("Successfully created new FeedbackToggle with ID:{newToggle.id}, State: {newToggle.IsEnabled}", toggleDto.Id, toggleDto.IsEnabled);
                    return Result<FeedbackToggleDto>.Success(toggleDto);
                }
                else //Update existing toggle state
                {
                    _logger.LogInformation("Updating toggle state with ID: {feedbackToggle.Id}, with new state: {command.IsEnabled}", feedbackToggle.Id, command.IsEnabled);
                    cancellationToken.ThrowIfCancellationRequested();

                    feedbackToggle.Update(command.IsEnabled);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _feedbackToggleRepository.SaveAsync(feedbackToggle, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    FeedbackToggleDto toggleDto = FeedbackToggleMapper.ToDto(feedbackToggle);
                    return Result<FeedbackToggleDto>.Success(toggleDto);
                }

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("FeedbackToggle update was canceled for feedback toggle state: {isEnabled}", command.IsEnabled);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating toggle with state: {isEnabled}", command.IsEnabled);
                return Result<FeedbackToggleDto>.Failure("An error occurred while updating the FeedbackToggle.");
            }
        }
    }
}
