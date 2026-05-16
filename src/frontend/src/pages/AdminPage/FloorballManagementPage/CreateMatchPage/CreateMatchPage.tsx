import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballRefereeService, type FloorballRefereeDto } from '../../../../api/floorball/floorballRefereeService';
import { floorballTournamentService } from '../../../../api/floorball/floorballTournamentService';
import type {
  FloorballTournamentDto,
  FloorballTournamentGroupDto,
} from '../../../../types/floorball/tournamentTypes';
import { useDivisions } from '../../../../hooks/useDivisions';
import type { CreateFloorballMatchRequest, FloorballTeam } from '../../../../types/floorball/floorballTypes';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './CreateMatchPage.scss';

export type CreateMatchMode = 'season' | 'tournament';

interface CreateMatchPageProps {
  /**
   * Determines what kind of competition the match belongs to.
   * - `season`     : Pick a Season → Division → Teams in division.
   * - `tournament` : Pick a Tournament → Group → Teams in group.
   * Defaults to `season` for backwards compatibility with the legacy
   * `/admin/floorball/matches/create` route.
   */
  mode?: CreateMatchMode;
}

interface TeamOption {
  id: string;
  name: string;
}

const CreateMatchPage = ({ mode = 'season' }: CreateMatchPageProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { divisions } = useDivisions();

  const isTournament = mode === 'tournament';

  // ── Data loading (competitions) ──────────────────────────────────────
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [tournaments, setTournaments] = useState<FloorballTournamentDto[]>([]);
  const [loadingCompetitions, setLoadingCompetitions] = useState(true);

  const [selectedSeason, setSelectedSeason] = useState<FloorballSeasonDto | null>(null);
  const [selectedTournament, setSelectedTournament] = useState<FloorballTournamentDto | null>(null);
  const [loadingCompetitionDetails, setLoadingCompetitionDetails] = useState(false);

  // ── Referees ─────────────────────────────────────────────────────────
  const [referees, setReferees] = useState<FloorballRefereeDto[]>([]);
  const [loadingReferees, setLoadingReferees] = useState(true);

  // ── Form state ───────────────────────────────────────────────────────
  const [selectedCompetitionId, setSelectedCompetitionId] = useState('');
  const [selectedDivisionId, setSelectedDivisionId] = useState('');
  const [selectedGroupId, setSelectedGroupId] = useState('');
  const [homeTeamId, setHomeTeamId] = useState('');
  const [awayTeamId, setAwayTeamId] = useState('');
  const [refereeId, setRefereeId] = useState('');
  const [selectedDate, setSelectedDate] = useState('');
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [venue, setVenue] = useState('');

  // ── UI state ─────────────────────────────────────────────────────────
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // ── Load competitions (seasons or tournaments depending on mode) ─────
  useEffect(() => {
    const load = async () => {
      try {
        setLoadingCompetitions(true);
        if (isTournament) {
          const response = await floorballTournamentService.getAll();
          if (response.success && response.data) {
            setTournaments(response.data);
          }
        } else {
          const response = await floorballSeasonService.getAll();
          if (response.success && response.data) {
            setSeasons(response.data);
          }
        }
      } catch (err) {
        console.error('Failed to load competitions', err);
      } finally {
        setLoadingCompetitions(false);
      }
    };
    load();
  }, [isTournament]);

  // ── Load referees ────────────────────────────────────────────────────
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

  // ── Load selected competition details ────────────────────────────────
  useEffect(() => {
    if (!selectedCompetitionId) {
      setSelectedSeason(null);
      setSelectedTournament(null);
      return;
    }

    let cancelled = false;
    const loadDetails = async () => {
      try {
        setLoadingCompetitionDetails(true);
        if (isTournament) {
          const response = await floorballTournamentService.getById(selectedCompetitionId);
          if (!cancelled && response.success && response.data) {
            setSelectedTournament(response.data);
          }
        } else {
          const response = await floorballSeasonService.getById(selectedCompetitionId);
          if (!cancelled && response.success && response.data) {
            setSelectedSeason(response.data);
          }
        }
      } catch {
        console.error('Failed to load competition details');
        if (!cancelled) {
          setSelectedSeason(null);
          setSelectedTournament(null);
        }
      } finally {
        if (!cancelled) {
          setLoadingCompetitionDetails(false);
        }
      }
    };
    loadDetails();
    return () => {
      cancelled = true;
    };
  }, [selectedCompetitionId, isTournament]);

  // ── Derived: divisions / groups available in the selected competition ─
  const seasonDivisions = useMemo(() => {
    if (isTournament) return [];
    return selectedSeason?.seasonDivisions ?? [];
  }, [isTournament, selectedSeason]);

  const tournamentGroups = useMemo<FloorballTournamentGroupDto[]>(() => {
    if (!isTournament) return [];
    return selectedTournament?.groups ?? [];
  }, [isTournament, selectedTournament]);

  // Auto-select the only available division/group
  useEffect(() => {
    if (isTournament) {
      if (tournamentGroups.length === 1) {
        setSelectedGroupId(tournamentGroups[0].id);
      } else if (tournamentGroups.length === 0) {
        setSelectedGroupId('');
      }
    } else {
      if (seasonDivisions.length === 1) {
        setSelectedDivisionId(seasonDivisions[0].divisionId);
      } else if (seasonDivisions.length === 0) {
        setSelectedDivisionId('');
      }
    }
  }, [isTournament, seasonDivisions, tournamentGroups]);

  // ── Reset downstream when the competition changes ────────────────────
  const handleCompetitionChange = useCallback((competitionId: string) => {
    setSelectedCompetitionId(competitionId);
    setSelectedDivisionId('');
    setSelectedGroupId('');
    setHomeTeamId('');
    setAwayTeamId('');
  }, []);

  const handleDivisionChange = useCallback((divisionId: string) => {
    setSelectedDivisionId(divisionId);
    setHomeTeamId('');
    setAwayTeamId('');
  }, []);

  const handleGroupChange = useCallback((groupId: string) => {
    setSelectedGroupId(groupId);
    setHomeTeamId('');
    setAwayTeamId('');
  }, []);

  // ── Derived: teams available in the selected division / group ────────
  const teamsAvailable = useMemo<TeamOption[]>(() => {
    if (isTournament) {
      const group = tournamentGroups.find((g) => g.id === selectedGroupId);
      if (!group) return [];
      return group.teams.map((team) => ({ id: team.teamId, name: team.teamName }));
    }

    if (!selectedSeason?.teams || !selectedDivisionId) return [];
    const sd = selectedSeason.seasonDivisions?.find((d) => d.divisionId === selectedDivisionId);
    if (!sd?.teamIds) return [];
    const teamIdSet = new Set(sd.teamIds);
    return selectedSeason.teams
      .filter((team: FloorballTeam) => teamIdSet.has(team.id))
      .map((team: FloorballTeam) => ({ id: team.id, name: team.name }));
  }, [isTournament, tournamentGroups, selectedGroupId, selectedSeason, selectedDivisionId]);

  const availableAwayTeams = useMemo(
    () => teamsAvailable.filter((team) => team.id !== homeTeamId),
    [teamsAvailable, homeTeamId]
  );

  // Helpers
  const getDivisionName = useCallback(
    (divisionId: string): string => divisions.find((d) => d.id === divisionId)?.name ?? divisionId,
    [divisions]
  );

  // ── Date/time helpers ────────────────────────────────────────────────
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

  // ── Navigation targets ───────────────────────────────────────────────
  const listPath = isTournament
    ? '/admin/floorball/tournaments/matches'
    : '/admin/floorball/seasons/matches';

  // ── Submit ───────────────────────────────────────────────────────────
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!selectedCompetitionId) {
      setError(
        isTournament
          ? t('floorball.matches.validation.tournamentRequired', 'Please select a tournament.')
          : t('floorball.matches.validation.seasonRequired', 'Please select a season.')
      );
      return;
    }
    if (isTournament) {
      if (!selectedGroupId) {
        setError(t('floorball.matches.validation.groupRequired', 'Please select a group.'));
        return;
      }
    } else {
      if (!selectedDivisionId) {
        setError(t('floorball.matches.validation.divisionRequired', 'Please select a division.'));
        return;
      }
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
        competitionId: selectedCompetitionId,
        homeTeamId,
        awayTeamId,
        refereeId: refereeId || undefined,
        scheduledDateTime,
        venue: venue || undefined,
        ...(isTournament && {
          tournamentGroupId: selectedGroupId,
          tournamentStage: 'GroupStage',
        }),
      };

      const response = await floorballMatchService.create(matchData);
      if (response.success) {
        setSuccessMessage(t('floorball.matches.created', 'Match created successfully!'));
        setTimeout(() => navigate(listPath), 1500);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to create match';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  // ── Render ───────────────────────────────────────────────────────────
  const pageTitle = isTournament
    ? t('floorball.matches.create.tournamentTitle', 'Create Tournament Match')
    : t('floorball.matches.create.seasonTitle', 'Create Season Match');

  const sectionTitle = isTournament
    ? t('floorball.matches.sections.tournamentGroup', 'Tournament & Group')
    : t('floorball.matches.sections.seasonDivision', 'Season & Division');

  const competitionLabel = isTournament
    ? t('floorball.matches.fields.tournament', 'Tournament')
    : t('floorball.matches.fields.season', 'Season');

  const competitionPlaceholder = loadingCompetitions
    ? t('common.loading', 'Loading...')
    : isTournament
      ? t('floorball.matches.placeholders.selectTournament', '-- Select a tournament --')
      : t('floorball.matches.placeholders.selectSeason', '-- Select a season --');

  const subdivisionLabel = isTournament
    ? t('floorball.matches.fields.group', 'Group')
    : t('floorball.matches.fields.division', 'Division');

  const competitions = isTournament ? tournaments : seasons;

  const hasGroupsOrDivisions = isTournament ? tournamentGroups.length > 0 : seasonDivisions.length > 0;
  const subdivisionResolved = isTournament ? !!selectedGroupId : !!selectedDivisionId;

  return (
    <PageTemplate title={pageTitle}>
      {successMessage && (
        <div className="cm-success-toast"><p>{successMessage}</p></div>
      )}

      <div className="cm-container">
        <div className="cm-card">
          <form onSubmit={handleSubmit} className="cm-form">
            <ErrorPopup message={error} />

            {/* Competition Selection */}
            <div className="cm-section">
              <h3 className="cm-section__title">
                <i className="fas fa-trophy"></i>
                {sectionTitle}
              </h3>

              <div className="cm-field">
                <label htmlFor="cm-competition">
                  {competitionLabel} *
                </label>
                <select
                  id="cm-competition"
                  value={selectedCompetitionId}
                  onChange={(e) => handleCompetitionChange(e.target.value)}
                  required
                  disabled={loading || loadingCompetitions}
                >
                  <option value="">{competitionPlaceholder}</option>
                  {competitions.map((c) => (
                    <option key={c.id} value={c.id}>
                      {isTournament
                        ? c.name
                        : `${(c as FloorballSeasonDto).name}` +
                          ((c as FloorballSeasonDto).isActive ? ' (Active)' : '') +
                          ((c as FloorballSeasonDto).isCompleted ? ' (Completed)' : '')}
                    </option>
                  ))}
                </select>
              </div>

              {/* Division / Group — only show when a competition is selected */}
              {selectedCompetitionId && loadingCompetitionDetails && (
                <div className="cm-field">
                  <label>{subdivisionLabel} *</label>
                  <div className="cm-field__info">
                    <i className="fas fa-spinner fa-spin"></i>
                    {isTournament
                      ? t('floorball.matches.loadingTournamentDetails', 'Loading tournament details...')
                      : t('common.loadingSeasonDetails', 'Loading season details...')}
                  </div>
                </div>
              )}

              {selectedCompetitionId && !loadingCompetitionDetails && isTournament && (
                <div className="cm-field">
                  <label htmlFor="cm-group">{subdivisionLabel} *</label>
                  {!hasGroupsOrDivisions ? (
                    <div className="cm-field__info cm-field__info--warning">
                      <i className="fas fa-exclamation-triangle"></i>
                      {t(
                        'floorball.matches.noGroupsInTournament',
                        'This tournament has no groups. Add groups in the tournament edit page first.'
                      )}
                    </div>
                  ) : tournamentGroups.length === 1 ? (
                    <div className="cm-field__auto-filled">
                      <i className="fas fa-check-circle"></i>
                      <span>{tournamentGroups[0].name}</span>
                      <span className="cm-field__auto-tag">{t('common.autoSelected', 'Auto-selected')}</span>
                    </div>
                  ) : (
                    <select
                      id="cm-group"
                      value={selectedGroupId}
                      onChange={(e) => handleGroupChange(e.target.value)}
                      required
                      disabled={loading}
                    >
                      <option value="">{t('floorball.matches.placeholders.selectGroup', '-- Select a group --')}</option>
                      {tournamentGroups.map((g) => (
                        <option key={g.id} value={g.id}>
                          {g.name} ({g.teams.length} {t('floorball.tournaments.teams', 'teams')})
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              )}

              {selectedCompetitionId && !loadingCompetitionDetails && !isTournament && (
                <div className="cm-field">
                  <label htmlFor="cm-division">{subdivisionLabel} *</label>
                  {seasonDivisions.length === 0 ? (
                    <div className="cm-field__info cm-field__info--warning">
                      <i className="fas fa-exclamation-triangle"></i>
                      {t(
                        'floorball.matches.noDivisionsInSeason',
                        'This season has no divisions. Please add divisions to the season first.'
                      )}
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
                      {seasonDivisions.map((sd) => (
                        <option key={sd.divisionId} value={sd.divisionId}>
                          {getDivisionName(sd.divisionId)} ({sd.teamCount} {t('floorball.seasons.teams', 'teams')})
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              )}
            </div>

            {/* Teams Selection — only show when division/group is resolved */}
            {subdivisionResolved && (
              <div className="cm-section">
                <h3 className="cm-section__title">
                  <i className="fas fa-users"></i>
                  {t('floorball.matches.sections.teams', 'Teams')}
                </h3>

                {teamsAvailable.length < 2 ? (
                  <div className="cm-field__info cm-field__info--warning">
                    <i className="fas fa-exclamation-triangle"></i>
                    {isTournament
                      ? t(
                          'floorball.matches.notEnoughTeamsInGroup',
                          'This group needs at least 2 teams. Add teams to the group from the tournament edit page first.'
                        )
                      : t(
                          'floorball.matches.notEnoughTeams',
                          'This division needs at least 2 teams. Please add teams to the season division first.'
                        )}
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
                        onChange={(e) => {
                          setHomeTeamId(e.target.value);
                          if (e.target.value === awayTeamId) setAwayTeamId('');
                        }}
                        required
                        disabled={loading}
                      >
                        <option value="">{t('floorball.matches.placeholders.selectHomeTeam', '-- Select home team --')}</option>
                        {teamsAvailable.map((team) => (
                          <option key={team.id} value={team.id}>{team.name}</option>
                        ))}
                      </select>
                    </div>

                    <div className="cm-field-row__vs" aria-hidden="true">
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
                        {availableAwayTeams.map((team) => (
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
                  {referees.map((ref) => (
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
                onClick={() => navigate(listPath)}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                type="submit"
                className="btn btn-primary"
                disabled={
                  loading ||
                  !selectedCompetitionId ||
                  !subdivisionResolved ||
                  !homeTeamId ||
                  !awayTeamId
                }
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
