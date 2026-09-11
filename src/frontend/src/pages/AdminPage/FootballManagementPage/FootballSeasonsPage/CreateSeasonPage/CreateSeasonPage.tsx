import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import {
  footballSeasonService,
  FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS,
  FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS,
  type CreateFootballSeasonRequest,
} from '../../../../../api/football/footballSeasonService';
import { useDivisions } from '../../../../../hooks/useDivisions';
import { SportsCategory } from '../../../../../types/common/sports';
import '../EditSeasonPage/EditSeasonPage.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

export const CreateSeasonPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();
  const footballDivisions = useMemo(
    () => divisions.filter((division) => division.sportType === SportsCategory.Football),
    [divisions]
  );
  const [formData, setFormData] = useState<CreateFootballSeasonRequest>({
    name: '',
    startDate: '',
    endDate: '',
    divisionIds: [],
    ...FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS,
    ...FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS,
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
      return t('football.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }
    if (errorMessage.includes('HTTP 400')) {
      return t('football.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    if (errorMessage.includes('HTTP 409')) {
      return t('football.seasons.errors.conflictError', 'A season with overlapping dates already exists.');
    }
    if (errorMessage.includes('HTTP 500')) {
      return t('football.seasons.errors.serverError', 'Server error. Please try again later.');
    }
    if (errorMessage.includes('overlapping dates') || errorMessage.includes('overlaps with')) {
      return t('football.seasons.errors.overlappingDates', 'A season already exists that overlaps with the specified dates.');
    }
    if (errorMessage.includes('Season name is required')) {
      return t('football.seasons.validation.nameRequired', 'Season name is required.');
    }
    if (errorMessage.includes('Season name cannot exceed 100 characters')) {
      return t('football.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters.');
    }
    if (errorMessage.includes('Division is required') || errorMessage.includes('Invalid division')) {
      return t('football.seasons.validation.divisionRequired', 'Please select a valid division.');
    }
    if (errorMessage.includes('Start date is required')) {
      return t('football.seasons.validation.startDateRequired', 'Start date is required.');
    }
    if (errorMessage.includes('End date is required')) {
      return t('football.seasons.validation.endDateRequired', 'End date is required.');
    }
    if (errorMessage.includes('End date must be after start date')) {
      return t('football.seasons.validation.endDateAfterStart', 'End date must be after start date.');
    }

    return errorMessage || t('football.seasons.errors.createFailed', 'Failed to create season. Please try again.');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('football.seasons.validation.nameRequired', 'Season name is required'));
      }
      if (formData.name.trim().length > 100) {
        throw new Error(t('football.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters'));
      }
      if (!formData.divisionIds || formData.divisionIds.length === 0) {
        throw new Error(t('football.seasons.validation.divisionRequired', 'At least one division is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('football.seasons.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('football.seasons.validation.endDateRequired', 'End date is required'));
      }

      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        throw new Error(t('football.seasons.validation.invalidDate', 'Please enter valid dates'));
      }
      if (endDate <= startDate) {
        throw new Error(t('football.seasons.validation.endDateAfterStart', 'End date must be after start date'));
      }

      const maxDuration = 2 * 365 * 24 * 60 * 60 * 1000;
      if (endDate.getTime() - startDate.getTime() > maxDuration) {
        throw new Error(t('football.seasons.validation.seasonTooLong', 'Season duration cannot exceed 2 years'));
      }

      const result = await footballSeasonService.create(formData);

      // Activate if requested
      if (isActive && result?.data?.id) {
        try {
          await footballSeasonService.activate(result.data.id);
        } catch {
          // Activation failed silently, user can toggle later
        }
      }

      if (successTimeoutId) {
        clearTimeout(successTimeoutId);
      }

      const message = t('football.seasons.seasonCreated', 'Season "{{seasonName}}" has been created successfully!', {
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
          navigate(`/admin/football/seasons/${createdCompetitionId}/edit`, { replace: true });
        } else {
          navigate('/admin/football/seasons');
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
    <PageTemplate title={t('football.seasons.create.title', 'Create New Season')}>
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
            {t('football.seasons.seasonDetails', 'Season Details')}
          </button>
          <button className="tab-button" disabled title={t('football.seasons.create.saveFirst', 'Save season first to manage divisions')}>
            {t('football.seasons.manageDivisions', 'Manage Divisions')}
          </button>
          <button className="tab-button" disabled title={t('football.seasons.create.saveFirst', 'Save season first to manage teams')}>
            {t('football.seasons.manageTeams', 'Manage Teams')}
          </button>
        </div>

        <div className="edit-season-content">
          <form onSubmit={handleSubmit} className="edit-season-form">
            <ErrorPopup message={error} />

            {/* Basic Information Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-info-circle"></i>
                {t('football.seasons.sections.basicInfo', 'Basic Information')}
              </h3>

              <div className="form-group">
                <label htmlFor="create-name">
                  {t('football.seasons.fields.name', 'Name')} *
                </label>
                <input
                  type="text"
                  id="create-name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  placeholder={t('football.seasons.placeholders.name', 'Enter season name')}
                />
              </div>

              <div className="form-group">
                <label>
                  {t('football.seasons.fields.divisions', 'Divisions')} *
                </label>
                <div className="divisions-checkbox-list">
                  {footballDivisions.length === 0 ? (
                    <p className="no-divisions">{t('football.seasons.noDivisionsAvailable', 'No divisions available')}</p>
                  ) : (
                    footballDivisions.map(division => (
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
                    {t('football.seasons.selectedDivisions', '{{count}} division(s) selected', { count: formData.divisionIds.length })}
                  </p>
                )}
              </div>
            </div>

            {/* Schedule Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-calendar-alt"></i>
                {t('football.seasons.sections.schedule', 'Schedule')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-startDate">
                    {t('football.seasons.fields.startDate', 'Start Date')} *
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
                    {t('football.seasons.fields.endDate', 'End Date')} *
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
                {t('football.seasons.fields.matchRules', 'Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-numberOfHalves">
                    {t('football.seasons.fields.numberOfHalves', 'Number of Halves')}
                  </label>
                  <select
                    id="create-numberOfHalves"
                    name="numberOfHalves"
                    value={formData.numberOfHalves}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    <option value={1}>1</option>
                    <option value={2}>2</option>
                  </select>
                </div>

                <div className="form-group">
                  <label htmlFor="create-halfDurationMinutes">
                    {t('football.seasons.fields.halfDurationMinutes', 'Half Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-halfDurationMinutes"
                    name="halfDurationMinutes"
                    value={formData.halfDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-playersOnField">
                    {t('football.seasons.fields.playersOnField', 'Players on Field')}
                  </label>
                  <input
                    type="number"
                    id="create-playersOnField"
                    name="playersOnField"
                    value={formData.playersOnField}
                    onChange={handleInputChange}
                    min={5}
                    max={11}
                    disabled={loading}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="create-maxSubstitutions">
                    {t('football.seasons.fields.maxSubstitutions', 'Max Substitutions')}
                  </label>
                  <input
                    type="number"
                    id="create-maxSubstitutions"
                    name="maxSubstitutions"
                    value={formData.maxSubstitutions}
                    onChange={handleInputChange}
                    min={0}
                    max={99}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.seasons.fields.requireGoalkeeper', 'Require Goalkeeper')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.requireGoalkeeper ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    requireGoalkeeper: !prev.requireGoalkeeper
                  }))}
                  disabled={loading}
                  aria-pressed={formData.requireGoalkeeper}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.seasons.fields.requireOfficialsToStart', 'Require Officials to Start')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.requireOfficialsToStart ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    requireOfficialsToStart: !prev.requireOfficialsToStart
                  }))}
                  disabled={loading}
                  aria-pressed={formData.requireOfficialsToStart}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.seasons.fields.allowExtraTime', 'Allow Extra Time')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.allowExtraTime ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    allowExtraTime: !prev.allowExtraTime
                  }))}
                  disabled={loading}
                  aria-pressed={formData.allowExtraTime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.allowExtraTime && (
                <div className="form-row">
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-extraTimeHalfCount">
                      {t('football.seasons.fields.extraTimeHalfCount', 'Extra Time Halves')}
                    </label>
                    <input
                      type="number"
                      id="create-extraTimeHalfCount"
                      name="extraTimeHalfCount"
                      value={formData.extraTimeHalfCount}
                      onChange={handleInputChange}
                      min={1}
                      max={4}
                      disabled={loading}
                    />
                  </div>
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-extraTimeHalfDurationMinutes">
                      {t('football.seasons.fields.extraTimeHalfDurationMinutes', 'Extra Time Half Duration (min)')}
                    </label>
                    <input
                      type="number"
                      id="create-extraTimeHalfDurationMinutes"
                      name="extraTimeHalfDurationMinutes"
                      value={formData.extraTimeHalfDurationMinutes}
                      onChange={handleInputChange}
                      min={1}
                      max={30}
                      disabled={loading}
                    />
                  </div>
                </div>
              )}

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.seasons.fields.allowPenaltyShootout', 'Allow Penalty Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.allowPenaltyShootout ? 'active' : ''}`}
                  onClick={() => setFormData(prev => ({
                    ...prev,
                    allowPenaltyShootout: !prev.allowPenaltyShootout
                  }))}
                  disabled={loading}
                  aria-pressed={formData.allowPenaltyShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-winPoints">
                    {t('football.seasons.fields.winPoints', 'Win Points')}
                  </label>
                  <input
                    type="number"
                    id="create-winPoints"
                    name="winPoints"
                    value={formData.winPoints}
                    onChange={handleInputChange}
                    min={0}
                    disabled={loading}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="create-drawPoints">
                    {t('football.seasons.fields.drawPoints', 'Draw Points')}
                  </label>
                  <input
                    type="number"
                    id="create-drawPoints"
                    name="drawPoints"
                    value={formData.drawPoints}
                    onChange={handleInputChange}
                    min={0}
                    disabled={loading}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="create-lossPoints">
                    {t('football.seasons.fields.lossPoints', 'Loss Points')}
                  </label>
                  <input
                    type="number"
                    id="create-lossPoints"
                    name="lossPoints"
                    value={formData.lossPoints}
                    onChange={handleInputChange}
                    min={0}
                    disabled={loading}
                  />
                </div>
              </div>
            </div>

            {/* Season Status Section */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-toggle-on"></i>
                {t('football.seasons.sections.status', 'Status')}
              </h3>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.seasons.fields.isActive', 'Active')}
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
                {t('football.seasons.create.info', 'The season will be created as inactive by default')}
              </div>
            </div>

            <div className="form-actions">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate('/admin/football/seasons')}
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
