using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class UpdateFootballMatchHandler : IRequestHandler<UpdateFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFootballMatchHandler> _logger;

    public UpdateFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<UpdateFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(UpdateFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? existingMatch = await _matchRepository.GetByIdAsync(request.Id);
            if (existingMatch == null)
            {
                _logger.LogWarning("Attempt to update non-existent football match with ID: {MatchId}", request.Id);
                return Result<FootballMatchDto>.NotFound("FootballMatch", request.Id);
            }

            FootballMatchMapper.UpdateFromCommand(existingMatch, request);

            _logger.LogInformation("Updating football match: {MatchId}", existingMatch.Id);
            await _matchRepository.UpdateAsync(existingMatch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(existingMatch);
            _logger.LogInformation("Successfully updated football match with ID: {MatchId}", existingMatch.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure("An error occurred while updating the football match.");
        }
    }
}
