// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Common;
using Application.Interfaces.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.PageContent.Commands
{
    public record DeletePageRuleCommand(
        string Slug,
        string RuleId
    ) : IRequest<Result<PageContentDto>>;

    public class DeletePageRuleCommandHandler : IRequestHandler<DeletePageRuleCommand, Result<PageContentDto>>
    {
        private readonly ICommonDbContext _context;

        public DeletePageRuleCommandHandler(ICommonDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PageContentDto>> Handle(DeletePageRuleCommand request, CancellationToken cancellationToken)
        {
            Domain.Entities.Common.PageContent? entity = await _context.PageContents
                .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

            if (entity == null)
            {
                return Result<PageContentDto>.Failure($"Page content with slug '{request.Slug}' not found.");
            }

            string pattern = $@"<div\s+class=""rules-item""\s+data-rule-id=""{Regex.Escape(request.RuleId)}""\s*[^>]*>(.*?)</div>\s*";

            if (!Regex.IsMatch(entity.ContentHtml, pattern, RegexOptions.Singleline))
            {
                return Result<PageContentDto>.Failure($"Rule with ID '{request.RuleId}' not found.");
            }

            entity.ContentHtml = Regex.Replace(
                entity.ContentHtml,
                pattern,
                string.Empty,
                RegexOptions.Singleline).Trim();

            await _context.SaveChangesAsync(cancellationToken);

            PageContentDto dto = new()
            {
                Id = entity.Id,
                PageSlug = entity.PageSlug,
                Title = entity.Title,
                ContentHtml = entity.ContentHtml,
                LastModifiedBy = entity.LastModifiedBy,
                UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
            };

            return Result<PageContentDto>.Success(dto);
        }
    }
}
