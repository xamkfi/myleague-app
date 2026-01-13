import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import type { ClubRequest } from '../../../api/common/clubService';
import './ClubForm.scss';
import ConfirmationDialog from '../FloorballManagementPage/ManageMatchPage/components/ConfirmationDialog';


interface ClubFormProps {
  initialValues?: ClubRequest;
  submitting?: boolean;
  onSubmit: (payload: ClubRequest) => Promise<void> | void;
  onDelete?: (() => Promise<void> | void) | undefined;
}

function ClubForm({ initialValues, submitting = false, onSubmit, onDelete }: ClubFormProps) {
  const { t } = useTranslation();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
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

  useEffect(() => {
    if (initialValues) {
      setValues((prev) => ({ ...prev, ...initialValues }));
    }
  }, [initialValues]);

  const handleChange = (field: keyof ClubRequest, val: string) => {
    setValues((prev) => ({ ...prev, [field]: val }));
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
      <div className="form-grid">
        <div className="form-group">
          <label htmlFor="club-name">{t('clubs.form.name', 'Club Name')} *</label>
          <input
            id="club-name"
            type="text"
            value={values.name}
            onChange={(e) => handleChange('name', e.target.value)}
            placeholder={t('clubs.form.namePlaceholder', 'Enter club name')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-city">{t('clubs.form.city', 'City')}</label>
          <input
            id="club-city"
            type="text"
            value={values.city ?? ''}
            onChange={(e) => handleChange('city', e.target.value)}
            placeholder={t('clubs.form.cityPlaceholder', 'Enter city')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-country">{t('clubs.form.country', 'Country')}</label>
          <input
            id="club-country"
            type="text"
            value={values.country ?? ''}
            onChange={(e) => handleChange('country', e.target.value)}
            placeholder={t('clubs.form.countryPlaceholder', 'Enter country')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-founding-date">{t('clubs.form.foundingDate', 'Founding Date')}</label>
          <input
            id="club-founding-date"
            type="date"
            value={values.foundingDate || ''}
            onChange={(e) => handleChange('foundingDate', e.target.value)}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-website">{t('clubs.form.websiteUrl', 'Website URL')}</label>
          <input
            id="club-website"
            value={values.websiteUrl || ''}
            onChange={(e) => handleChange('websiteUrl', e.target.value)}
            placeholder="https://example.com"
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-logo">{t('clubs.form.logoUrl', 'Logo URL')}</label>
          <input
            id="club-logo"
            value={values.logoUrl || ''}
            onChange={(e) => handleChange('logoUrl', e.target.value)}
            placeholder="https://example.com/logo.png"
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-email">{t('clubs.form.contactEmail', 'Contact Email')}</label>
          <input
            id="club-email"
            value={values.contactEmail || ''}
            onChange={(e) => handleChange('contactEmail', e.target.value)}
            placeholder="club@example.com"
          />
        </div>
      </div>
      <div className="form-actions">
        <div className="left-actions">
          {onDelete && (
            <button type="button" className="btn btn-danger" onClick={() => setConfirmOpen(true)}> Delete Club
            </button>
          )}
        </div>
        <div className="right-actions">
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


