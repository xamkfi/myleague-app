using Application.Common;
using Application.Features.Common.SiteSettings.DTOs;
using MediatR;

namespace Application.Features.Common.SiteSettings.Commands;

/// <summary>
/// Command for updating footer contact settings.
/// </summary>
public record UpdateFooterContactCommand(
    string OrganizationName,
    string OrganizationAddress,
    IReadOnlyList<FooterContactPersonUpdateDto> ContactPersons,
    string? ModifiedBy
) : IRequest<Result<FooterContactDto>>;
