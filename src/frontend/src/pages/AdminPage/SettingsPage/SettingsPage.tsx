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

function SettingsPage() {
  const { t } = useTranslation();
  const [form, setForm] = useState<SiteSettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
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

            <div className="settings-page__actions">
              <Button type="submit" isLoading={isSaving} disabled={isSaving}>
                {t('admin.settings.save', 'Save settings')}
              </Button>
            </div>
          </form>
        )}
      </div>
    </PageTemplate>
  );
}

export default SettingsPage;
