using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles assigning an official to a hockey match.
/// </summary>
public class AddHockeyMatchOfficialHandler : IRequestHandler<AddHockeyMatchOfficialCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyMatchOfficialHandler> _logger;

    public AddHockeyMatchOfficialHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyOfficialRepository officialRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyMatchOfficialHandler> logger)
    {
        _matchRepository = matchRepository;
        _officialRepository = officialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(
        AddHockeyMatchOfficialCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyOfficial? official = await _officialRepository.GetByIdAsync(request.OfficialId);
            if (official is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyOfficial", request.OfficialId);
            }

            if (!official.IsActive)
            {
                return Result<HockeyMatchDto>.Failure("Cannot assign an inactive hockey official to a match.");
            }

            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            match.AddOfficial(request.OfficialId, request.Role, request.IsMainOfficial);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Assigned official {OfficialId} to match {MatchId}",
                request.OfficialId,
                request.MatchId);

            return Result<HockeyMatchDto>.Success(
                Application.Features.Hockey.Matches.Mappings.HockeyMatchMapper.ToDto(match));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddOfficial for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddOfficial for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddOfficial for match {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure(
                "An error occurred while assigning the official.",
                ex.Flatten());
        }
    }
}
