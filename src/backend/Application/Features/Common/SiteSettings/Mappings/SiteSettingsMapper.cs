using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;

namespace Application.Features.Common.SiteSettings.Mappings;

internal static class SiteSettingsMapper
{
    public static SiteSettingsDto ToDto(SiteSettingsEntity entity, bool isPersisted)
    {
        return new SiteSettingsDto(
            entity.AccessTokenExpirationMinutes,
            entity.RefreshTokenExpirationDays,
            entity.LoginCodeExpirationMinutes,
            entity.LoginCodeMaxAttempts,
            entity.SessionExpiryWarningMinutes,
            isPersisted);
    }

    public static SiteSettingsDto ToDto(EffectiveAuthSettings settings)
    {
        return new SiteSettingsDto(
            settings.AccessTokenExpirationMinutes,
            settings.RefreshTokenExpirationDays,
            settings.LoginCodeExpirationMinutes,
            settings.LoginCodeMaxAttempts,
            settings.SessionExpiryWarningMinutes,
            settings.IsPersisted);
    }

    public static SiteSettingsEntity ToEntity(UpdateSiteSettingsCommand command)
    {
        return new SiteSettingsEntity(
            Guid.NewGuid(),
            command.AccessTokenExpirationMinutes,
            command.RefreshTokenExpirationDays,
            command.LoginCodeExpirationMinutes,
            command.LoginCodeMaxAttempts,
            command.SessionExpiryWarningMinutes);
    }

    public static void UpdateFromCommand(SiteSettingsEntity entity, UpdateSiteSettingsCommand command)
    {
        entity.Update(
            command.AccessTokenExpirationMinutes,
            command.RefreshTokenExpirationDays,
            command.LoginCodeExpirationMinutes,
            command.LoginCodeMaxAttempts,
            command.SessionExpiryWarningMinutes);
    }
}
