using Application.Common;
using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Mappings;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Features.Common.SiteSettings.Handlers;

/// <summary>
/// Handles footer contact settings update requests.
/// </summary>
public class UpdateFooterContactHandler : IRequestHandler<UpdateFooterContactCommand, Result<FooterContactDto>>
{
    private const string FooterContactKey = "footer-contact";

    private readonly ISiteSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFooterContactHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFooterContactHandler"/> class.
    /// </summary>
    public UpdateFooterContactHandler(
        ISiteSettingRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFooterContactHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command and updates footer contact settings.
    /// </summary>
    public async Task<Result<FooterContactDto>> Handle(UpdateFooterContactCommand request, CancellationToken cancellationToken)
    {
        try
        {
            SiteSetting? existing = await _repository.GetByKeyAsync(FooterContactKey, cancellationToken);

            FooterContactSettingValue value = new(
                request.OrganizationName,
                request.OrganizationAddress,
                request.ContactPersons
                    .Select(p => new FooterContactPersonDto(p.NameOrRole, p.Email, p.Phone))
                    .ToList());

            string valueJson = JsonSerializer.Serialize(value);

            if (existing == null)
            {
                existing = new SiteSetting(
                    Guid.NewGuid(),
                    FooterContactKey,
                    valueJson,
                    request.ModifiedBy);
            }
            else
            {
                existing.UpdateValue(valueJson, request.ModifiedBy);
            }

            await _repository.SaveAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FooterContactDto>.Success(FooterContactMapper.ToDto(
                value,
                existing.LastModifiedBy,
                existing.UpdatedAt ?? existing.CreatedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating footer contact settings");
            return Result<FooterContactDto>.Failure("An error occurred while updating footer contact settings.");
        }
    }
}
