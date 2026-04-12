import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../../api/floorball/floorballTournamentService';
import type { CreateFloorballTournamentRequest } from '../../../../../types/floorball/tournamentTypes';
import '../../FloorballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

const CreateTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const currentYear = new Date().getFullYear();

  const [formData, setFormData] = useState<CreateFloorballTournamentRequest>({
    name: '',
    startDate: `${currentYear}-06-01`,
    endDate: `${currentYear}-06-03`,
    venue: '',
    contentHtml: '',
    groupStageNumberOfPeriods: 2,
    groupStagePeriodDurationMinutes: 15,
    groupStageAllowOvertime: false,
    groupStageOvertimeDurationMinutes: 5,
    groupStageAllowShootout: false,
    playoffNumberOfPeriods: 3,
    playoffPeriodDurationMinutes: 20,
    playoffAllowOvertime: true,
    playoffOvertimeDurationMinutes: 10,
    playoffAllowShootout: true,
    teamsAdvancingPerGroup: 2,
    hasPlayoffStage: true,
    hasThirdPlaceMatch: false,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [successTimeoutId, setSuccessTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (successTimeoutId) clearTimeout(successTimeoutId);
    };
  }, [successTimeoutId]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('floorball.tournaments.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('floorball.tournaments.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('floorball.tournaments.validation.endDateRequired', 'End date is required'));
      }

      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);
      if (endDate < startDate) {
        throw new Error(t('floorball.tournaments.validation.endDateAfterStart', 'End date must be after start date'));
      }

      const result = await floorballTournamentService.create(formData);

      if (successTimeoutId) clearTimeout(successTimeoutId);

      setSuccessMessage(
        t('floorball.tournaments.created', 'Tournament "{{name}}" created successfully!', { name: formData.name })
      );

      const createdId = result?.data?.id;
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        if (createdId) {
          navigate(`/admin/floorball/tournaments/${createdId}/edit`, { replace: true });
        } else {
          navigate('/admin/floorball/tournaments');
        }
      }, 1500);
      setSuccessTimeoutId(timeoutId);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to create tournament';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('floorball.tournaments.createTitle', 'Create Tournament')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-season-container">
        <div className="tab-navigation">
          <button className="tab-button active">
            {t('floorball.tournaments.tabs.details', 'Tournament Details')}
          </button>
          <button className="tab-button" disabled title="Save tournament first to manage groups">
            {t('floorball.tournaments.tabs.groups', 'Manage Groups')}
          </button>
        </div>

        <div className="edit-season-content">
          <form onSubmit={handleSubmit} className="edit-season-form">
            <ErrorPopup message={error} />

            {/* Basic Information */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-info-circle"></i>
                {t('floorball.tournaments.sections.basicInfo', 'Basic Information')}
              </h3>

              <div className="form-group">
                <label htmlFor="create-name">
                  {t('floorball.tournaments.fields.name', 'Name')} *
                </label>
                <input
                  type="text"
                  id="create-name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  placeholder={t('floorball.tournaments.placeholders.name', 'Enter tournament name')}
                />
              </div>

              <div className="form-group">
                <label htmlFor="create-venue">
                  {t('floorball.tournaments.fields.venue', 'Venue')}
                </label>
                <input
                  type="text"
                  id="create-venue"
                  name="venue"
                  value={formData.venue ?? ''}
                  onChange={handleInputChange}
                  disabled={loading}
                  placeholder={t('floorball.tournaments.placeholders.venue', 'e.g. Helsinki Sports Hall')}
                />
              </div>
            </div>

            {/* Schedule */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-calendar-alt"></i>
                {t('floorball.tournaments.sections.schedule', 'Schedule')}
              </h3>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-startDate">
                    {t('floorball.tournaments.fields.startDate', 'Start Date')} *
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
                    {t('floorball.tournaments.fields.endDate', 'End Date')} *
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

            {/* Content */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-file-alt"></i>
                {t('floorball.tournaments.sections.content', 'Description')}
              </h3>
              <div className="form-group">
                <label htmlFor="create-contentHtml">
                  {t('floorball.tournaments.fields.contentHtml', 'Content (HTML)')}
                </label>
                <textarea
                  id="create-contentHtml"
                  name="contentHtml"
                  value={formData.contentHtml ?? ''}
                  onChange={handleInputChange}
                  disabled={loading}
                  rows={5}
                  placeholder={t('floorball.tournaments.placeholders.content', 'Tournament description...')}
                  style={{ width: '100%', resize: 'vertical', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '6px', fontFamily: 'inherit', fontSize: '14px', boxSizing: 'border-box' }}
                />
              </div>
            </div>

            {/* Group Stage Match Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-gavel"></i>
                {t('floorball.tournaments.sections.groupStageRules', 'Group Stage Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-groupStageNumberOfPeriods">
                    {t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}
                  </label>
                  <select
                    id="create-groupStageNumberOfPeriods"
                    name="groupStageNumberOfPeriods"
                    value={formData.groupStageNumberOfPeriods}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    {[1, 2, 3, 4, 5].map((n) => (
                      <option key={n} value={n}>{n}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label htmlFor="create-groupStagePeriodDurationMinutes">
                    {t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-groupStagePeriodDurationMinutes"
                    name="groupStagePeriodDurationMinutes"
                    value={formData.groupStagePeriodDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageAllowOvertime ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageAllowOvertime: !prev.groupStageAllowOvertime }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageAllowOvertime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.groupStageAllowOvertime && (
                <div className="form-group form-group--indented">
                  <label htmlFor="create-groupStageOvertimeDurationMinutes">
                    {t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-groupStageOvertimeDurationMinutes"
                    name="groupStageOvertimeDurationMinutes"
                    value={formData.groupStageOvertimeDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={30}
                    disabled={loading}
                  />
                </div>
              )}

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageAllowShootout ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageAllowShootout: !prev.groupStageAllowShootout }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageAllowShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
            </div>

            {/* Playoff Match Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-trophy"></i>
                {t('floorball.tournaments.sections.playoffRules', 'Playoff Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-playoffNumberOfPeriods">
                    {t('floorball.tournaments.fields.numberOfPeriods', 'Number of Periods')}
                  </label>
                  <select
                    id="create-playoffNumberOfPeriods"
                    name="playoffNumberOfPeriods"
                    value={formData.playoffNumberOfPeriods}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    {[1, 2, 3, 4, 5].map((n) => (
                      <option key={n} value={n}>{n}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label htmlFor="create-playoffPeriodDurationMinutes">
                    {t('floorball.tournaments.fields.periodDuration', 'Period Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-playoffPeriodDurationMinutes"
                    name="playoffPeriodDurationMinutes"
                    value={formData.playoffPeriodDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.tournaments.fields.allowOvertime', 'Allow Overtime')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffAllowOvertime ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffAllowOvertime: !prev.playoffAllowOvertime }))}
                  disabled={loading}
                  aria-pressed={formData.playoffAllowOvertime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.playoffAllowOvertime && (
                <div className="form-group form-group--indented">
                  <label htmlFor="create-playoffOvertimeDurationMinutes">
                    {t('floorball.tournaments.fields.overtimeDuration', 'Overtime Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-playoffOvertimeDurationMinutes"
                    name="playoffOvertimeDurationMinutes"
                    value={formData.playoffOvertimeDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={30}
                    disabled={loading}
                  />
                </div>
              )}

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.tournaments.fields.allowShootout', 'Allow Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffAllowShootout ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffAllowShootout: !prev.playoffAllowShootout }))}
                  disabled={loading}
                  aria-pressed={formData.playoffAllowShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
            </div>

            {/* Tournament Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-cogs"></i>
                {t('floorball.tournaments.sections.tournamentRules', 'Tournament Rules')}
              </h3>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('floorball.tournaments.fields.hasPlayoffStage', 'Has Playoff Stage')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.hasPlayoffStage ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, hasPlayoffStage: !prev.hasPlayoffStage }))}
                  disabled={loading}
                  aria-pressed={formData.hasPlayoffStage}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.hasPlayoffStage && (
                <>
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-teamsAdvancingPerGroup">
                      {t('floorball.tournaments.fields.teamsAdvancingPerGroup', 'Teams Advancing Per Group')}
                    </label>
                    <input
                      type="number"
                      id="create-teamsAdvancingPerGroup"
                      name="teamsAdvancingPerGroup"
                      value={formData.teamsAdvancingPerGroup}
                      onChange={handleInputChange}
                      min={1}
                      max={8}
                      disabled={loading}
                    />
                  </div>

                  <div className="toggle-container">
                    <label className="toggle-label">
                      {t('floorball.tournaments.fields.hasThirdPlaceMatch', 'Has Third Place Match')}
                    </label>
                    <button
                      type="button"
                      className={`toggle-switch ${formData.hasThirdPlaceMatch ? 'active' : ''}`}
                      onClick={() => setFormData((prev) => ({ ...prev, hasThirdPlaceMatch: !prev.hasThirdPlaceMatch }))}
                      disabled={loading}
                      aria-pressed={formData.hasThirdPlaceMatch}
                    >
                      <span className="toggle-switch__slider" />
                    </button>
                  </div>
                </>
              )}
            </div>

            <div className="form-actions">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate('/admin/floorball/tournaments')}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? (
                  <><i className="fas fa-spinner fa-spin"></i> {t('common.creating', 'Creating...')}</>
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

export default CreateTournamentPage;
