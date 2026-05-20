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
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Feedback.Handlers
{
    public class GetAllFeedbackHandler :BasePagedQueryHandler<GetAllFeedbackQuery,FeedbackListDto>,
        IRequestHandler<GetAllFeedbackQuery,Result<PagedResult<FeedbackListDto>>>
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public GetAllFeedbackHandler(IFeedbackRepository feedbackRepository,
            IPaginationService paginationService,
            ILogger<GetAllFeedbackQuery> logger) : base (paginationService, logger)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<Result<PagedResult<FeedbackListDto>>> Handle(GetAllFeedbackQuery request, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Retrieving feedback. Page: {page}, PageSize: {pageSize}", request.page, request.pageSize);

                IEnumerable<FeedbackEntity> feedbacks = await _feedbackRepository.GetAllAsync(request.page, request.pageSize, cancellationToken);

                int totalCount = await _feedbackRepository.GetTotalCountAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                IEnumerable<FeedbackListDto> feedbackList = FeedbackMapper.ToListDtos(feedbacks);
                PagedResult<FeedbackListDto> pagedResult = CreatePagedResult(feedbackList, totalCount, request.page, request.pageSize);

                _logger.LogInformation("Successfully retrieved {Count} feedbacks out of {totalCount}", feedbacks.Count(), totalCount);

                return Result<PagedResult<FeedbackListDto>>.Success(pagedResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Feedback retrieval was cancelled. Page: {page}, PageSize: {pageSize}", request.page, request.pageSize);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving feedback");
                return Result<PagedResult<FeedbackListDto>>.Failure("An error occurred while retrieving feedback");
            }
        }
    }
}
