using Application.Common;
using Application.Features.Common.SiteSettings.DTOs;
using MediatR;

namespace Application.Features.Common.SiteSettings.Queries;

/// <summary>
/// Query for fetching footer contact settings.
/// </summary>
public record GetFooterContactQuery() : IRequest<Result<FooterContactDto>>;
