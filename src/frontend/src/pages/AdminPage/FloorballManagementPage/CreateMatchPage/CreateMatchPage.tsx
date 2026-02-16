import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballRefereeService, type FloorballRefereeDto } from '../../../../api/floorball/floorballRefereeService';
import { useDivisions } from '../../../../hooks/useDivisions';
import type { CreateFloorballMatchRequest, FloorballTeam } from '../../../../types/floorball/floorballTypes';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './CreateMatchPage.scss';

const CreateMatchPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();

  // Data loading
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [selectedSeason, setSelectedSeason] = useState<FloorballSeasonDto | null>(null);
  const [referees, setReferees] = useState<FloorballRefereeDto[]>([]);
  const [loadingSeasons, setLoadingSeasons] = useState(true);
  const [loadingSeasonDetails, setLoadingSeasonDetails] = useState(false);
  const [loadingReferees, setLoadingReferees] = useState(true);

  // Form state
  const [selectedSeasonId, setSelectedSeasonId] = useState('');
  const [selectedDivisionId, setSelectedDivisionId] = useState('');
  const [homeTeamId, setHomeTeamId] = useState('');
  const [awayTeamId, setAwayTeamId] = useState('');
  const [refereeId, setRefereeId] = useState('');
  const [selectedDate, setSelectedDate] = useState('');
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [venue, setVenue] = useState('');

  // UI state
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Load seasons
  useEffect(() => {
    const loadSeasons = async () => {
      try {
        setLoadingSeasons(true);
        const response = await floorballSeasonService.getAll();
        if (response.success && response.data) {
          setSeasons(response.data);
        }
      } catch {
        console.error('Failed to load seasons');
      } finally {
        setLoadingSeasons(false);
      }
    };
    loadSeasons();
  }, []);

  // Load referees
  useEffect(() => {
    const loadReferees = async () => {
      try {
        setLoadingReferees(true);
        const response = await floorballRefereeService.getAll({ page: 1, pageSize: 100 });
        if (response.success && response.data) {
          setReferees(response.data);
        }
      } catch {
        console.error('Failed to load referees');
      } finally {
        setLoadingReferees(false);
      }
    };
    loadReferees();
  }, []);

  // Load full season details when season is selected
  useEffect(() => {
    if (!selectedSeasonId) {
      setSelectedSeason(null);
      return;
    }

    const loadSeasonDetails = async () => {
      try {
        setLoadingSeasonDetails(true);
        const response = await floorballSeasonService.getById(selectedSeasonId);
        if (response.success && response.data) {
          setSelectedSeason(response.data);
        }
      } catch {
        console.error('Failed to load season details');
        setSelectedSeason(null);
      } finally {
        setLoadingSeasonDetails(false);
      }
    };
    loadSeasonDetails();
  }, [selectedSeasonId]);

  // Divisions in the selected season
  const seasonDivisions = useMemo(() => {
    if (!selectedSeason?.seasonDivisions) return [];
    return selectedSeason.seasonDivisions;
  }, [selectedSeason]);

  // Auto-select division if only one
  useEffect(() => {
    if (seasonDivisions.length === 1) {
      setSelectedDivisionId(seasonDivisions[0].divisionId);
    } else if (seasonDivisions.length === 0) {
      setSelectedDivisionId('');
    }
  }, [seasonDivisions]);

  // Reset downstream when season changes
  const handleSeasonChange = useCallback((seasonId: string) => {
    setSelectedSeasonId(seasonId);
    setSelectedDivisionId('');
    setHomeTeamId('');
    setAwayTeamId('');
  }, []);

  // Reset teams when division changes
  const handleDivisionChange = useCallback((divisionId: string) => {
    setSelectedDivisionId(divisionId);
    setHomeTeamId('');
    setAwayTeamId('');
  }, []);

  // Teams in the selected division of the season
  const teamsInDivision = useMemo((): FloorballTeam[] => {
    if (!selectedSeason?.teams || !selectedDivisionId) return [];
    const sd = selectedSeason.seasonDivisions?.find(d => d.divisionId === selectedDivisionId);
    if (!sd?.teamIds) return [];
    const teamIdSet = new Set(sd.teamIds);
    return selectedSeason.teams.filter(team => teamIdSet.has(team.id));
  }, [selectedSeason, selectedDivisionId]);

  // Available away teams (exclude selected home team)
  const availableAwayTeams = useMemo(() => {
    return teamsInDivision.filter(t => t.id !== homeTeamId);
  }, [teamsInDivision, homeTeamId]);

  // Division name helper
  const getDivisionName = useCallback((divisionId: string): string => {
    return divisions.find(d => d.id === divisionId)?.name ?? divisionId;
  }, [divisions]);

  // Date/time helpers
  const handleHoursChange = (value: string) => {
    const num = parseInt(value);
    if (value === '' || (num >= 0 && num <= 23 && value.length <= 2)) {
      setHoursInput(value);
    }
  };

  const handleMinutesChange = (value: string) => {
    const num = parseInt(value);
    if (value === '' || (num >= 0 && num <= 59 && value.length <= 2)) {
      setMinutesInput(value);
    }
  };

  const buildScheduledDateTime = (): string => {
    if (!selectedDate || !hoursInput || !minutesInput) return '';
    const date = new Date(selectedDate);
    date.setHours(parseInt(hoursInput), parseInt(minutesInput), 0, 0);
    return date.toISOString();
  };

  // Submit
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validation
    if (!selectedSeasonId) {
      setError(t('floorball.matches.validation.seasonRequired', 'Please select a season.'));
      return;
    }
    if (!selectedDivisionId) {
      setError(t('floorball.matches.validation.divisionRequired', 'Please select a division.'));
      return;
    }
    if (!homeTeamId) {
      setError(t('floorball.matches.validation.homeTeamRequired', 'Please select a home team.'));
      return;
    }
    if (!awayTeamId) {
      setError(t('floorball.matches.validation.awayTeamRequired', 'Please select an away team.'));
      return;
    }
    if (homeTeamId === awayTeamId) {
      setError(t('floorball.matches.validation.sameTeam', 'Home and away team cannot be the same.'));
      return;
    }
    if (!selectedDate || !hoursInput || !minutesInput) {
      setError(t('floorball.matches.validation.dateTimeRequired', 'Please enter a valid date and time.'));
      return;
    }

    const scheduledDateTime = buildScheduledDateTime();
    if (!scheduledDateTime) {
      setError(t('floorball.matches.validation.dateTimeRequired', 'Please enter a valid date and time.'));
      return;
    }

    setLoading(true);
    try {
      const matchData: CreateFloorballMatchRequest = {
        seasonId: selectedSeasonId,
        homeTeamId,
        awayTeamId,
        refereeId: refereeId || undefined,
        scheduledDateTime,
        venue: venue || undefined
      };

      const response = await floorballMatchService.create(matchData);
      if (response.success) {
        setSuccessMessage(t('floorball.matches.created', 'Match created successfully!'));
        setTimeout(() => {
          navigate('/admin/floorball/matches');
        }, 1500);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to create match';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('floorball.matches.create.title', 'Create Match')}>
      {successMessage && (
        <div className="cm-success-toast"><p>{successMessage}</p></div>
      )}

      <div className="cm-container">
        <div className="cm-card">
          <form onSubmit={handleSubmit} className="cm-form">
            <ErrorPopup message={error} />

            {/* Season Selection */}
            <div className="cm-section">
              <h3 className="cm-section__title">
                <i className="fas fa-trophy"></i>
                {t('floorball.matches.sections.seasonDivision', 'Season & Division')}
              </h3>

              <div className="cm-field">
                <label htmlFor="cm-season">
                  {t('floorball.matches.fields.season', 'Season')} *
                </label>
                <select
                  id="cm-season"
                  value={selectedSeasonId}
                  onChange={(e) => handleSeasonChange(e.target.value)}
                  required
                  disabled={loading || loadingSeasons}
                >
                  <option value="">{loadingSeasons ? t('common.loading', 'Loading...') : t('floorball.matches.placeholders.selectSeason', '-- Select a season --')}</option>
                  {seasons.map(season => (
                    <option key={season.id} value={season.id}>
                      {season.name} {season.isActive ? '(Active)' : ''} {season.isCompleted ? '(Completed)' : ''}
                    </option>
                  ))}
                </select>
              </div>

              {/* Division - only show when season selected */}
              {selectedSeasonId && loadingSeasonDetails && (
                <div className="cm-field">
                  <label>{t('floorball.matches.fields.division', 'Division')} *</label>
                  <div className="cm-field__info">
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.loadingSeasonDetails', 'Loading season details...')}
                  </div>
                </div>
              )}
              {selectedSeasonId && !loadingSeasonDetails && (
                <div className="cm-field">
                  <label htmlFor="cm-division">
                    {t('floorball.matches.fields.division', 'Division')} *
                  </label>
                  {seasonDivisions.length === 0 ? (
                    <div className="cm-field__info cm-field__info--warning">
                      <i className="fas fa-exclamation-triangle"></i>
                      {t('floorball.matches.noDivisionsInSeason', 'This season has no divisions. Please add divisions to the season first.')}
                    </div>
                  ) : seasonDivisions.length === 1 ? (
                    <div className="cm-field__auto-filled">
                      <i className="fas fa-check-circle"></i>
                      <span>{getDivisionName(seasonDivisions[0].divisionId)}</span>
                      <span className="cm-field__auto-tag">{t('common.autoSelected', 'Auto-selected')}</span>
                    </div>
                  ) : (
                    <select
                      id="cm-division"
                      value={selectedDivisionId}
                      onChange={(e) => handleDivisionChange(e.target.value)}
                      required
                      disabled={loading}
                    >
                      <option value="">{t('floorball.matches.placeholders.selectDivision', '-- Select a division --')}</option>
                      {seasonDivisions.map(sd => (
                        <option key={sd.divisionId} value={sd.divisionId}>
                          {getDivisionName(sd.divisionId)} ({sd.teamCount} {t('floorball.seasons.teams', 'teams')})
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              )}
            </div>

            {/* Teams Selection - only show when division selected */}
            {selectedDivisionId && (
              <div className="cm-section">
                <h3 className="cm-section__title">
                  <i className="fas fa-users"></i>
                  {t('floorball.matches.sections.teams', 'Teams')}
                </h3>

                {teamsInDivision.length < 2 ? (
                  <div className="cm-field__info cm-field__info--warning">
                    <i className="fas fa-exclamation-triangle"></i>
                    {t('floorball.matches.notEnoughTeams', 'This division needs at least 2 teams. Please add teams to the season division first.')}
                  </div>
                ) : (
                  <div className="cm-field-row">
                    <div className="cm-field">
                      <label htmlFor="cm-home-team">
                        {t('floorball.matches.fields.homeTeam', 'Home Team')} *
                      </label>
                      <select
                        id="cm-home-team"
                        value={homeTeamId}
                        onChange={(e) => { setHomeTeamId(e.target.value); if (e.target.value === awayTeamId) setAwayTeamId(''); }}
                        required
                        disabled={loading}
                      >
                        <option value="">{t('floorball.matches.placeholders.selectHomeTeam', '-- Select home team --')}</option>
                        {teamsInDivision.map(team => (
                          <option key={team.id} value={team.id}>{team.name}</option>
                        ))}
                      </select>
                    </div>

                    <div className="cm-field-row__vs">
                      <span>VS</span>
                    </div>

                    <div className="cm-field">
                      <label htmlFor="cm-away-team">
                        {t('floorball.matches.fields.awayTeam', 'Away Team')} *
                      </label>
                      <select
                        id="cm-away-team"
                        value={awayTeamId}
                        onChange={(e) => setAwayTeamId(e.target.value)}
                        required
                        disabled={loading || !homeTeamId}
                      >
                        <option value="">{t('floorball.matches.placeholders.selectAwayTeam', '-- Select away team --')}</option>
                        {availableAwayTeams.map(team => (
                          <option key={team.id} value={team.id}>{team.name}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Schedule & Venue */}
            <div className="cm-section">
              <h3 className="cm-section__title">
                <i className="fas fa-calendar-alt"></i>
                {t('floorball.matches.sections.schedule', 'Schedule & Venue')}
              </h3>

              <div className="cm-field">
                <label>
                  {t('floorball.matches.fields.dateTime', 'Date & Time')} *
                </label>
                <div className="cm-datetime">
                  <input
                    type="date"
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                    required
                    disabled={loading}
                    className="cm-datetime__date"
                  />
                  <div className="cm-datetime__time">
                    <input
                      type="number"
                      placeholder="HH"
                      value={hoursInput}
                      onChange={(e) => handleHoursChange(e.target.value)}
                      min={0}
                      max={23}
                      required
                      disabled={loading}
                      className="cm-datetime__input"
                    />
                    <span className="cm-datetime__sep">:</span>
                    <input
                      type="number"
                      placeholder="MM"
                      value={minutesInput}
                      onChange={(e) => handleMinutesChange(e.target.value)}
                      min={0}
                      max={59}
                      required
                      disabled={loading}
                      className="cm-datetime__input"
                    />
                  </div>
                </div>
              </div>

              <div className="cm-field">
                <label htmlFor="cm-venue">
                  {t('floorball.matches.fields.venue', 'Venue')}
                  <span className="cm-optional">{t('common.optional', '(optional)')}</span>
                </label>
                <input
                  type="text"
                  id="cm-venue"
                  value={venue}
                  onChange={(e) => setVenue(e.target.value)}
                  placeholder={t('floorball.matches.placeholders.venue', 'Enter venue name')}
                  disabled={loading}
                />
              </div>
            </div>

            {/* Referee */}
            <div className="cm-section">
              <h3 className="cm-section__title">
                <i className="fas fa-id-badge"></i>
                {t('floorball.matches.sections.referee', 'Referee')}
              </h3>

              <div className="cm-field">
                <label htmlFor="cm-referee">
                  {t('floorball.matches.fields.referee', 'Referee')}
                  <span className="cm-optional">{t('common.optional', '(optional)')}</span>
                </label>
                <select
                  id="cm-referee"
                  value={refereeId}
                  onChange={(e) => setRefereeId(e.target.value)}
                  disabled={loading || loadingReferees}
                >
                  <option value="">{loadingReferees ? t('common.loading', 'Loading...') : t('floorball.matches.placeholders.noReferee', '-- No referee (assign later) --')}</option>
                  {referees.map(ref => (
                    <option key={ref.id} value={ref.id}>{ref.person?.fullName ?? ref.id}</option>
                  ))}
                </select>
                <p className="cm-field__hint">
                  <i className="fas fa-info-circle"></i>
                  {t('floorball.matches.refereeHint', 'You can assign a referee later from the match details page.')}
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="cm-actions">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate('/admin/floorball/matches')}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="submit"
                className="btn btn-primary"
                disabled={loading || !selectedSeasonId || !selectedDivisionId || !homeTeamId || !awayTeamId}
              >
                {loading ? (
                  <><i className="fas fa-spinner fa-spin"></i> {t('common.creating', 'Creating...')}</>
                ) : (
                  <><i className="fas fa-plus"></i> {t('floorball.matches.create.submit', 'Create Match')}</>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </PageTemplate>
  );
};

export default CreateMatchPage;
