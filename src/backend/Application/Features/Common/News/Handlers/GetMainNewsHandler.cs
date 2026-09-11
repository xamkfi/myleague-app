// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Application.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Common.News.Queries;

namespace Application.Features.Common.News.Handlers
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
                return Result<NewsArticleDto>.NotFound("not found", nameof(mainNews));
            }
            NewsArticleDto dto = NewsArticleMapper.ToDto(mainNews);
            return Result<NewsArticleDto>.Success(dto);
        }
    }
}
