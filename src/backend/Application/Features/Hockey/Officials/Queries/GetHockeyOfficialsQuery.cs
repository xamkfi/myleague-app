using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using MediatR;

namespace Application.Features.Hockey.Officials.Queries;

/// <summary>
/// Lists hockey officials, optionally filtered by active status.
/// </summary>
public record GetHockeyOfficialsQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<HockeyOfficialDto>>>;
