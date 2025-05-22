using Application.Commands.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.Clubs;

/// <summary>
/// Use case for creating a new club
/// </summary>
public class CreateClubUseCase
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<CreateClubUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateClubUseCase class
    /// </summary>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public CreateClubUseCase(IClubRepository clubRepository, ILogger<CreateClubUseCase> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the use case to create a new club
    /// </summary>
    /// <param name="command">The command containing club information</param>
    /// <returns>The newly created club as a DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when a club with the same name already exists</exception>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    public async Task<ClubDto> ExecuteAsync(CreateClubCommand command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (await _clubRepository.ExistsByNameAsync(command.Name))
        {
            _logger.LogError("A club with the name {ClubName} already exists", command.Name);
            throw new InvalidOperationException($"A club with the name '{command.Name}' already exists.");
        }

        Club club = ClubMapper.ToEntity(command);

        _logger.LogInformation("Creating new club: {ClubName}", club.Name);
        await _clubRepository.AddAsync(club);

        return ClubMapper.ToDto(club);
    }
}
