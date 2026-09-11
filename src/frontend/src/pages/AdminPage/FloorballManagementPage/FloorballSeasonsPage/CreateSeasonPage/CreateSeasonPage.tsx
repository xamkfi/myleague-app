import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import type { CreateFloorballSeasonRequest } from '../../../../../api/floorball/floorballSeasonService';
import { floorballSeasonService } from '../../../../../api/floorball/floorballSeasonService';
import { useDivisions } from '../../../../../hooks/useDivisions';
import '../EditSeasonPage/EditSeasonPage.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

export const CreateSeasonPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const [formData, setFormData] = useState<CreateFloorballSeasonRequest>({
    name: '',
    startDate: '',
    endDate: '',
    divisionIds: [],
    numberOfPeriods: 2,
    periodDurationMinutes: 15,
    allowOvertime: true,
    overtimeDurationMinutes: 5,
    allowShootout: true
  });
  const [isActive, setIsActive] = useState(false);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }
    };
  }, [successTimeoutId]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value
    }));
  };

  const handleDivisionToggle = (divisionId: string) => {
    setFormData(prev => {
      const currentIds = prev.divisionIds || [];
      const isSelected = currentIds.includes(divisionId);

      if (isSelected) {
        return {
          ...prev,
          divisionIds: currentIds.filter(id => id !== divisionId)
        };
      } else {
        return {
          ...prev,
          divisionIds: [...currentIds, divisionId]
        };
      }
    });
  };

  const parseApiError = (error: unknown): string => {
    const errorMessage = error instanceof Error ? error.message : String(error);

    if (errorMessage.includes('Failed to fetch') || errorMessage.includes('NetworkError')) {
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }
    if (errorMessage.includes('HTTP 400')) {
      return t('floorball.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    if (errorMessage.includes('HTTP 409')) {
      return t('floorball.seasons.errors.conflictError', 'A season with overlapping dates already exists.');
    }
    if (errorMessage.includes('HTTP 500')) {
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    }
    if (errorMessage.includes('overlapping dates') || errorMessage.includes('overlaps with')) {
      return t('floorball.seasons.errors.overlappingDates', 'A season already exists that overlaps with the specified dates.');
    }
    if (errorMessage.includes('Season name is required')) {
      return t('floorball.seasons.validation.nameRequired', 'Season name is required.');
    }
    if (errorMessage.includes('Season name cannot exceed 100 characters')) {
      return t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters.');
    }
    if (errorMessage.includes('Division is required') || errorMessage.includes('Invalid division')) {
      return t('floorball.seasons.validation.divisionRequired', 'Please select a valid division.');
    }
    if (errorMessage.includes('Start date is required')) {
      return t('floorball.seasons.validation.startDateRequired', 'Start date is required.');
    }
    if (errorMessage.includes('End date is required')) {
      return t('floorball.seasons.validation.endDateRequired', 'End date is required.');
    }
    if (errorMessage.includes('End date must be after start date')) {
      return t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date.');
    }

    return errorMessage || t('floorball.seasons.errors.createFailed', 'Failed to create season. Please try again.');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('floorball.seasons.validation.nameRequired', 'Season name is required'));
      }
      if (formData.name.trim().length > 100) {
        throw new Error(t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters'));
      }
      if (!formData.divisionIds || formData.divisionIds.length === 0) {
        throw new Error(t('floorball.seasons.validation.divisionRequired', 'At least one division is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('floorball.seasons.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('floorball.seasons.validation.endDateRequired', 'End date is required'));
      }

      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        throw new Error(t('floorball.seasons.validation.invalidDate', 'Please enter valid dates'));
      }
      if (endDate <= startDate) {
        throw new Error(t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date'));
      }

      const maxDuration = 2 * 365 * 24 * 60 * 60 * 1000;
      if (endDate.getTime() - startDate.getTime() > maxDuration) {
        throw new Error(t('floorball.seasons.validation.seasonTooLong', 'Season duration cannot exceed 2 years'));
      }

      const result = await floorballSeasonService.create(formData);

      // Activate if requested
      if (isActive && result?.data?.id) {
        try {
          await floorballSeasonService.activate(result.data.id);
        } catch {
          // Activation failed silently, user can toggle later
        }
      }

      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }

      const message = t('floorball.seasons.seasonCreated', 'Season "{{seasonName}}" has been created successfully!', {
        seasonName: formData.name
      });
      setSuccessMessage(message);

      // Navigate to edit page after a short delay so the user sees the success message
      // This keeps them on the season and unlocks the division/team tabs
      const createdCompetitionId = result?.data?.id;
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        if (createdCompetitionId) {
          navigate(`/admin/floorball/seasons/${createdCompetitionId}/edit`, { replace: true });
        } else {
          navigate('/admin/floorball/seasons');
        }
      }, 1500);
      setSuccessTimeoutId(timeoutId);
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  // Set default dates (current year season)
  const currentYear = new Date().getFullYear();
  const defaultStartDate = `${currentYear}-01-01`;
  const defaultEndDate = `${currentYear}-12-31`;

  useEffect(() => {
    if (!formData.startDate) {
      setFormData(prev => ({ ...prev, startDate: defaultStartDate }));
    }
    if (!formData.endDate) {
      setFormData(prev => ({ ...prev, endDate: defaultEndDate }));
    }
  }, [defaultStartDate, defaultEndDate, formData.startDate, formData.endDate]);

  return (
    <PageTemplate title={t('floorball.seasons.create.title', 'Create New Season')}>
      {/* Floating Success Toast */}
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-season-container">
        {/* Tab Navigation - same as edit page */}
        <div className="tab-navigation">
          <button className="tab-button active">
            {t('floorball.seasons.seasonDetails', 'Season Details')}
          </button>
          <button className="tab-button" disabled title={t('floorball.seasons.create.saveFirst', 'Save season first to manage divisions')}>
            {t('floorball.seasons.manageDivisions', 'Manage Divisions')}
          </button>
          <button className="tab-button" disabled title={t('floorball.seasons.create.saveFirst', 'Save season first to manage teams')}>
            {t('floorball.seasons.manageTeams', 'Manage Teams')}
          </button>
        </div>

        <div className="edit-season-content">
          <form onSubmit={handleSubmit} className="edit-season-form">
            <ErrorPopup message={error} />

            {/* Basic Information Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-info-circle"></i>
                {t('floorball.seasons.sections.basicInfo', 'Basic Information')}
              </h3>

              <div className="form-group">
                <label htmlFor="create-name">
                  {t('floorball.seasons.fields.name', 'Name')} *
                </label>
                <input
                  type="text"
                  id="create-name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  placeholder={t('floorball.seasons.placeholders.name', 'Enter season name')}
                />
              </div>

              <div className="form-group">
                <label>
                  {t('floorball.seasons.fields.divisions', 'Divisions')} *
                </label>
                <div className="divisions-checkbox-list">
                  {divisions.length === 0 ? (
                    <p className="no-divisions">{t('floorball.seasons.noDivisionsAvailable', 'No divisions available')}</p>
                  ) : (
                    divisions.map(division => (
                      <label key={division.id} className="division-checkbox-item">
                        <input
                          type="checkbox"
                          checked={formData.divisionIds?.includes(division.id) || false}
                          onChange={() => handleDivisionToggle(division.id)}
                          disabled={loading}
                        />
                        <span className="checkbox-label">{division.name}</span>
                      </label>
                    ))
                  )}
                </div>
                {formData.divisionIds && formData.divisionIds.length > 0 && (
                  <p className="selected-count">
                    {t('floorball.seasons.selectedDivisions', '{{count}} division(s) selected', { count: formData.divisionIds.length })}
                  </p>
                )}
              </div>
            </div>

            {/* Schedule Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-calendar-alt"></i>
                {t('floorball.seasons.sections.schedule', 'Schedule')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-startDate">
                    {t('floorball.seasons.fields.startDate', 'Start Date')} *
                  </label>
                  <input
                    type="date"
                    id="create-startDate"
                    name="startDate"
                    value={formData.startDate}
                    onChange={handleInputChange}
                    required
                    disabled={loading}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="create-endDate">
                    {t('floorball.seasons.fields.endDate', 'End Date')} *
                  </label>
                  <input
                    type="date"
                    id="create-endDate"
                    name="endDate"
                    value={formData.endDate}
                    onChange={handleInputChange}
                    required
                    disabled={loading}
                    min={formData.startDate}
                  />
                </div>
              </div>
            </div>

            {/* Match Rules Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-gavel"></i>
                {t('floorball.seasons.fields.matchRules', 'Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-numberOfPeriods">
                    {t('floorball.seasons.fields.numberOfPeriods', 'Number of Periods')}
                  </label>
                  <select
                    id="create-numberOfPeriods"
                    name="numberOfPeriods"
                    value={formData.numberOfPeriods}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    <option value={1}>1</option>
                    <option value={2}>2</option>
                    <option value={3}>3</option>
                    <option value={4}>4</option>
                    <option value={5}>5</option>
                  </select>
                </div>

                <div className="form-group">
                  <label htmlFor="create-periodDurationMinutes">
                    {t('floorball.seasons.fields.periodDurationMinutes', 'Period Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-periodDurationMinutes"
                    name="periodDurationMinutes"
                    value={formData.periodDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              {/* Overtime */}
              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.seasons.fields.allowOvertime', 'Allow Overtime')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.allowOvertime ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    allowOvertime: !prev.allowOvertime
                  }))}
                  disabled={loading}
                  aria-pressed={formData.allowOvertime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.allowOvertime && (
                <div className="form-group form-group--indented">
                  <label htmlFor="create-overtimeDurationMinutes">
                    {t('floorball.seasons.fields.overtimeDurationMinutes', 'Overtime Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-overtimeDurationMinutes"
                    name="overtimeDurationMinutes"
                    value={formData.overtimeDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={30}
                    disabled={loading}
                  />
                </div>
              )}

              {/* Shootout - independent of overtime */}
              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.seasons.fields.allowShootout', 'Allow Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.allowShootout ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    allowShootout: !prev.allowShootout
                  }))}
                  disabled={loading}
                  aria-pressed={formData.allowShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
            </div>

            {/* Season Status Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-toggle-on"></i>
                {t('floorball.seasons.sections.status', 'Status')}
              </h3>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.seasons.fields.isActive', 'Active')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${isActive ? 'active' : ''}`}
                  onClick={() => setIsActive(!isActive)}
                  disabled={loading}
                  aria-pressed={isActive}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
              <div className="info-message">
                <i className="fas fa-info-circle"></i>
                {t('floorball.seasons.create.info', 'The season will be created as inactive by default')}
              </div>
            </div>

            <div className="form-actions">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate('/admin/floorball/seasons')}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="submit"
                className="btn btn-primary"
                disabled={loading}
              >
                {loading ? (
                  <>
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.creating', 'Creating...')}
                  </>
                ) : (
                  t('common.create', 'Create')
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </PageTemplate>
  );
};

export default CreateSeasonPage;
