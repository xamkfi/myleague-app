import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import type { ClubRequest } from '../../../api/common/clubService';
import './ClubForm.scss';
import ConfirmationDialog from '../FloorballManagementPage/ManageMatchPage/components/ConfirmationDialog';

function toIsoFromDmy(dmy: string): string {
  const match = /^([0-9]{2})-([0-9]{2})-([0-9]{4})$/.exec(dmy);
  if (!match) throw new Error('Invalid date format. Use dd-mm-yyyy');
  const [, ddStr, mmStr, yyyyStr] = match;
  const dd = Number(ddStr);
  const mm = Number(mmStr);
  const yyyy = Number(yyyyStr);
  const date = new Date(Date.UTC(yyyy, mm - 1, dd, 0, 0, 0));
  if (Number.isNaN(date.getTime())) throw new Error('Invalid date');
  return date.toISOString();
}

export interface ClubFormValues {
  name: string;
  city: string;
  country: string;
  foundingDate: string;
  websiteUrl?: string;
  logoUrl?: string;
  contactEmail?: string;
}

interface ClubFormProps {
  initialValues?: ClubFormValues;
  submitting?: boolean;
  onSubmit: (payload: ClubRequest) => Promise<void> | void;
  onDelete?: (() => Promise<void> | void) | undefined;
}

function ClubForm({ initialValues, submitting = false, onSubmit, onDelete }: ClubFormProps) {
  const { t } = useTranslation();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [values, setValues] = useState<ClubFormValues>({
    name: '',
    city: '',
    country: '',
    foundingDate: '',
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

  const isValid = useMemo(() => {
    if (!values.name.trim()) return false;
    if (!values.city.trim()) return false;
    if (!values.country.trim()) return false;
    if (!/^\d{2}-\d{2}-\d{4}$/.test(values.foundingDate)) return false;
    return true;
  }, [values]);

  const handleChange = (field: keyof ClubFormValues, val: string) => {
    setValues((prev) => ({ ...prev, [field]: val }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const iso = toIsoFromDmy(values.foundingDate);
      const payload: ClubRequest = {
        name: values.name.trim(),
        city: values.city.trim(),
        country: values.country.trim(),
        foundingDate: iso
      };
      if (values.websiteUrl && values.websiteUrl.trim().length > 0) {
        payload.websiteUrl = values.websiteUrl.trim();
      }
      if (values.logoUrl && values.logoUrl.trim().length > 0) {
        payload.logoUrl = values.logoUrl.trim();
      }
      if (values.contactEmail && values.contactEmail.trim().length > 0) {
        payload.contactEmail = values.contactEmail.trim();
      }
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
            required
            placeholder={t('clubs.form.namePlaceholder', 'Enter club name')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-city">{t('clubs.form.city', 'City')} *</label>
          <input
            id="club-city"
            type="text"
            value={values.city}
            onChange={(e) => handleChange('city', e.target.value)}
            required
            placeholder={t('clubs.form.cityPlaceholder', 'Enter city')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-country">{t('clubs.form.country', 'Country')} *</label>
          <input
            id="club-country"
            type="text"
            value={values.country}
            onChange={(e) => handleChange('country', e.target.value)}
            required
            placeholder={t('clubs.form.countryPlaceholder', 'Enter country')}
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-founding-date">{t('clubs.form.foundingDate', 'Founding Date')} *</label>
          <input
            id="club-founding-date"
            type="text"
            inputMode="numeric"
            pattern="[0-9]{2}-[0-9]{2}-[0-9]{4}"
            title="dd-mm-yyyy"
            placeholder={t('clubs.form.foundingDatePlaceholder', 'dd-mm-yyyy')}
            value={values.foundingDate}
            onChange={(e) => handleChange('foundingDate', e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-website">{t('clubs.form.websiteUrl', 'Website URL')}</label>
          <input
            id="club-website"
            type="url"
            value={values.websiteUrl || ''}
            onChange={(e) => handleChange('websiteUrl', e.target.value)}
            placeholder="https://example.com"
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-logo">{t('clubs.form.logoUrl', 'Logo URL')}</label>
          <input
            id="club-logo"
            type="url"
            value={values.logoUrl || ''}
            onChange={(e) => handleChange('logoUrl', e.target.value)}
            placeholder="https://example.com/logo.png"
          />
        </div>
        <div className="form-group">
          <label htmlFor="club-email">{t('clubs.form.contactEmail', 'Contact Email')}</label>
          <input
            id="club-email"
            type="email"
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
          <button type="submit" className="btn btn-primary" disabled={!isValid || submitting}>
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


