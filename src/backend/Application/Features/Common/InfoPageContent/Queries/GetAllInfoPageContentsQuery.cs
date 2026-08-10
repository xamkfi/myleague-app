// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.InfoPageContent.Mappings;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.InfoPageContent.Queries;

/// <summary>
/// Query for retrieving all info page contents
/// </summary>
public record GetAllInfoPageContentsQuery() : IRequest<Result<IReadOnlyList<InfoPageContentDto>>>;

/// <summary>
/// Handler for retrieving all info page contents
/// </summary>
public class GetAllInfoPageContentsQueryHandler
    : IRequestHandler<GetAllInfoPageContentsQuery, Result<IReadOnlyList<InfoPageContentDto>>>
{
    private readonly IInfoPageContentRepository _repository;

    /// <summary>
    /// Initializes a new instance of the GetAllInfoPageContentsQueryHandler class
    /// </summary>
    /// <param name="repository">The info page content repository</param>
    public GetAllInfoPageContentsQueryHandler(IInfoPageContentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the GetAllInfoPageContentsQuery request
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All info page contents wrapped in a Result</returns>
    public async Task<Result<IReadOnlyList<InfoPageContentDto>>> Handle(
        GetAllInfoPageContentsQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Common.InfoPageContent> items =
            await _repository.GetAllAsync(cancellationToken);

        IReadOnlyList<InfoPageContentDto> dtos = items
            .Select(InfoPageContentMapper.ToDto)
            .ToList();

        return Result<IReadOnlyList<InfoPageContentDto>>.Success(dtos);
    }
}
