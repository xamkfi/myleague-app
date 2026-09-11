using Application.Features.Common.SiteSettings.Commands;
using FluentValidation;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;

namespace Application.Features.Common.SiteSettings.Validators;

public class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(x => x.AccessTokenExpirationMinutes)
            .InclusiveBetween(SiteSettingsEntity.AccessTokenExpirationMinutesMin, SiteSettingsEntity.AccessTokenExpirationMinutesMax);

        RuleFor(x => x.RefreshTokenExpirationDays)
            .InclusiveBetween(SiteSettingsEntity.RefreshTokenExpirationDaysMin, SiteSettingsEntity.RefreshTokenExpirationDaysMax);

        RuleFor(x => x.LoginCodeExpirationMinutes)
            .InclusiveBetween(SiteSettingsEntity.LoginCodeExpirationMinutesMin, SiteSettingsEntity.LoginCodeExpirationMinutesMax);

        RuleFor(x => x.LoginCodeMaxAttempts)
            .InclusiveBetween(SiteSettingsEntity.LoginCodeMaxAttemptsMin, SiteSettingsEntity.LoginCodeMaxAttemptsMax);

        RuleFor(x => x.SessionExpiryWarningMinutes)
            .InclusiveBetween(SiteSettingsEntity.SessionExpiryWarningMinutesMin, SiteSettingsEntity.SessionExpiryWarningMinutesMax);
    }
}
