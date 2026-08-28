import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballSeasonService, type FootballSeasonDto } from '../../../../api/football/footballSeasonService';
import { footballMatchService } from '../../../../api/football/footballMatchService';
import { footballTournamentService } from '../../../../api/football/footballTournamentService';
import type {
  FootballTournamentDto,
  FootballTournamentGroupDto,
} from '../../../../types/football/tournamentTypes';
import { useDivisions } from '../../../../hooks/useDivisions';
import { SportsCategory } from '../../../../types/common/sports';
import type { CreateFootballMatchRequest, FootballTeam } from '../../../../types/football/footballTypes';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './CreateMatchPage.scss';

export type CreateMatchMode = 'season' | 'tournament';

interface CreateMatchPageProps {
  /**
   * Determines what kind of competition the match belongs to.
   * - `season`     : Pick a Season → Division → Teams in division.
   * - `tournament` : Pick a Tournament → Group → Teams in group.
   * Defaults to `season` for backwards compatibility with the legacy
   * `/admin/football/matches/create` route.
   */
  mode?: CreateMatchMode;
}

interface TeamOption {
  id: string;
  name: string;
}

const safeReturnTo = (raw: string | null): string | null => {
  if (!raw) return null;

  let decoded: string;

  try {
    decoded = decodeURIComponent(raw);
  } catch {
    return null;
  }

  if (!decoded.startsWith('/admin/')) return null;

  return decoded;
};

