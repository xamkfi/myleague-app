using Application.Common;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Mappings;
using Application.Interfaces.Common;
using Domain.Repositories.Common;
using MediatR;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;

namespace Application.Features.Common.SiteSettings.Commands;

public record UpdateSiteSettingsCommand(
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays,
    int LoginCodeExpirationMinutes,
    int LoginCodeMaxAttempts,
    int SessionExpiryWarningMinutes
) : IRequest<Result<SiteSettingsDto>>;

public class UpdateSiteSettingsCommandHandler
    : IRequestHandler<UpdateSiteSettingsCommand, Result<SiteSettingsDto>>
{
    private readonly ISiteSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISiteSettingsProvider _siteSettingsProvider;

    public UpdateSiteSettingsCommandHandler(
        ISiteSettingsRepository repository,
        IUnitOfWork unitOfWork,
        ISiteSettingsProvider siteSettingsProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _siteSettingsProvider = siteSettingsProvider;
    }

    public async Task<Result<SiteSettingsDto>> Handle(
        UpdateSiteSettingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            SiteSettingsEntity? entity = await _repository.GetAsync(cancellationToken);

            if (entity is null)
            {
                entity = SiteSettingsMapper.ToEntity(request);
                await _repository.AddAsync(entity, cancellationToken);
            }
            else
            {
                SiteSettingsMapper.UpdateFromCommand(entity, request);
                await _repository.UpdateAsync(entity, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _siteSettingsProvider.Invalidate();

            return Result<SiteSettingsDto>.Success(SiteSettingsMapper.ToDto(entity, isPersisted: true));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return Result<SiteSettingsDto>.Failure(ex.Message);
        }
    }
}
