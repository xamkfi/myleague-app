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
    public record UpdatePageRuleCommand(
        string Slug,
        string RuleId,
        string RuleHtml
    ) : IRequest<Result<PageContentDto>>;

    public class UpdatePageRuleCommandHandler : IRequestHandler<UpdatePageRuleCommand, Result<PageContentDto>>
    {
        private readonly ICommonDbContext _context;

        public UpdatePageRuleCommandHandler(ICommonDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PageContentDto>> Handle(UpdatePageRuleCommand request, CancellationToken cancellationToken)
        {
            Domain.Entities.Common.PageContent? entity = await _context.PageContents
                .FirstOrDefaultAsync(x => x.PageSlug == request.Slug, cancellationToken);

            if (entity == null)
            {
                return Result<PageContentDto>.Failure($"Page content with slug '{request.Slug}' not found.");
            }

            string strippedRuleText = Regex.Replace(request.RuleHtml ?? string.Empty, "<[^>]*>", string.Empty)
                .Replace("&nbsp;", " ")
                .Trim();

            if (string.IsNullOrWhiteSpace(strippedRuleText))
            {
                return Result<PageContentDto>.Failure("Rule content cannot be empty.");
            }

            string pattern = $@"<div\s+class=""rules-item""\s+data-rule-id=""{Regex.Escape(request.RuleId)}"">(.*?)</div>";

            if (!Regex.IsMatch(entity.ContentHtml, pattern, RegexOptions.Singleline))
            {
                return Result<PageContentDto>.Failure($"Rule with ID '{request.RuleId}' not found.");
            }

            string replacement = $@"<div class=""rules-item"" data-rule-id=""{request.RuleId}"">{request.RuleHtml}</div>";

            entity.ContentHtml = Regex.Replace(
                entity.ContentHtml,
                pattern,
                replacement,
                RegexOptions.Singleline);

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
