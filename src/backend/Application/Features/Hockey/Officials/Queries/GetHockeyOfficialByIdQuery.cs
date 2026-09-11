using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using MediatR;

namespace Application.Features.Hockey.Officials.Queries;

/// <summary>
/// Gets a hockey official by id.
/// </summary>
public record GetHockeyOfficialByIdQuery(Guid OfficialId) : IRequest<Result<HockeyOfficialDto>>;