const CreateMatchPage = ({ mode = 'season' }: CreateMatchPageProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { divisions } = useDivisions();
  const footballDivisions = useMemo(
    () => divisions.filter((division) => division.sportType === SportsCategory.Football),
    [divisions]
  );

  const isTournament = mode === 'tournament';

  const lockedCompetitionIdFromUrl: string = searchParams.get('competitionId') ?? '';

  const returnToTarget: string | null = useMemo(
    () => safeReturnTo(searchParams.get('returnTo')),
    [searchParams]
  );

  const isCompetitionLocked: boolean = lockedCompetitionIdFromUrl.length > 0;

  // ── Data loading: competitions ───────────────────────────────────────
  const [seasons, setSeasons] = useState<FootballSeasonDto[]>([]);
  const [tournaments, setTournaments] = useState<FootballTournamentDto[]>([]);
  const [loadingCompetitions, setLoadingCompetitions] = useState(true);

  const [selectedSeason, setSelectedSeason] = useState<FootballSeasonDto | null>(null);
  const [selectedTournament, setSelectedTournament] = useState<FootballTournamentDto | null>(null);
  const [loadingCompetitionDetails, setLoadingCompetitionDetails] = useState(false);

  // ── Form state ───────────────────────────────────────────────────────
  const [selectedCompetitionId, setSelectedCompetitionId] = useState<string>(lockedCompetitionIdFromUrl);
  const [selectedDivisionId, setSelectedDivisionId] = useState('');
  const [selectedGroupId, setSelectedGroupId] = useState('');
  const [homeTeamId, setHomeTeamId] = useState('');
  const [awayTeamId, setAwayTeamId] = useState('');
  const [selectedDate, setSelectedDate] = useState('');
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [venue, setVenue] = useState('');

  // ── UI state ─────────────────────────────────────────────────────────
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // ── Load competitions: seasons or tournaments depending on mode ──────
  useEffect(() => {
    const load = async () => {
      try {
        setLoadingCompetitions(true);

        if (isTournament) {
          const response = await footballTournamentService.getAll();

          if (response.success && response.data) {
            setTournaments(response.data);
          }
        } else {
          const response = await footballSeasonService.getAll();

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
          const response = await footballTournamentService.getById(selectedCompetitionId);

          if (!cancelled && response.success && response.data) {
            setSelectedTournament(response.data);
          }
        } else {
          const response = await footballSeasonService.getById(selectedCompetitionId);

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

  const tournamentGroups = useMemo<FootballTournamentGroupDto[]>(() => {
    if (!isTournament) return [];
    return selectedTournament?.groups ?? [];
  }, [isTournament, selectedTournament]);

  // Auto-select the only available division/group.
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

      return group.teams.map((team) => ({
        id: team.teamId,
        name: team.teamName,
      }));
    }

    if (!selectedSeason?.teams || !selectedDivisionId) return [];

    const seasonDivision = selectedSeason.seasonDivisions?.find(
      (division) => division.divisionId === selectedDivisionId
    );

    if (!seasonDivision?.teamIds) return [];

    const teamIdSet = new Set(seasonDivision.teamIds);

    return selectedSeason.teams
      .filter((team: FootballTeam) => teamIdSet.has(team.id))
      .map((team: FootballTeam) => ({
        id: team.id,
        name: team.name,
      }));
  }, [isTournament, tournamentGroups, selectedGroupId, selectedSeason, selectedDivisionId]);

  const availableAwayTeams = useMemo(
    () => teamsAvailable.filter((team) => team.id !== homeTeamId),
    [teamsAvailable, homeTeamId]
  );

  // ── Helpers ──────────────────────────────────────────────────────────
  const getDivisionName = useCallback(
    (divisionId: string): string => footballDivisions.find((division) => division.id === divisionId)?.name ?? divisionId,
    [footballDivisions]
  );

  const handleHoursChange = (value: string) => {
    const parsedValue = parseInt(value, 10);

    if (value === '' || (parsedValue >= 0 && parsedValue <= 23 && value.length <= 2)) {
      setHoursInput(value);
    }
  };

  const handleMinutesChange = (value: string) => {
    const parsedValue = parseInt(value, 10);

    if (value === '' || (parsedValue >= 0 && parsedValue <= 59 && value.length <= 2)) {
      setMinutesInput(value);
    }
  };

  const buildScheduledDateTime = (): string => {
    if (!selectedDate || !hoursInput || !minutesInput) return '';

    const date = new Date(selectedDate);
    date.setHours(parseInt(hoursInput, 10), parseInt(minutesInput, 10), 0, 0);

    return date.toISOString();
  };

  // ── Navigation targets ───────────────────────────────────────────────
  const listPath = isTournament
    ? '/admin/football/tournaments/matches'
    : '/admin/football/seasons/matches';

  // ── Submit ───────────────────────────────────────────────────────────
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!selectedCompetitionId) {
      setError(
        isTournament
          ? t('football.matches.validation.tournamentRequired', 'Please select a tournament.')
          : t('football.matches.validation.seasonRequired', 'Please select a season.')
      );
      return;
    }

    if (isTournament) {
      if (!selectedGroupId) {
        setError(t('football.matches.validation.groupRequired', 'Please select a group.'));
        return;
      }
    } else if (!selectedDivisionId) {
      setError(t('football.matches.validation.divisionRequired', 'Please select a division.'));
      return;
    }

    if (!homeTeamId) {
      setError(t('football.matches.validation.homeTeamRequired', 'Please select a home team.'));
      return;
    }

    if (!awayTeamId) {
      setError(t('football.matches.validation.awayTeamRequired', 'Please select an away team.'));
      return;
    }

    if (homeTeamId === awayTeamId) {
      setError(t('football.matches.validation.sameTeam', 'Home and away team cannot be the same.'));
      return;
    }

    if (!selectedDate || !hoursInput || !minutesInput) {
      setError(t('football.matches.validation.dateTimeRequired', 'Please enter a valid date and time.'));
      return;
    }

    const scheduledDateTime = buildScheduledDateTime();

    if (!scheduledDateTime) {
      setError(t('football.matches.validation.dateTimeRequired', 'Please enter a valid date and time.'));
      return;
    }

    setLoading(true);

    try {
      const matchData: CreateFootballMatchRequest = {
        competitionId: selectedCompetitionId,
        homeTeamId,
        awayTeamId,
        scheduledDateTime,
        venue: venue || undefined,
        ...(isTournament && {
          tournamentGroupId: selectedGroupId,
          tournamentStage: 'GroupStage',
        }),
      };

      const response = await footballMatchService.create(matchData);

      if (response.success) {
        setSuccessMessage(t('football.matches.created', 'Match created successfully!'));

        const successPath: string = returnToTarget ?? listPath;

        setTimeout(() => navigate(successPath), 1500);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to create match';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  // ── Render values ────────────────────────────────────────────────────
  const pageTitle = isTournament
    ? t('football.matches.create.tournamentTitle', 'Create Tournament Match')
    : t('football.matches.create.seasonTitle', 'Create Season Match');

  const sectionTitle = isTournament
    ? t('football.matches.sections.tournamentGroup', 'Tournament & Group')
    : t('football.matches.sections.seasonDivision', 'Season & Division');

  const competitionLabel = isTournament
    ? t('football.matches.fields.tournament', 'Tournament')
    : t('football.matches.fields.season', 'Season');

  const competitionPlaceholder = loadingCompetitions
    ? t('common.loading', 'Loading...')
    : isTournament
      ? t('football.matches.placeholders.selectTournament', '-- Select a tournament --')
      : t('football.matches.placeholders.selectSeason', '-- Select a season --');

  const subdivisionLabel = isTournament
    ? t('football.matches.fields.group', 'Group')
    : t('football.matches.fields.division', 'Division');

  const competitions = isTournament ? tournaments : seasons;

  const hasGroupsOrDivisions = isTournament
    ? tournamentGroups.length > 0
    : seasonDivisions.length > 0;

  const subdivisionResolved = isTournament
    ? !!selectedGroupId
    : !!selectedDivisionId;

  return (
    <PageTemplate title={pageTitle}>
      {successMessage && (
        <div className="cm-success-toast">
          <p>{successMessage}</p>
        </div>
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
                  disabled={loading || loadingCompetitions || isCompetitionLocked}
                  title={
                    isCompetitionLocked
                      ? t(
                          'football.matches.create.lockedCompetitionTooltip',
                          'Tournament is locked from context'
                        )
                      : undefined
                  }
                >
                  <option value="">{competitionPlaceholder}</option>

                  {competitions.map((competition) => (
                    <option key={competition.id} value={competition.id}>
                      {isTournament
                        ? competition.name
                        : `${(competition as FootballSeasonDto).name}` +
                          ((competition as FootballSeasonDto).isActive ? ' (Active)' : '') +
                          ((competition as FootballSeasonDto).isCompleted ? ' (Completed)' : '')}
                    </option>
                  ))}
                </select>
              </div>

              {/* Division / Group: only show when a competition is selected */}
              {selectedCompetitionId && loadingCompetitionDetails && (
                <div className="cm-field">
                  <label>{subdivisionLabel} *</label>

                  <div className="cm-field__info">
                    <i className="fas fa-spinner fa-spin"></i>
                    {isTournament
                      ? t('football.matches.loadingTournamentDetails', 'Loading tournament details...')
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
                        'football.matches.noGroupsInTournament',
                        'This tournament has no groups. Add groups in the tournament edit page first.'
                      )}
                    </div>
                  ) : tournamentGroups.length === 1 ? (
                    <div className="cm-field__auto-filled">
                      <i className="fas fa-check-circle"></i>
                      <span>{tournamentGroups[0].name}</span>
                      <span className="cm-field__auto-tag">
                        {t('common.autoSelected', 'Auto-selected')}
                      </span>
                    </div>
                  ) : (
                    <select
                      id="cm-group"
                      value={selectedGroupId}
                      onChange={(e) => handleGroupChange(e.target.value)}
                      required
                      disabled={loading}
                    >
                      <option value="">
                        {t('football.matches.placeholders.selectGroup', '-- Select a group --')}
                      </option>

                      {tournamentGroups.map((group) => (
                        <option key={group.id} value={group.id}>
                          {group.name} ({group.teams.length} {t('football.tournaments.teams', 'teams')})
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
                        'football.matches.noDivisionsInSeason',
                        'This season has no divisions. Please add divisions to the season first.'
                      )}
                    </div>
                  ) : seasonDivisions.length === 1 ? (
                    <div className="cm-field__auto-filled">
                      <i className="fas fa-check-circle"></i>
                      <span>{getDivisionName(seasonDivisions[0].divisionId)}</span>
                      <span className="cm-field__auto-tag">
                        {t('common.autoSelected', 'Auto-selected')}
                      </span>
                    </div>
                  ) : (
                    <select
                      id="cm-division"
                      value={selectedDivisionId}
                      onChange={(e) => handleDivisionChange(e.target.value)}
                      required
                      disabled={loading}
                    >
                      <option value="">
                        {t('football.matches.placeholders.selectDivision', '-- Select a division --')}
                      </option>

                      {seasonDivisions.map((seasonDivision) => (
                        <option key={seasonDivision.divisionId} value={seasonDivision.divisionId}>
                          {getDivisionName(seasonDivision.divisionId)} ({seasonDivision.teamCount}{' '}
                          {t('football.seasons.teams', 'teams')})
                        </option>
                      ))}
                    </select>
                  )}
                </div>
              )}
            </div>

            {/* Teams Selection: only show when division/group is resolved */}
            {subdivisionResolved && (
              <div className="cm-section">
                <h3 className="cm-section__title">
                  <i className="fas fa-users"></i>
                  {t('football.matches.sections.teams', 'Teams')}
                </h3>

                {teamsAvailable.length < 2 ? (
                  <div className="cm-field__info cm-field__info--warning">
                    <i className="fas fa-exclamation-triangle"></i>
                    {isTournament
                      ? t(
                          'football.matches.notEnoughTeamsInGroup',
                          'This group needs at least 2 teams. Add teams to the group from the tournament edit page first.'
                        )
                      : t(
                          'football.matches.notEnoughTeams',
                          'This division needs at least 2 teams. Please add teams to the season division first.'
                        )}
                  </div>
                ) : (
                  <div className="cm-field-row">
                    <div className="cm-field">
                      <label htmlFor="cm-home-team">
                        {t('football.matches.fields.homeTeam', 'Home Team')} *
                      </label>

                      <select
                        id="cm-home-team"
                        value={homeTeamId}
                        onChange={(e) => {
                          setHomeTeamId(e.target.value);

                          if (e.target.value === awayTeamId) {
                            setAwayTeamId('');
                          }
                        }}
                        required
                        disabled={loading}
                      >
                        <option value="">
                          {t('football.matches.placeholders.selectHomeTeam', '-- Select home team --')}
                        </option>

                        {teamsAvailable.map((team) => (
                          <option key={team.id} value={team.id}>
                            {team.name}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="cm-field-row__vs" aria-hidden="true">
                      <span>VS</span>
                    </div>

                    <div className="cm-field">
                      <label htmlFor="cm-away-team">
                        {t('football.matches.fields.awayTeam', 'Away Team')} *
                      </label>

                      <select
                        id="cm-away-team"
                        value={awayTeamId}
                        onChange={(e) => setAwayTeamId(e.target.value)}
                        required
                        disabled={loading || !homeTeamId}
                      >
                        <option value="">
                          {t('football.matches.placeholders.selectAwayTeam', '-- Select away team --')}
                        </option>

                        {availableAwayTeams.map((team) => (
                          <option key={team.id} value={team.id}>
                            {team.name}
                          </option>
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
                {t('football.matches.sections.schedule', 'Schedule & Venue')}
              </h3>

              <div className="cm-field">
                <label>
                  {t('football.matches.fields.dateTime', 'Date & Time')} *
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
                  {t('football.matches.fields.venue', 'Venue')}
                  <span className="cm-optional">
                    {t('common.optional', '(optional)')}
                  </span>
                </label>

                <input
                  type="text"
                  id="cm-venue"
                  value={venue}
                  onChange={(e) => setVenue(e.target.value)}
                  placeholder={t('football.matches.placeholders.venue', 'Enter venue name')}
                  disabled={loading}
                />
              </div>
            </div>

            {/* Actions */}
            <div className="cm-actions">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => navigate(returnToTarget ?? listPath)}
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
                  <>
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.creating', 'Creating...')}
                  </>
                ) : (
                  <>
                    <i className="fas fa-plus"></i>
                    {t('football.matches.create.submit', 'Create Match')}
                  </>
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