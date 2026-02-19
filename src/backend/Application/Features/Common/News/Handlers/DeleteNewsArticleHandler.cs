// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.News.Commands;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.News.Queries;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.News.Handlers
{
    /// <summary>
    /// Handler for deleting news by id
    /// </summary>
    public class DeleteNewsArticleHandler : IRequestHandler<DeleteNewsArticleCommand, Result<bool>>
    {
        private readonly INewsArticleRepository _newsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteNewsArticleHandler> _logger;

        public DeleteNewsArticleHandler(INewsArticleRepository newsArticleRepository, IUnitOfWork unitOfWork, ILogger<DeleteNewsArticleHandler> logger)
        {
            _newsRepository = newsArticleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteNewsArticleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                bool exists = await _newsRepository.ExistsAsync(request.id);

                if (!exists)
                {
                    _logger.LogWarning("Attempt to delete non-existent news article with ID: {NewsId}", request.id);
                    return Result<bool>.Failure($"News article with ID '{request.id}' not found.");
                }

                await _newsRepository.DeleteNews(request.id);

                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting news article with ID: {NewsId}", request.id);
                return Result<bool>.Failure("An error occurred while deleting the news article.");
            }
        }
    }
}
