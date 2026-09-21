import { useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import { siteSettingsService } from '../../../api/common/siteSettingsService';
import type { SiteSettings } from '../../../types/admin/siteSettingsTypes';
import './SettingsPage.scss';

const FIELD_BOUNDS = {
  accessTokenExpirationMinutes: { min: 2, max: 180 },
  refreshTokenExpirationDays: { min: 1, max: 90 },
  loginCodeExpirationMinutes: { min: 2, max: 60 },
  loginCodeMaxAttempts: { min: 3, max: 20 },
  sessionExpiryWarningMinutes: { min: 1, max: 30 },
} as const;

type SettingsField = keyof typeof FIELD_BOUNDS;

function daysInMonth(month: number): number {
  if (month === 2) {
    return 29;
  }
  return new Date(2024, month, 0).getDate();
}

function SettingsPage() {
  const { t } = useTranslation();
  const [form, setForm] = useState<SiteSettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const load = async (): Promise<void> => {
      try {
        setIsLoading(true);
        setErrorMessage(null);
        const settings = await siteSettingsService.get();
        if (isMounted) {
          setForm(settings);
        }
      } catch (error) {
        if (!isMounted) {
          return;
        }
        setErrorMessage(
          error instanceof Error
            ? error.message
            : t('admin.settings.loadFailed', 'Failed to load settings.'),
        );
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    void load();
    return () => {
      isMounted = false;
    };
  }, [t]);

  useEffect(() => {
    if (!successMessage) {
      return;
    }
    const timeout = setTimeout(() => setSuccessMessage(null), 5000);
    return () => clearTimeout(timeout);
  }, [successMessage]);

  const handleNumberChange = (field: SettingsField, value: string): void => {
    if (!form) {
      return;
    }
    const parsed = Number.parseInt(value, 10);
    setForm({
      ...form,
      [field]: Number.isNaN(parsed) ? 0 : parsed,
    });
  };

  const handleLicenceDateChange = (field: 'playerLicenceResetMonth' | 'playerLicenceResetDay', value: string): void => {
    if (!form) {
      return;
    }
    const parsed = Number.parseInt(value, 10);
    const next = Number.isNaN(parsed) ? 0 : parsed;
    if (field === 'playerLicenceResetMonth') {
      const maxDay = daysInMonth(next);
      setForm({
        ...form,
        playerLicenceResetMonth: next,
        playerLicenceResetDay: Math.min(form.playerLicenceResetDay, maxDay),
      });
      return;
    }
    setForm({
      ...form,
      playerLicenceResetDay: next,
    });
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault();
    if (!form) {
      return;
    }

    try {
      setIsSaving(true);
      setErrorMessage(null);
      const saved = await siteSettingsService.update({
        accessTokenExpirationMinutes: form.accessTokenExpirationMinutes,
        refreshTokenExpirationDays: form.refreshTokenExpirationDays,
        loginCodeExpirationMinutes: form.loginCodeExpirationMinutes,
        loginCodeMaxAttempts: form.loginCodeMaxAttempts,
        sessionExpiryWarningMinutes: form.sessionExpiryWarningMinutes,
        playerLicenceResetMonth: form.playerLicenceResetMonth,
        playerLicenceResetDay: form.playerLicenceResetDay,
      });
      setForm(saved);
      setSuccessMessage(t('admin.settings.saveSuccess', 'Settings saved.'));
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.settings.saveFailed', 'Failed to save settings.'),
      );
    } finally {
      setIsSaving(false);
    }
  };

  const handleResetLicences = async (): Promise<void> => {
    const confirmed = window.confirm(
      t(
        'admin.settings.licenceResetConfirm',
        'Deactivate unpaid team licences now? Players stay inactive until you mark each team licence paid again.',
      ),
    );
    if (!confirmed) {
      return;
    }

    try {
      setIsResetting(true);
      setErrorMessage(null);
      const result = await siteSettingsService.resetPlayerLicences();
      const refreshed = await siteSettingsService.get();
      setForm(refreshed);
      setSuccessMessage(
        t('admin.settings.licenceResetSuccess', {
          floorball: result.floorballDeactivated,
          football: result.footballDeactivated,
          hockey: result.hockeyDeactivated,
          defaultValue:
            'Licences reset: {{floorball}} floorball, {{football}} football, {{hockey}} hockey.',
        }),
      );
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.settings.licenceResetFailed', 'Failed to reset player licences.'),
      );
    } finally {
      setIsResetting(false);
    }
  };

  const fields: Array<{ key: SettingsField; labelKey: string; hintKey: string }> = [
    {
      key: 'accessTokenExpirationMinutes',
      labelKey: 'admin.settings.accessTokenMinutes',
      hintKey: 'admin.settings.accessTokenMinutesHint',
    },
    {
      key: 'refreshTokenExpirationDays',
      labelKey: 'admin.settings.refreshTokenDays',
      hintKey: 'admin.settings.refreshTokenDaysHint',
    },
    {
      key: 'loginCodeExpirationMinutes',
      labelKey: 'admin.settings.loginCodeMinutes',
      hintKey: 'admin.settings.loginCodeMinutesHint',
    },
    {
      key: 'loginCodeMaxAttempts',
      labelKey: 'admin.settings.loginCodeMaxAttempts',
      hintKey: 'admin.settings.loginCodeMaxAttemptsHint',
    },
    {
      key: 'sessionExpiryWarningMinutes',
      labelKey: 'admin.settings.sessionWarningMinutes',
      hintKey: 'admin.settings.sessionWarningMinutesHint',
    },
  ];

  const licenceDayMax = form ? daysInMonth(form.playerLicenceResetMonth) : 31;

  return (
    <PageTemplate title={t('admin.settings.pageTitle', 'Site settings')}>
      <div className="settings-page">
        <p className="settings-page__description">
          {t(
            'admin.settings.description',
            'Token and login-code lifetimes apply to newly issued sessions only. Secrets stay in server configuration.',
          )}
        </p>

        {successMessage && (
          <p className="settings-page__alert settings-page__alert--success">{successMessage}</p>
        )}
        {errorMessage && (
          <p className="settings-page__alert settings-page__alert--error">{errorMessage}</p>
        )}

        {isLoading || !form ? (
          <p>{t('common.loading', 'Loading...')}</p>
        ) : (
          <form className="settings-page__form" onSubmit={(event) => { void handleSubmit(event); }}>
            {form.isPersisted ? (
              <p className="settings-page__source">
                {t('admin.settings.sourcePersisted', 'These values are saved for this site.')}
              </p>
            ) : (
              <p className="settings-page__source">
                {t(
                  'admin.settings.sourceDefaults',
                  'Showing server defaults. Save to store site-owned values.',
                )}
              </p>
            )}

            {fields.map((field) => {
              const bounds = FIELD_BOUNDS[field.key];
              return (
                <label key={field.key} className="settings-page__field">
                  <span>{t(field.labelKey)}</span>
                  <input
                    type="number"
                    min={bounds.min}
                    max={bounds.max}
                    value={form[field.key]}
                    onChange={(event) => handleNumberChange(field.key, event.target.value)}
                    required
                  />
                  <span className="settings-page__hint">
                    {t(field.hintKey, { min: bounds.min, max: bounds.max })}
                  </span>
                </label>
              );
            })}

            <fieldset className="settings-page__licence">
              <legend>{t('admin.settings.licenceTitle', 'Player licences')}</legend>
              <p className="settings-page__hint">
                {t(
                  'admin.settings.licenceHint',
                  'On this date each year, unpaid team licences become inactive. Default is 1 May.',
                )}
              </p>
              <div className="settings-page__licence-row">
                <label className="settings-page__field">
                  <span>{t('admin.settings.licenceDay', 'Day')}</span>
                  <input
                    type="number"
                    min={1}
                    max={licenceDayMax}
                    value={form.playerLicenceResetDay}
                    onChange={(event) => handleLicenceDateChange('playerLicenceResetDay', event.target.value)}
                    required
                  />
                </label>
                <label className="settings-page__field">
                  <span>{t('admin.settings.licenceMonth', 'Month')}</span>
                  <input
                    type="number"
                    min={1}
                    max={12}
                    value={form.playerLicenceResetMonth}
                    onChange={(event) => handleLicenceDateChange('playerLicenceResetMonth', event.target.value)}
                    required
                  />
                </label>
              </div>
              {form.lastPlayerLicenceResetYear != null && (
                <p className="settings-page__hint">
                  {t('admin.settings.licenceLastReset', {
                    year: form.lastPlayerLicenceResetYear,
                    defaultValue: 'Last licence reset year: {{year}}.',
                  })}
                </p>
              )}
            </fieldset>

            <div className="settings-page__actions">
              <Button type="submit" isLoading={isSaving} disabled={isSaving || isResetting}>
                {t('admin.settings.save', 'Save settings')}
              </Button>
              <Button
                type="button"
                variant="secondary"
                isLoading={isResetting}
                disabled={isSaving || isResetting}
                onClick={() => { void handleResetLicences(); }}
              >
                {t('admin.settings.licenceResetNow', 'Reset player licences now')}
              </Button>
            </div>
          </form>
        )}
      </div>
    </PageTemplate>
  );
}

export default SettingsPage;
