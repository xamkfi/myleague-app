// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.NewsArticles;

namespace Application.Handlers.NewsArticles
{
    public class GetMainNewsHandler : IRequestHandler<GetMainNewsQuery, Result<NewsArticleDto>>
    {
        private readonly INewsArticleRepository _newsRepository;

        public GetMainNewsHandler(INewsArticleRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<Result<NewsArticleDto>> Handle(GetMainNewsQuery request, CancellationToken cancellationToken)
        {
            NewsArticle? mainNews = await _newsRepository.GetMainNews();
            if (mainNews == null)
            {
                return Result<NewsArticleDto>.NotFound("not found", mainNews);
            }
            NewsArticleDto dto = NewsArticleMapper.ToDto(mainNews);
            return Result<NewsArticleDto>.Success(dto);
        }
    }
}
