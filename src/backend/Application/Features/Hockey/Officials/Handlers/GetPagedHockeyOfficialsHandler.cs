using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Mappings;
using Application.Features.Hockey.Officials.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Officials.Handlers;

/// <summary>
/// Handles paginated hockey official listing.
/// </summary>
public class GetPagedHockeyOfficialsHandler
    : IRequestHandler<GetPagedHockeyOfficialsQuery, Result<PagedResult<HockeyOfficialDto>>>
{
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IPaginationService _paginationService;
    private readonly ILogger<GetPagedHockeyOfficialsHandler> _logger;

    public GetPagedHockeyOfficialsHandler(
        IHockeyOfficialRepository officialRepository,
        IPaginationService paginationService,
        ILogger<GetPagedHockeyOfficialsHandler> logger)
    {
        _officialRepository = officialRepository;
        _paginationService = paginationService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyOfficialDto>>> Handle(
        GetPagedHockeyOfficialsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int pageSize = _paginationService.ResolvePageSize(
                GetPagedHockeyOfficialsQuery.ResourceKey,
                request.PageSize);

            PagedResult<HockeyOfficial> pagedOfficials = await _officialRepository.GetPagedAsync(
                request.Page,
                pageSize,
                request.IsActive,
                request.SearchTerm,
                request.LicenseExpiringWithinDays,
                cancellationToken);

            IReadOnlyList<HockeyOfficialDto> items =
                pagedOfficials.Items.Select(HockeyOfficialMapper.ToDto).ToList();
            return Result<PagedResult<HockeyOfficialDto>>.Success(
                PagedResult.Create(items, pagedOfficials.TotalCount, pagedOfficials.Page, pagedOfficials.PageSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Paged hockey official retrieval was cancelled");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid paged hockey official query");
            return Result<PagedResult<HockeyOfficialDto>>.Failure(
                "An error occurred while listing hockey officials.",
                ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get paged hockey officials");
            return Result<PagedResult<HockeyOfficialDto>>.Failure(
                "An error occurred while listing hockey officials.",
                ex.Flatten());
        }
    }
}
