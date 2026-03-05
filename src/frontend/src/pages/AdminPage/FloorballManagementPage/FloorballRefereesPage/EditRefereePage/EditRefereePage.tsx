import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import {
  floorballRefereeService,
  type FloorballRefereeDto,
  type UpdateFloorballRefereeRequest,
} from '../../../../../api/floorball/floorballRefereeService';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import './EditRefereePage.scss';

const EditRefereePage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { refereeId } = useParams<{ refereeId: string }>();

  const [referee, setReferee] = useState<FloorballRefereeDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  const [formData, setFormData] = useState<UpdateFloorballRefereeRequest>({
    licenseIssueDate: '',
    licenseExpiryDate: '',
    matchesOfficiated: 0,
    isActive: true,
  });

  const loadReferee = useCallback(async () => {
    if (!refereeId) return;
    try {
      setLoading(true);
      setError(null);
      const data = await floorballRefereeService.getById(refereeId);
      setReferee(data);
      setFormData({
        licenseIssueDate: data.licenseIssueDate?.split('T')[0] ?? '',
        licenseExpiryDate: data.licenseExpiryDate?.split('T')[0] ?? '',
        matchesOfficiated: data.matchesOfficiated,
        isActive: data.isActive,
      });
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('floorball.referees.errors.loadFailed', 'Failed to load referee data')
      );
    } finally {
      setLoading(false);
    }
  }, [refereeId, t]);

  useEffect(() => {
    loadReferee();
  }, [loadReferee]);

  useEffect(() => {
    return () => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
    };
  }, [successTimeoutId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!refereeId) return;

    if (formData.licenseIssueDate && formData.licenseExpiryDate) {
      const issueDate = new Date(formData.licenseIssueDate);
      const expiryDate = new Date(formData.licenseExpiryDate);
      if (expiryDate <= issueDate) {
        setError(
          t(
            'floorball.referees.validation.expiryAfterIssue',
            'License expiry date must be after the issue date'
          )
        );
        return;
      }
    }

    if (formData.matchesOfficiated < 0) {
      setError(
        t(
          'floorball.referees.validation.matchesNonNegative',
          'Matches officiated cannot be negative'
        )
      );
      return;
    }

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);

      await floorballRefereeService.update(refereeId, formData);

      if (successTimeoutId) clearTimeout(successTimeoutId);

      setSuccessMessage(
        t('floorball.referees.refereeUpdated', 'Referee updated successfully!')
      );

      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        navigate('/admin/floorball/referees');
      }, 2000);
      setSuccessTimeoutId(timeoutId);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('floorball.referees.errors.updateFailed', 'Failed to update referee')
      );
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.referees.edit.title', 'Edit Referee')}>
        <div className="edit-referee-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!referee) {
    return (
      <PageTemplate title={t('floorball.referees.edit.title', 'Edit Referee')}>
        <ErrorPopup
          message={error ?? t('floorball.referees.errors.notFound', 'Referee not found')}
        />
      </PageTemplate>
    );
  }

  const refereeName = [referee.person.firstName, referee.person.lastName]
    .filter(Boolean)
    .join(' ') || '-';

  return (
    <PageTemplate
      title={t('floorball.referees.edit.title', 'Edit Referee')}
    >
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-referee-container">
        <div className="edit-referee-header">
          <h2 className="edit-referee-name">{refereeName}</h2>
          {referee.person.email && (
            <span className="edit-referee-email">{referee.person.email}</span>
          )}
        </div>

        <form onSubmit={handleSubmit} className="edit-referee-form">
          <ErrorPopup message={error} />

          <div className="form-section">
            <h3 className="form-section__title">
              {t('floorball.referees.edit.statusSection', 'Status')}
            </h3>
            <div className="toggle-container">
              <label className="toggle-label">
                {t('floorball.referees.edit.isActive', 'Active')}
              </label>
              <button
                type="button"
                className={`toggle-switch ${formData.isActive ? 'active' : ''}`}
                onClick={() =>
                  setFormData((prev) => ({ ...prev, isActive: !prev.isActive }))
                }
                disabled={saving}
                aria-pressed={formData.isActive}
              >
                <span className="toggle-switch__slider" />
              </button>
            </div>
          </div>

          <div className="form-section">
            <h3 className="form-section__title">
              {t('floorball.referees.edit.licenseSection', 'License Information')}
            </h3>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="edit-licenseIssueDate">
                  {t('floorball.referees.licenseIssueDate', 'License Issue Date')}
                </label>
                <input
                  type="date"
                  id="edit-licenseIssueDate"
                  value={formData.licenseIssueDate ?? ''}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      licenseIssueDate: e.target.value,
                    }))
                  }
                  disabled={saving}
                />
              </div>
              <div className="form-group">
                <label htmlFor="edit-licenseExpiryDate">
                  {t('floorball.referees.licenseExpiryDate', 'License Expiry Date')}
                </label>
                <input
                  type="date"
                  id="edit-licenseExpiryDate"
                  value={formData.licenseExpiryDate ?? ''}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      licenseExpiryDate: e.target.value,
                    }))
                  }
                  disabled={saving}
                  min={formData.licenseIssueDate ?? undefined}
                />
              </div>
            </div>
          </div>

          <div className="form-section">
            <h3 className="form-section__title">
              {t('floorball.referees.edit.statsSection', 'Statistics')}
            </h3>
            <div className="form-group">
              <label htmlFor="edit-matchesOfficiated">
                {t('floorball.referees.table.matchesOfficiated', 'Matches Officiated')}
              </label>
              <input
                type="number"
                id="edit-matchesOfficiated"
                value={formData.matchesOfficiated}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    matchesOfficiated: parseInt(e.target.value, 10) || 0,
                  }))
                }
                min={0}
                disabled={saving}
              />
            </div>
          </div>

          <div className="form-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => navigate('/admin/floorball/referees')}
              disabled={saving}
            >
              {t('common.cancel', 'Cancel')}
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving
                ? t('common.saving', 'Saving...')
                : t('common.save', 'Save')}
            </button>
          </div>
        </form>
      </div>
    </PageTemplate>
  );
};

export default EditRefereePage;
