// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Mappings;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.InfoPageContent.Queries;

/// <summary>
/// Query for retrieving info page content by slug
/// </summary>
public record GetInfoPageContentBySlugQuery(string Slug) : IRequest<Result<InfoPageContentDto>>;

/// <summary>
/// Handler for retrieving info page content by slug
/// </summary>
public class GetInfoPageContentBySlugQueryHandler
    : IRequestHandler<GetInfoPageContentBySlugQuery, Result<InfoPageContentDto>>
{
    private readonly IInfoPageContentRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetInfoPageContentBySlugQueryHandler class
    /// </summary>
    /// <param name="repository">The info page content repository</param>
    public GetInfoPageContentBySlugQueryHandler(IInfoPageContentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the GetInfoPageContentBySlugQuery request
    /// </summary>
    /// <param name="request">The query containing the page slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The info page content as a DTO wrapped in a Result</returns>
    public async Task<Result<InfoPageContentDto>> Handle(
        GetInfoPageContentBySlugQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.InfoPageContent? entity =
            await _repository.GetBySlugAsync(request.Slug, cancellationToken);

        if (entity == null)
        {
            return Result<InfoPageContentDto>.NotFound("InfoPageContent", request.Slug);
        }

        return Result<InfoPageContentDto>.Success(InfoPageContentMapper.ToDto(entity));
    }
}
