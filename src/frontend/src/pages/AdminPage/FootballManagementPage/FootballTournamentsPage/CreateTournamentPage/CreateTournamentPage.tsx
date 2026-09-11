import { useCallback, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';
import RichTextEditor from '../../../../../components/RichTextEditor';
import { footballTournamentService } from '../../../../../api/football/footballTournamentService';
import {
  FOOTBALL_GROUP_STAGE_RULE_DEFAULTS,
  FOOTBALL_PLAYOFF_RULE_DEFAULTS,
  type CreateFootballTournamentRequest,
} from '../../../../../types/football/tournamentTypes';
import '../../FootballSeasonsPage/EditSeasonPage/EditSeasonPage.scss';

const CreateTournamentPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const currentYear = new Date().getFullYear();

  const [formData, setFormData] = useState<CreateFootballTournamentRequest>({
    name: '',
    startDate: `${currentYear}-06-01`,
    endDate: `${currentYear}-06-03`,
    venue: '',
    contentHtml: '',
    ...FOOTBALL_GROUP_STAGE_RULE_DEFAULTS,
    ...FOOTBALL_PLAYOFF_RULE_DEFAULTS,
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

  const handleContentChange = useCallback((html: string) => {
    setFormData((prev) => ({ ...prev, contentHtml: html }));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      if (!formData.name.trim()) {
        throw new Error(t('football.tournaments.validation.nameRequired', 'Tournament name is required'));
      }
      if (!formData.startDate) {
        throw new Error(t('football.tournaments.validation.startDateRequired', 'Start date is required'));
      }
      if (!formData.endDate) {
        throw new Error(t('football.tournaments.validation.endDateRequired', 'End date is required'));
      }

      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);
      // Same-day tournaments are valid (e.g. a Saturday one-day cup), so only reject when the
      // range is actually inverted. The HTML input also enforces this via `min={startDate}`.
      if (endDate < startDate) {
        throw new Error(t('football.tournaments.validation.endDateNotBeforeStart', 'End date cannot be before start date'));
      }

      const result = await footballTournamentService.create(formData);

      if (successTimeoutId) clearTimeout(successTimeoutId);

      setSuccessMessage(
        t('football.tournaments.created', 'Tournament "{{name}}" created successfully!', { name: formData.name })
      );

      const createdId = result?.data?.id;
      const timeoutId = setTimeout(() => {
        setSuccessMessage(null);
        setSuccessTimeoutId(null);
        if (createdId) {
          navigate(`/admin/football/tournaments/${createdId}/edit`, { replace: true });
        } else {
          navigate('/admin/football/tournaments');
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
    <PageTemplate title={t('football.tournaments.createTitle', 'Create Tournament')}>
      {successMessage && (
        <div className="success-toast">
          <p>{successMessage}</p>
        </div>
      )}

      <div className="edit-season-container">
        <div className="tab-navigation">
          <button className="tab-button active">
            {t('football.tournaments.tabs.details', 'Tournament Details')}
          </button>
          <button className="tab-button" disabled title="Save tournament first to manage groups">
            {t('football.tournaments.tabs.groups', 'Manage Groups')}
          </button>
        </div>

        <div className="edit-season-content">
          <form onSubmit={handleSubmit} className="edit-season-form">
            <ErrorPopup message={error} />

            {/* Basic Information */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-info-circle"></i>
                {t('football.tournaments.sections.basicInfo', 'Basic Information')}
              </h3>

              <div className="form-group">
                <label htmlFor="create-name">
                  {t('football.tournaments.fields.name', 'Name')} *
                </label>
                <input
                  type="text"
                  id="create-name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  placeholder={t('football.tournaments.placeholders.name', 'Enter tournament name')}
                />
              </div>

              <div className="form-group">
                <label htmlFor="create-venue">
                  {t('football.tournaments.fields.venue', 'Venue')}
                </label>
                <input
                  type="text"
                  id="create-venue"
                  name="venue"
                  value={formData.venue ?? ''}
                  onChange={handleInputChange}
                  disabled={loading}
                  placeholder={t('football.tournaments.placeholders.venue', 'e.g. Helsinki Sports Hall')}
                />
              </div>
            </div>

            {/* Schedule */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-calendar-alt"></i>
                {t('football.tournaments.sections.schedule', 'Schedule')}
              </h3>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-startDate">
                    {t('football.tournaments.fields.startDate', 'Start Date')} *
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
                    {t('football.tournaments.fields.endDate', 'End Date')} *
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
                {t('football.tournaments.sections.content', 'Description')}
              </h3>
              <div className="form-group">
                <label htmlFor="create-contentHtml">
                  {t('football.tournaments.fields.contentHtml', 'Content (HTML)')}
                </label>
                <RichTextEditor
                  id="create-contentHtml"
                  value={formData.contentHtml ?? ''}
                  onChange={handleContentChange}
                  readOnly={loading}
                  variant="compact"
                  showMatchInsert={false}
                  placeholder={t('football.tournaments.placeholders.content', 'Tournament description...')}
                />
              </div>
            </div>

            {/* Group Stage Match Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-gavel"></i>
                {t('football.tournaments.sections.groupStageRules', 'Group Stage Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-groupStageNumberOfHalves">
                    {t('football.tournaments.fields.numberOfHalves', 'Number of Halves')}
                  </label>
                  <select
                    id="create-groupStageNumberOfHalves"
                    name="groupStageNumberOfHalves"
                    value={formData.groupStageNumberOfHalves}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    {[1, 2].map((n) => (
                      <option key={n} value={n}>{n}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label htmlFor="create-groupStageHalfDurationMinutes">
                    {t('football.tournaments.fields.halfDurationMinutes', 'Half Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-groupStageHalfDurationMinutes"
                    name="groupStageHalfDurationMinutes"
                    value={formData.groupStageHalfDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-groupStagePlayersOnField">
                    {t('football.tournaments.fields.playersOnField', 'Players on Field')}
                  </label>
                  <input
                    type="number"
                    id="create-groupStagePlayersOnField"
                    name="groupStagePlayersOnField"
                    value={formData.groupStagePlayersOnField}
                    onChange={handleInputChange}
                    min={5}
                    max={11}
                    disabled={loading}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="create-groupStageMaxSubstitutions">
                    {t('football.tournaments.fields.maxSubstitutions', 'Max Substitutions')}
                  </label>
                  <input
                    type="number"
                    id="create-groupStageMaxSubstitutions"
                    name="groupStageMaxSubstitutions"
                    value={formData.groupStageMaxSubstitutions}
                    onChange={handleInputChange}
                    min={0}
                    max={99}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.requireGoalkeeper', 'Require Goalkeeper')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageRequireGoalkeeper ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageRequireGoalkeeper: !prev.groupStageRequireGoalkeeper }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageRequireGoalkeeper}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.requireOfficialsToStart', 'Require Officials to Start')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageRequireOfficialsToStart ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageRequireOfficialsToStart: !prev.groupStageRequireOfficialsToStart }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageRequireOfficialsToStart}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.allowExtraTime', 'Allow Extra Time')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageAllowExtraTime ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageAllowExtraTime: !prev.groupStageAllowExtraTime }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageAllowExtraTime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.groupStageAllowExtraTime && (
                <div className="form-row">
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-groupStageExtraTimeHalfCount">
                      {t('football.tournaments.fields.extraTimeHalfCount', 'Extra Time Halves')}
                    </label>
                    <input
                      type="number"
                      id="create-groupStageExtraTimeHalfCount"
                      name="groupStageExtraTimeHalfCount"
                      value={formData.groupStageExtraTimeHalfCount}
                      onChange={handleInputChange}
                      min={1}
                      max={4}
                      disabled={loading}
                    />
                  </div>
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-groupStageExtraTimeHalfDurationMinutes">
                      {t('football.tournaments.fields.extraTimeHalfDurationMinutes', 'Extra Time Half Duration (min)')}
                    </label>
                    <input
                      type="number"
                      id="create-groupStageExtraTimeHalfDurationMinutes"
                      name="groupStageExtraTimeHalfDurationMinutes"
                      value={formData.groupStageExtraTimeHalfDurationMinutes}
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
                  {t('football.tournaments.fields.allowPenaltyShootout', 'Allow Penalty Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.groupStageAllowPenaltyShootout ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, groupStageAllowPenaltyShootout: !prev.groupStageAllowPenaltyShootout }))}
                  disabled={loading}
                  aria-pressed={formData.groupStageAllowPenaltyShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
            </div>

            {/* Playoff Match Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-trophy"></i>
                {t('football.tournaments.sections.playoffRules', 'Playoff Match Rules')}
              </h3>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-playoffNumberOfHalves">
                    {t('football.tournaments.fields.numberOfHalves', 'Number of Halves')}
                  </label>
                  <select
                    id="create-playoffNumberOfHalves"
                    name="playoffNumberOfHalves"
                    value={formData.playoffNumberOfHalves}
                    onChange={handleInputChange}
                    disabled={loading}
                  >
                    {[1, 2].map((n) => (
                      <option key={n} value={n}>{n}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label htmlFor="create-playoffHalfDurationMinutes">
                    {t('football.tournaments.fields.halfDurationMinutes', 'Half Duration (min)')}
                  </label>
                  <input
                    type="number"
                    id="create-playoffHalfDurationMinutes"
                    name="playoffHalfDurationMinutes"
                    value={formData.playoffHalfDurationMinutes}
                    onChange={handleInputChange}
                    min={1}
                    max={60}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="create-playoffPlayersOnField">
                    {t('football.tournaments.fields.playersOnField', 'Players on Field')}
                  </label>
                  <input
                    type="number"
                    id="create-playoffPlayersOnField"
                    name="playoffPlayersOnField"
                    value={formData.playoffPlayersOnField}
                    onChange={handleInputChange}
                    min={5}
                    max={11}
                    disabled={loading}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="create-playoffMaxSubstitutions">
                    {t('football.tournaments.fields.maxSubstitutions', 'Max Substitutions')}
                  </label>
                  <input
                    type="number"
                    id="create-playoffMaxSubstitutions"
                    name="playoffMaxSubstitutions"
                    value={formData.playoffMaxSubstitutions}
                    onChange={handleInputChange}
                    min={0}
                    max={99}
                    disabled={loading}
                  />
                </div>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.requireGoalkeeper', 'Require Goalkeeper')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffRequireGoalkeeper ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffRequireGoalkeeper: !prev.playoffRequireGoalkeeper }))}
                  disabled={loading}
                  aria-pressed={formData.playoffRequireGoalkeeper}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.requireOfficialsToStart', 'Require Officials to Start')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffRequireOfficialsToStart ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffRequireOfficialsToStart: !prev.playoffRequireOfficialsToStart }))}
                  disabled={loading}
                  aria-pressed={formData.playoffRequireOfficialsToStart}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.allowExtraTime', 'Allow Extra Time')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffAllowExtraTime ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffAllowExtraTime: !prev.playoffAllowExtraTime }))}
                  disabled={loading}
                  aria-pressed={formData.playoffAllowExtraTime}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>

              {formData.playoffAllowExtraTime && (
                <div className="form-row">
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-playoffExtraTimeHalfCount">
                      {t('football.tournaments.fields.extraTimeHalfCount', 'Extra Time Halves')}
                    </label>
                    <input
                      type="number"
                      id="create-playoffExtraTimeHalfCount"
                      name="playoffExtraTimeHalfCount"
                      value={formData.playoffExtraTimeHalfCount}
                      onChange={handleInputChange}
                      min={1}
                      max={4}
                      disabled={loading}
                    />
                  </div>
                  <div className="form-group form-group--indented">
                    <label htmlFor="create-playoffExtraTimeHalfDurationMinutes">
                      {t('football.tournaments.fields.extraTimeHalfDurationMinutes', 'Extra Time Half Duration (min)')}
                    </label>
                    <input
                      type="number"
                      id="create-playoffExtraTimeHalfDurationMinutes"
                      name="playoffExtraTimeHalfDurationMinutes"
                      value={formData.playoffExtraTimeHalfDurationMinutes}
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
                  {t('football.tournaments.fields.allowPenaltyShootout', 'Allow Penalty Shootout')}
                </label>
                <button
                  type="button"
                  className={`toggle-switch ${formData.playoffAllowPenaltyShootout ? 'active' : ''}`}
                  onClick={() => setFormData((prev) => ({ ...prev, playoffAllowPenaltyShootout: !prev.playoffAllowPenaltyShootout }))}
                  disabled={loading}
                  aria-pressed={formData.playoffAllowPenaltyShootout}
                >
                  <span className="toggle-switch__slider" />
                </button>
              </div>
            </div>

            {/* Tournament Rules */}
            <div className="form-section">
              <h3 className="form-section__title">
                <i className="fas fa-cogs"></i>
                {t('football.tournaments.sections.tournamentRules', 'Tournament Rules')}
              </h3>

              <div className="toggle-container">
                <label className="toggle-label">
                  {t('football.tournaments.fields.hasPlayoffStage', 'Has Playoff Stage')}
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
                      {t('football.tournaments.fields.teamsAdvancingPerGroup', 'Teams Advancing Per Group')}
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
                      {t('football.tournaments.fields.hasThirdPlaceMatch', 'Has Third Place Match')}
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
                onClick={() => navigate('/admin/football/tournaments')}
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
