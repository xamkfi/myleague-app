using Application.Common;
using Application.Features.Hockey.Players.Commands;
using Application.Features.Hockey.Players.DTOs;
using Application.Features.Hockey.Players.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Players.Handlers;

/// <summary>
/// Handles creation of a hockey player profile.
/// </summary>
public class CreateHockeyPlayerHandler : IRequestHandler<CreateHockeyPlayerCommand, Result<HockeyPlayerDto>>
{
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyPlayerHandler> _logger;

    public CreateHockeyPlayerHandler(
        IHockeyPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyPlayerHandler> logger)
    {
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyPlayerDto>> Handle(CreateHockeyPlayerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person is null)
            {
                return Result<HockeyPlayerDto>.NotFound("Person", request.PersonId);
            }

            HockeyPlayer? existing = await _playerRepository.GetByPersonIdAsync(request.PersonId);
            if (existing is not null)
            {
                _logger.LogInformation(
                    "Hockey player already exists for person {PersonId} ({PlayerId}), returning existing",
                    request.PersonId,
                    existing.Id);
                return Result<HockeyPlayerDto>.Success(HockeyPlayerMapper.ToDto(existing));
            }

            HockeyPlayer player = new(
                request.PersonId,
                request.PrimaryPosition,
                request.Shoots,
                request.Catches,
                request.LicenseNumber);

            await _playerRepository.AddAsync(player);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created hockey player {PlayerId} for person {PersonId}", player.Id, request.PersonId);
            return Result<HockeyPlayerDto>.Success(HockeyPlayerMapper.ToDto(player));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CreateHockeyPlayer for person {PersonId}", request.PersonId);
            return Result<HockeyPlayerDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed CreateHockeyPlayer for person {PersonId}", request.PersonId);
            return Result<HockeyPlayerDto>.Failure("An error occurred while creating the hockey player.", ex.Flatten());
        }
    }
}
