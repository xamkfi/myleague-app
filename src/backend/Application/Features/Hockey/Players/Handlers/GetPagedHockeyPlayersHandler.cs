using Application.Common;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Players.Mappings;
using Application.Features.Hockey.Players.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Players.Handlers;

/// <summary>
/// Handles paginated hockey player listing.
/// </summary>
public class GetPagedHockeyPlayersHandler
    : IRequestHandler<GetPagedHockeyPlayersQuery, Result<PagedResult<HockeyPlayerDto>>>
{
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IPaginationService _paginationService;
    private readonly ILogger<GetPagedHockeyPlayersHandler> _logger;

    public GetPagedHockeyPlayersHandler(
        IHockeyPlayerRepository playerRepository,
        IPaginationService paginationService,
        ILogger<GetPagedHockeyPlayersHandler> logger)
    {
        _playerRepository = playerRepository;
        _paginationService = paginationService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HockeyPlayerDto>>> Handle(
        GetPagedHockeyPlayersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            int pageSize = _paginationService.ResolvePageSize(
                GetPagedHockeyPlayersQuery.ResourceKey,
                request.PageSize);

            PagedResult<HockeyPlayer> pagedPlayers = await _playerRepository.GetPagedAsync(
                request.Page,
                pageSize,
                request.SearchTerm,
                request.IsActive,
                request.Position,
                request.ClubId,
                request.TeamId,
                request.TeamCategory,
                cancellationToken);

            IReadOnlyList<HockeyPlayerDto> items = pagedPlayers.Items.Select(HockeyPlayerMapper.ToDto).ToList();
            return Result<PagedResult<HockeyPlayerDto>>.Success(
                PagedResult.Create(items, pagedPlayers.TotalCount, pagedPlayers.Page, pagedPlayers.PageSize));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Paged hockey player retrieval was cancelled");
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid paged hockey player query");
            return Result<PagedResult<HockeyPlayerDto>>.Failure(
                "An error occurred while retrieving hockey players.",
                ex.Flatten());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to get paged hockey players");
            return Result<PagedResult<HockeyPlayerDto>>.Failure(
                "An error occurred while retrieving hockey players.",
                ex.Flatten());
        }
    }
}
