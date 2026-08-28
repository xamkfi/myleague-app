using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Competitions;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Competitions.Handlers;

/// <summary>
/// Handles creating a competition division link via Domain <c>AddDivision</c>.
/// </summary>
public class CreateHockeyCompetitionDivisionHandler
    : IRequestHandler<CreateHockeyCompetitionDivisionCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyCompetitionDivisionHandler> _logger;

    public CreateHockeyCompetitionDivisionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IDivisionRepository divisionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyCompetitionDivisionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _divisionRepository = divisionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        CreateHockeyCompetitionDivisionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            Division? division = await _divisionRepository.GetByIdAsync(request.DivisionId);
            if (division is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("Division", request.DivisionId);
            }

            if (division.SportType != SportsCategory.Icehockey)
            {
                return Result<HockeyCompetitionDto>.Failure("Division must be an ice hockey division.");
            }

            competition.AddDivision(request.DivisionId, request.Name, request.SortOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added division {DivisionId} to hockey competition {CompetitionId}",
                request.DivisionId,
                request.CompetitionId);

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected CreateDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CreateDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed CreateDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while adding the division to the competition.",
                ex.Flatten());
        }
    }
}
