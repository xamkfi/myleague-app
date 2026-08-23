import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { hockeyOfficialService } from '../../../../api/hockey/hockeyOfficialService';
import {
  HOCKEY_OFFICIAL_ROLES,
  type HockeyOfficialDto,
  type HockeyOfficialRole,
  type UpdateHockeyOfficialRequest,
} from '../../../../types/hockey/hockeyTypes';
import { loadPersonNameMap } from '../../../../utils/hockeyLookups';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './EditOfficialPage/EditOfficialPage.scss';

function EditHockeyOfficialPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { officialId } = useParams<{ officialId: string }>();
  const [official, setOfficial] = useState<HockeyOfficialDto | null>(null);
  const [officialName, setOfficialName] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  const [formData, setFormData] = useState<UpdateHockeyOfficialRequest>({
    officialRole: 'Referee',
    officialNumber: '',
    licenseIssueDate: '',
    licenseExpiryDate: '',
    isActive: true,
  });

  const loadOfficial = useCallback(async (): Promise<void> => {
    if (!officialId) {
      return;
    }
    try {
      setLoading(true);
      setError(null);
      const data = await hockeyOfficialService.getById(officialId);
      setOfficial(data);
      const names = await loadPersonNameMap([data.personId]);
      setOfficialName(names.get(data.personId) ?? data.personId.slice(0, 8));
      setFormData({
        officialRole: data.officialRole,
        officialNumber: data.officialNumber ?? '',
        licenseIssueDate: data.licenseIssueDate?.split('T')[0] ?? '',
        licenseExpiryDate: data.licenseExpiryDate?.split('T')[0] ?? '',
        isActive: data.isActive,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.officials.errors.loadFailed', 'Failed to load referee data'));
    } finally {
      setLoading(false);
    }
  }, [officialId, t]);

  useEffect(() => {
    void loadOfficial();
  }, [loadOfficial]);

  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  const handleSubmit = async (event: React.FormEvent): Promise<void> => {
    event.preventDefault();
    if (!officialId) {
      return;
    }
    if (formData.licenseIssueDate && formData.licenseExpiryDate) {
      const issueDate = new Date(formData.licenseIssueDate);
      const expiryDate = new Date(formData.licenseExpiryDate);
      if (expiryDate <= issueDate) {
        setError(t('hockey.officials.validation.expiryAfterIssue', 'License expiry date must be after the issue date'));
        return;
      }
    }
    try {
      setSaving(true);
      setError(null);
      await hockeyOfficialService.update(officialId, {
        officialRole: formData.officialRole,
        officialNumber: formData.officialNumber || null,
        licenseIssueDate: formData.licenseIssueDate || null,
        licenseExpiryDate: formData.licenseExpiryDate || null,
        isActive: formData.isActive,
      });
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
      setSuccessMessage(t('hockey.officials.officialUpdated', 'Referee updated successfully!'));
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        navigate('/admin/hockey/officials');
      }, 2000);
      setSuccessTimeoutId(timeoutId);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('hockey.officials.errors.updateFailed', 'Failed to update referee'));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('hockey.officials.editTitle', 'Edit Referee')}>
        <div className="edit-referee-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!official) {
    return (
      <PageTemplate title={t('hockey.officials.editTitle', 'Edit Referee')}>
        <ErrorPopup message={error ?? t('hockey.officials.errors.notFound', 'Referee not found')} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.officials.editTitle', 'Edit Referee')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}
      <div className="edit-referee-container">
        <div className="edit-referee-header">
          <h2 className="edit-referee-name">{officialName}</h2>
        </div>
        <form onSubmit={(event) => void handleSubmit(event)} className="edit-referee-form">
          <ErrorPopup message={error} />
          <div className="form-section">
            <h3 className="form-section__title">{t('hockey.officials.edit.statusSection', 'Status')}</h3>
            <div className="toggle-container">
              <label className="toggle-label">{t('hockey.officials.edit.isActive', 'Active')}</label>
              <button
                type="button"
                className={`toggle-switch ${formData.isActive ? 'active' : ''}`}
                onClick={() => setFormData((prev) => ({ ...prev, isActive: !prev.isActive }))}
                disabled={saving}
                aria-pressed={formData.isActive}
              >
                <span className="toggle-switch__slider" />
              </button>
            </div>
          </div>
          <div className="form-section">
            <h3 className="form-section__title">{t('hockey.officials.role', 'Role')}</h3>
            <div className="form-group">
              <label htmlFor="edit-role">{t('hockey.officials.role', 'Role')}</label>
              <select
                id="edit-role"
                value={formData.officialRole}
                onChange={(event) => setFormData((prev) => ({ ...prev, officialRole: event.target.value as HockeyOfficialRole }))}
                disabled={saving}
              >
                {HOCKEY_OFFICIAL_ROLES.map((role) => (
                  <option key={role} value={role}>
                    {t(`hockey.officials.roles.${role}`, role)}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="edit-number">{t('hockey.officials.number', 'Number')}</label>
              <input
                id="edit-number"
                value={formData.officialNumber ?? ''}
                onChange={(event) => setFormData((prev) => ({ ...prev, officialNumber: event.target.value }))}
                disabled={saving}
              />
            </div>
          </div>
          <div className="form-section">
            <h3 className="form-section__title">{t('hockey.officials.edit.licenseSection', 'License Information')}</h3>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="edit-licenseIssueDate">{t('hockey.officials.licenseIssueDate', 'License Issue Date')}</label>
                <input
                  type="date"
                  id="edit-licenseIssueDate"
                  value={formData.licenseIssueDate ?? ''}
                  onChange={(event) => setFormData((prev) => ({ ...prev, licenseIssueDate: event.target.value }))}
                  disabled={saving}
                />
              </div>
              <div className="form-group">
                <label htmlFor="edit-licenseExpiryDate">{t('hockey.officials.licenseExpiryDate', 'License Expiry Date')}</label>
                <input
                  type="date"
                  id="edit-licenseExpiryDate"
                  value={formData.licenseExpiryDate ?? ''}
                  onChange={(event) => setFormData((prev) => ({ ...prev, licenseExpiryDate: event.target.value }))}
                  disabled={saving}
                  min={formData.licenseIssueDate ?? undefined}
                />
              </div>
            </div>
          </div>
          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/hockey/officials')} disabled={saving}>
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
}

export default EditHockeyOfficialPage;
