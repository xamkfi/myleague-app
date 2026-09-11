using Application.Common;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Players.Mappings;
using Application.Features.Hockey.Players.Queries;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Players.Handlers;

/// <summary>
/// Handles retrieving a hockey player by id.
/// </summary>
public class GetHockeyPlayerByIdHandler : IRequestHandler<GetHockeyPlayerByIdQuery, Result<HockeyPlayerDto>>
{
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly ILogger<GetHockeyPlayerByIdHandler> _logger;

    public GetHockeyPlayerByIdHandler(
        IHockeyPlayerRepository playerRepository,
        ILogger<GetHockeyPlayerByIdHandler> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyPlayerDto>> Handle(GetHockeyPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyPlayer? player = await _playerRepository.GetByIdAsync(request.Id);
            if (player is null)
            {
                return Result<HockeyPlayerDto>.NotFound("HockeyPlayer", request.Id);
            }

            return Result<HockeyPlayerDto>.Success(HockeyPlayerMapper.ToDto(player));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey player {PlayerId}", request.Id);
            return Result<HockeyPlayerDto>.Failure("An error occurred while retrieving the hockey player.", ex.Flatten());
        }
    }
}
