using Application.Features.Floorball.Tournaments.DTOs;
using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

public record CreateFloorballTournamentCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    string? DescriptionHtml,
    int NumberOfPeriods = 2,
    int PeriodDurationMinutes = 15,
    bool AllowOvertime = true,
    int OvertimeDurationMinutes = 5,
    bool AllowShootout = true,
    string PlayoffFormat = "None",
    int GroupStageAdvancingCount = 1) : IRequest<Result<FloorballTournamentDto>>;
