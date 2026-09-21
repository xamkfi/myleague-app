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

        RuleFor(x => x.PlayerLicenceResetMonth)
            .InclusiveBetween(SiteSettingsEntity.PlayerLicenceResetMonthMin, SiteSettingsEntity.PlayerLicenceResetMonthMax);

        RuleFor(x => x)
            .Must(command => SiteSettingsEntity.IsValidLicenceResetDate(
                command.PlayerLicenceResetMonth,
                command.PlayerLicenceResetDay))
            .WithName(nameof(UpdateSiteSettingsCommand.PlayerLicenceResetDay))
            .WithMessage("Player licence reset day must be a real day for the selected month (29 February is allowed).");
    }
}
