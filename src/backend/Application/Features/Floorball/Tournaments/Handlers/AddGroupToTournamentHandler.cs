using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class AddGroupToTournamentHandler : IRequestHandler<AddGroupToTournamentCommand, Result<FloorballTournamentGroupDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTournamentGroupRepository _groupRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddGroupToTournamentHandler> _logger;

    public AddGroupToTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTournamentGroupRepository groupRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddGroupToTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentGroupDto>> Handle(AddGroupToTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool tournamentExists = await _tournamentRepository.ExistsAsync(request.TournamentId);
            if (!tournamentExists)
            {
                _logger.LogWarning("Attempt to add group to non-existent tournament {TournamentId}", request.TournamentId);
                return Result<FloorballTournamentGroupDto>.NotFound("FloorballTournament", request.TournamentId);
            }

            if (!Enum.TryParse<FloorballTournamentGroupPhase>(request.Phase, true, out FloorballTournamentGroupPhase phase))
                return Result<FloorballTournamentGroupDto>.Failure($"Invalid phase: '{request.Phase}'. Valid values: GroupStage, Playoff.");

            FloorballTournamentGroup group = new(request.TournamentId, request.Name, phase, request.SortOrder);

            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added group '{GroupName}' ({GroupId}) to tournament {TournamentId}", request.Name, group.Id, request.TournamentId);

            FloorballTournamentGroupDto dto = FloorballTournamentMapper.ToGroupDto(group);
            return Result<FloorballTournamentGroupDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding group to tournament {TournamentId}", request.TournamentId);
            return Result<FloorballTournamentGroupDto>.Failure("An error occurred while adding the group to the tournament.");
        }
    }
}
