// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Feedback.Commands;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Feedback.Handlers
{
    /// <summary>
    /// Handler for deleting feedback
    /// </summary>
    public class DeleteFeedbackHandler : IRequestHandler<DeleteFeedbackCommand, Result<bool>>
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteFeedbackHandler> _logger;

        public DeleteFeedbackHandler(IFeedbackRepository feedbackRepository, IUnitOfWork unitOfWork, ILogger<DeleteFeedbackHandler> logger)
        {
            _feedbackRepository = feedbackRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteFeedbackCommand request,  CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _feedbackRepository.ExistsAsync(request.id);

                if (!exists)
                {
                    _logger.LogWarning("Attempted to delete non-existent feedback with ID: {id}", request.id);
                    return Result<bool>.Failure($"Feedback with ID: {request.id} not found.");
                }
                await _feedbackRepository.DeleteAsync(request.id);

                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback with ID: {request.id}");
                return Result<bool>.Failure("An error occurred while deleting feedback");
            }
        }
    }
}
