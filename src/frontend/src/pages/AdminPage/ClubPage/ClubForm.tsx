import { useEffect, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import { clubService, type ClubRequest } from '../../../api/common/clubService';
import './ClubForm.scss';
import ConfirmationDialog from '../FloorballManagementPage/ManageMatchPage/components/ConfirmationDialog';

interface ClubFormProps {
  initialValues?: ClubRequest;
  submitting?: boolean;
  onSubmit: (payload: ClubRequest) => Promise<void> | void;
  onDelete?: (() => Promise<void> | void) | undefined;
  beforeActions?: ReactNode;
}

function ClubForm({ initialValues, submitting = false, onSubmit, onDelete, beforeActions }: ClubFormProps) {
  const { t } = useTranslation();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [uploadingLogo, setUploadingLogo] = useState(false);
  const [values, setValues] = useState<ClubRequest>({
    name: '',
    city: '',
    country: '',
    foundingDate: null,
    websiteUrl: '',
    logoUrl: '',
    contactEmail: '',
    ...(initialValues || {})
  });
  const [error, setError] = useState<string | null>(null);

  // Sync from initialValues when parent passes new data (e.g. club loaded or changed).
  // EditClubPage memoizes initialValues by [club] so this does not overwrite in-form edits.
  useEffect(() => {
    if (initialValues) {
      setValues((prev) => ({ ...prev, ...initialValues }));
    }
  }, [initialValues]);

  const handleChange = (field: keyof ClubRequest, val: string) => {
    setValues((prev) => ({ ...prev, [field]: val }));
  };

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      setError(t('clubs.form.errorInvalidImage'));
      return;
    }
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      setError(t('clubs.form.errorImageTooLarge'));
      return;
    }
    setError(null);
    try {
      setUploadingLogo(true);
      const url = await clubService.uploadLogo(file);
      handleChange('logoUrl', url);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setUploadingLogo(false);
    }
  };

  const removeLogo = () => {
    handleChange('logoUrl', '');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    
    const payload: ClubRequest = {
      name: values.name.trim(),
      ...(values.city && values.city.trim().length > 0 && { city: values.city.trim() }),
      ...(values.country && values.country.trim().length > 0 && { country: values.country.trim() }),
      ...(values.foundingDate && values.foundingDate !== '' && { foundingDate: values.foundingDate }),
      ...(values.websiteUrl && values.websiteUrl.trim().length > 0 && { websiteUrl: values.websiteUrl.trim() }),
      ...(values.logoUrl && values.logoUrl.trim().length > 0 && { logoUrl: values.logoUrl.trim() }),
      ...(values.contactEmail && values.contactEmail.trim().length > 0 && { contactEmail: values.contactEmail.trim() })
    };
    
    try {
      await onSubmit(payload);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    }

  };

  return (
    <form className="club-form" onSubmit={handleSubmit}>
      <ErrorPopup message={error} />

      <section className="club-form__section">
        <header className="club-form__section-header">
          <h3>{t('clubs.form.basicSection')}</h3>
        </header>
        <div className="club-form__field">
          <label htmlFor="club-name">
            {t('clubs.form.name')}
            <span className="club-form__required" aria-hidden="true">*</span>
          </label>
          <input
            id="club-name"
            type="text"
            required
            value={values.name}
            onChange={(e) => handleChange('name', e.target.value)}
            placeholder={t('clubs.form.namePlaceholder')}
          />
        </div>
        <div className="club-form__grid">
          <div className="club-form__field">
            <label htmlFor="club-city">
              {t('clubs.form.city')}
              <span className="club-form__optional">{t('clubs.form.optional')}</span>
            </label>
            <input
              id="club-city"
              type="text"
              value={values.city ?? ''}
              onChange={(e) => handleChange('city', e.target.value)}
              placeholder={t('clubs.form.cityPlaceholder')}
            />
          </div>
          <div className="club-form__field">
            <label htmlFor="club-country">
              {t('clubs.form.country')}
              <span className="club-form__optional">{t('clubs.form.optional')}</span>
            </label>
            <input
              id="club-country"
              type="text"
              value={values.country ?? ''}
              onChange={(e) => handleChange('country', e.target.value)}
              placeholder={t('clubs.form.countryPlaceholder')}
            />
          </div>
          <div className="club-form__field">
            <label htmlFor="club-founding-date">
              {t('clubs.form.foundingDate')}
              <span className="club-form__optional">{t('clubs.form.optional')}</span>
            </label>
            <input
              id="club-founding-date"
              type="date"
              value={values.foundingDate || ''}
              onChange={(e) => handleChange('foundingDate', e.target.value)}
            />
          </div>
        </div>
      </section>

      <section className="club-form__section">
        <header className="club-form__section-header">
          <h3>{t('clubs.form.contactSection')}</h3>
        </header>
        <div className="club-form__grid">
          <div className="club-form__field">
            <label htmlFor="club-website">
              {t('clubs.form.websiteUrl')}
              <span className="club-form__optional">{t('clubs.form.optional')}</span>
            </label>
            <input
              id="club-website"
              type="url"
              value={values.websiteUrl || ''}
              onChange={(e) => handleChange('websiteUrl', e.target.value)}
              placeholder="https://"
            />
          </div>
          <div className="club-form__field">
            <label htmlFor="club-email">
              {t('clubs.form.contactEmail')}
              <span className="club-form__optional">{t('clubs.form.optional')}</span>
            </label>
            <input
              id="club-email"
              type="email"
              value={values.contactEmail || ''}
              onChange={(e) => handleChange('contactEmail', e.target.value)}
              placeholder={t('clubs.form.emailPlaceholder')}
            />
          </div>
        </div>
      </section>

      <section className="club-form__section">
        <div className="club-form__field club-form__logo-group">
          <label className="club-form__logo-label" id="club-logo-label">
            {t('clubs.form.logoUrl')}
            <span className="club-form__optional">{t('clubs.form.optional')}</span>
          </label>
          {values.logoUrl?.trim() ? (
            <div className="club-form__logo-preview" aria-labelledby="club-logo-label">
              <div className="club-form__logo-preview-inner">
                <img
                  key={values.logoUrl}
                  src={values.logoUrl}
                  alt={t('clubs.form.logoPreviewAlt')}
                  className="club-form__logo-img"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.style.display = 'none';
                  }}
                />
                <div className="club-form__logo-preview-actions">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleLogoUpload}
                    className="club-form__logo-file-input"
                    id="club-logo-replace"
                    disabled={uploadingLogo}
                    aria-label={t('clubs.form.replaceLogo')}
                  />
                  <label htmlFor="club-logo-replace" className="btn btn-secondary club-form__logo-btn">
                    {uploadingLogo ? t('common.loading') : t('clubs.form.replaceLogo')}
                  </label>
                  <button
                    type="button"
                    className="btn btn-danger club-form__logo-btn"
                    onClick={removeLogo}
                    title={t('clubs.form.removeLogo')}
                  >
                    {t('clubs.form.removeLogo')}
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div
              className={`club-form__logo-dropzone ${uploadingLogo ? 'club-form__logo-dropzone--uploading' : ''}`}
              aria-labelledby="club-logo-label"
            >
              <input
                type="file"
                accept="image/*"
                onChange={handleLogoUpload}
                className="club-form__logo-file-input"
                id="club-logo-upload"
                disabled={uploadingLogo}
                aria-label={t('clubs.form.uploadLogo')}
              />
              <label htmlFor="club-logo-upload" className="club-form__logo-dropzone-label">
                <span className="club-form__logo-dropzone-text">
                  {uploadingLogo ? t('clubs.form.uploadingLogo') : t('clubs.form.clickToUploadLogo')}
                </span>
                <span className="club-form__logo-dropzone-hint">
                  PNG, JPG, GIF, WebP — {t('clubs.form.upTo')} 5MB
                </span>
              </label>
            </div>
          )}
          <label htmlFor="club-logo-url" className="club-form__logo-url-label">
            {t('clubs.form.orPasteUrl')}
          </label>
          <input
            id="club-logo-url"
            type="url"
            value={values.logoUrl ?? ''}
            onChange={(e) => handleChange('logoUrl', e.target.value)}
            placeholder={t('clubs.form.logoUrlPlaceholder')}
            className="club-form__logo-url-input"
          />
        </div>
      </section>

      {beforeActions && (
        <section className="club-form__section club-form__section--admins">
          {beforeActions}
        </section>
      )}

      <div className="club-form__actions">
        <div className="club-form__actions-left">
          {onDelete && (
            <button type="button" className="btn btn-danger" onClick={() => setConfirmOpen(true)}>
              {t('clubs.deleteClubButton')}
            </button>
          )}
        </div>
        <div className="club-form__actions-right">
          <button type="submit" className="btn btn-primary">
            {submitting ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
          </button>
        </div>
      </div>

      {onDelete && (
        <ConfirmationDialog
          isOpen={confirmOpen}
          icon="⚠️"
          title={t('clubs.confirmDeleteTitle', 'Delete club?')}
          message={t('clubs.confirmDelete', 'Are you sure you want to delete this club?')}
          warningMessage={t('clubs.confirmDeleteWarning','This action cannot be undone.')}
          confirmText={t('common.delete', 'Delete')}
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={deleting}
          onConfirm={async () => {
            try {
              setDeleting(true);
              await onDelete();
              setConfirmOpen(false);
            } finally {
              setDeleting(false);
            }
          }}
          onCancel={() => setConfirmOpen(false)}
        />
      )}
    </form>
  );
}

export default ClubForm;
