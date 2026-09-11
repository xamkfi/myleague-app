import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import type {
  CreateFloorballMatchRequest,
  FloorballMatchDto,
} from '../../../../../types/floorball/floorballTypes';
import SearchableInfiniteDropdown from '../../../../../components/SearchableInfiniteDropdown/SearchableInfiniteDropdown';
import {
  floorballSeasonSearchService,
  floorballTournamentSearchService,
} from '../../../../../api/floorball/floorballTeamSearchService';
import { floorballTeamNameSearchService } from '../../../../../api/floorball/floorballTeamNameSearchService';
import ConfirmationDialog from '../../ManageMatchPage/components/ConfirmationDialog';
import './MatchForm.scss';
import ErrorPopup from '../../../../../components/ErrorPopup/ErrorPopup';

type MatchFormMode = 'create' | 'edit';

/**
 * Competition kind the form is editing/creating against. When omitted, the form falls back to
 * 'season' for backwards compatibility (the original behaviour). For tournament matches the
 * competition dropdown is wired to the tournament search service and labels/copy switches to
 * "Tournament" / "Turnaus".
 */
type MatchFormCompetitionKind = 'season' | 'tournament';

interface MatchFormProps {
  mode: MatchFormMode;
  initialData?: FloorballMatchDto;
  onSubmit: (matchData: CreateFloorballMatchRequest) => Promise<void>;
  onCancel: () => void;
  onCancelMatch?: (matchId: string) => Promise<void>;
  onReactivateMatch?: (matchId: string) => Promise<void>;
  loading?: boolean;
  /**
   * Optional override for the competition kind. When unset, the form auto-detects from
   * `initialData` (tournament if `tournamentGroupId` or `tournamentStage` is set, else season).
   */
  competitionKind?: MatchFormCompetitionKind;
}

const MatchForm = ({
  mode,
  initialData,
  onSubmit,
  onCancel,
  onCancelMatch,
  onReactivateMatch,
  loading = false,
  competitionKind,
}: MatchFormProps) => {
  const { t } = useTranslation();

  // Auto-detect tournament matches: if the match has a tournament group or a non-empty stage label,
  // treat it as a tournament match. Caller can override via the `competitionKind` prop.
  const isTournamentMatch: boolean = competitionKind
    ? competitionKind === 'tournament'
    : Boolean(
        initialData?.tournamentGroupId ||
          (initialData?.tournamentStage && initialData.tournamentStage !== 'None')
      );

  const competitionLabel: string = isTournamentMatch
    ? t('floorball.matches.matchForm.tournament', 'Turnaus')
    : t('floorball.matches.matchForm.season', 'Kausi');

  const competitionPlaceholder: string = isTournamentMatch
    ? t('floorball.matches.matchForm.selectTournament', 'Valitse turnaus')
    : t('floorball.matches.matchForm.selectSeason', 'Valitse kausi');

  const competitionSearchPlaceholder: string = isTournamentMatch
    ? t('floorball.matches.matchForm.searchTournaments', 'Hae turnauksia...')
    : t('floorball.matches.matchForm.searchSeasons', 'Hae kausia...');

  const competitionEmptyMessage: string = isTournamentMatch
    ? t('floorball.matches.matchForm.noTournaments', 'Turnauksia ei löytynyt')
    : t('floorball.matches.matchForm.noSeasons', 'Kausia ei löytynyt');

  const [formData, setFormData] = useState<CreateFloorballMatchRequest>({
    competitionId: undefined,
    homeTeamId: undefined,
    awayTeamId: undefined,
    refereeId: undefined,
    scheduledDateTime: '',
    venue: '',
  });

  const [selectedDate, setSelectedDate] = useState('');
  const [hoursInput, setHoursInput] = useState('');
  const [minutesInput, setMinutesInput] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [cancelLoading, setCancelLoading] = useState(false);
  const [reactivateLoading, setReactivateLoading] = useState(false);
  const [dateError, setDateError] = useState<string | null>(null);
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);
  const [showReactivateConfirm, setShowReactivateConfirm] = useState(false);

  const [initialSeasonOptions, setInitialSeasonOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [initialHomeTeamOptions, setInitialHomeTeamOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [initialAwayTeamOptions, setInitialAwayTeamOptions] = useState<Array<{ id: string; name: string }>>([]);

  const createInitialOptions = useCallback(() => {
    if (mode === 'edit' && initialData) {
      // Use the denormalized competition name returned by the API; falls back to the id
      // when name is missing (legacy responses) so the dropdown still has something to render.
      const competitionOption = {
        id: initialData.competitionId,
        name: initialData.competitionName?.trim() || initialData.competitionId,
      };
      setInitialSeasonOptions([competitionOption]);

      // Only seed the team dropdowns with the existing selection when the match actually has a
      // team assigned. For placeholder fixtures (homeTeamId/awayTeamId === null) the dropdowns
      // start empty so the user can pick a team for the first time.
      if (initialData.homeTeamId && initialData.homeTeamName) {
        setInitialHomeTeamOptions([{
          id: initialData.homeTeamId,
          name: initialData.homeTeamName,
        }]);
      } else {
        setInitialHomeTeamOptions([]);
      }

      if (initialData.awayTeamId && initialData.awayTeamName) {
        setInitialAwayTeamOptions([{
          id: initialData.awayTeamId,
          name: initialData.awayTeamName,
        }]);
      } else {
        setInitialAwayTeamOptions([]);
      }
    } else {
      setInitialSeasonOptions([]);
      setInitialHomeTeamOptions([]);
      setInitialAwayTeamOptions([]);
    }
  }, [mode, initialData]);

  const preloadInitialOptions = useCallback(async () => {
    if (mode === 'edit' && initialData) {
      try {
        // Switch to tournament search service for tournament matches so the dropdown actually
        // resolves the competition; otherwise the season service would return an empty list
        // (no season with that id exists) and the user would not be able to pick anything.
        const competitionResult = isTournamentMatch
          ? await floorballTournamentSearchService.searchTournaments('', 1)
          : await floorballSeasonSearchService.searchSeasons('', 1);

        const matchingCompetition = competitionResult.data.find(
          (competition) => competition.id === initialData.competitionId
        );

        if (matchingCompetition) {
          setInitialSeasonOptions([matchingCompetition]);
        }

        const homeTeamResult = await floorballTeamNameSearchService.searchTeams('', 1);
        const matchingHomeTeam = homeTeamResult.data.find((team) => team.id === initialData.homeTeamId);

        if (matchingHomeTeam) {
          setInitialHomeTeamOptions([matchingHomeTeam]);
        }

        const awayTeamResult = await floorballTeamNameSearchService.searchTeams('', 1);
        const matchingAwayTeam = awayTeamResult.data.find((team) => team.id === initialData.awayTeamId);

        if (matchingAwayTeam) {
          setInitialAwayTeamOptions([matchingAwayTeam]);
        }
      } catch (error) {
        console.error('Error pre-loading initial options:', error);
      }
    }
  }, [mode, initialData, isTournamentMatch]);

  useEffect(() => {
    if (mode === 'edit' && initialData) {
      const matchDate = new Date(initialData.scheduledDateTime);
      const hoursStr = matchDate.getHours().toString().padStart(2, '0');
      const minutesStr = matchDate.getMinutes().toString().padStart(2, '0');
      const year = matchDate.getFullYear();
      const month = (matchDate.getMonth() + 1).toString().padStart(2, '0');
      const day = matchDate.getDate().toString().padStart(2, '0');
      const dateStr = `${year}-${month}-${day}`;

      setFormData({
        competitionId: initialData.competitionId,
        // Convert nulls (placeholder slots) to undefined so the dropdown stays unselected.
        homeTeamId: initialData.homeTeamId ?? undefined,
        awayTeamId: initialData.awayTeamId ?? undefined,
        refereeId: undefined,
        scheduledDateTime: initialData.scheduledDateTime,
        venue: initialData.venue || '',
      });

      setSelectedDate(dateStr);
      setHoursInput(hoursStr);
      setMinutesInput(minutesStr);

      createInitialOptions();
      preloadInitialOptions();
    } else {
      setFormData({
        competitionId: undefined,
        homeTeamId: undefined,
        awayTeamId: undefined,
        refereeId: undefined,
        scheduledDateTime: '',
        venue: '',
      });

      setSelectedDate('');
      setHoursInput('');
      setMinutesInput('');
      createInitialOptions();
    }
  }, [mode, initialData, createInitialOptions, preloadInitialOptions]);

  const searchCompetitionsWithInitial = useCallback(
    async (query: string, page: number) => {
      // Pick the right backend depending on the competition kind. Note: the result type is the
      // same `SearchResult`, so the consumer (`SearchableInfiniteDropdown`) doesn't need to know
      // which kind of competition it's listing.
      const result = isTournamentMatch
        ? await floorballTournamentSearchService.searchTournaments(query, page)
        : await floorballSeasonSearchService.searchSeasons(query, page);

      if (mode === 'edit' && page === 1 && initialData) {
        try {
          const matchingCompetition = result.data.find(
            (competition) => competition.id === initialData.competitionId
          );

          if (matchingCompetition) {
            const updatedInitialOptions = [
              {
                id: initialData.competitionId,
                name: matchingCompetition.name,
              },
            ];

            const filteredInitial = updatedInitialOptions.filter((option) =>
              option.name.toLowerCase().includes(query.toLowerCase())
            );

            return {
              data: [
                ...filteredInitial,
                ...result.data.filter(
                  (item) => !updatedInitialOptions.some((initial) => initial.id === item.id)
                ),
              ],
              pagination: result.pagination,
            };
          }
        } catch (error) {
          console.error('Error fetching competition name:', error);
        }
      }

      if (mode === 'edit' && page === 1 && initialSeasonOptions.length > 0) {
        const filteredInitial = initialSeasonOptions.filter((option) =>
          option.name.toLowerCase().includes(query.toLowerCase())
        );

        return {
          data: [
            ...filteredInitial,
            ...result.data.filter(
              (item) => !initialSeasonOptions.some((initial) => initial.id === item.id)
            ),
          ],
          pagination: result.pagination,
        };
      }

      return result;
    },
    [mode, initialData, initialSeasonOptions, isTournamentMatch]
  );

  const searchHomeTeamsWithInitial = useCallback(
    async (query: string, page: number) => {
      const result = await floorballTeamNameSearchService.searchTeams(query, page);

      if (mode === 'edit' && page === 1 && initialHomeTeamOptions.length > 0) {
        const filteredInitial = initialHomeTeamOptions.filter((option) =>
          option.name.toLowerCase().includes(query.toLowerCase())
        );

        return {
          data: [
            ...filteredInitial,
            ...result.data.filter(
              (item) => !initialHomeTeamOptions.some((initial) => initial.id === item.id)
            ),
          ],
          pagination: result.pagination,
        };
      }

      return result;
    },
    [mode, initialHomeTeamOptions]
  );

  const searchAwayTeamsWithInitial = useCallback(
    async (query: string, page: number) => {
      const result = await floorballTeamNameSearchService.searchTeams(query, page);

      if (mode === 'edit' && page === 1 && initialAwayTeamOptions.length > 0) {
        const filteredInitial = initialAwayTeamOptions.filter((option) =>
          option.name.toLowerCase().includes(query.toLowerCase())
        );

        return {
          data: [
            ...filteredInitial,
            ...result.data.filter(
              (item) => !initialAwayTeamOptions.some((initial) => initial.id === item.id)
            ),
          ],
          pagination: result.pagination,
        };
      }

      return result;
    },
    [mode, initialAwayTeamOptions]
  );

  const updateScheduledDateTime = (dateStr: string, hours: string, minutes: string) => {
    if (dateStr && hours && minutes) {
      const date = new Date(dateStr);
      date.setHours(parseInt(hours), parseInt(minutes), 0, 0);
      const isoDateTime = date.toISOString();

      setFormData((prev) => ({
        ...prev,
        scheduledDateTime: isoDateTime,
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        scheduledDateTime: '',
      }));
    }
  };

  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSelectedDate(value);
    updateScheduledDateTime(value, hoursInput, minutesInput);
    setDateError(null);
  };

  const handleHoursChange = (value: string) => {
    const numValue = parseInt(value);

    if (value === '' || (numValue >= 0 && numValue <= 23 && value.length <= 2)) {
      setHoursInput(value);
      updateScheduledDateTime(selectedDate, value, minutesInput);
    }
  };

  const handleMinutesChange = (value: string) => {
    const numValue = parseInt(value);

    if (value === '' || (numValue >= 0 && numValue <= 59 && value.length <= 2)) {
      setMinutesInput(value);
      updateScheduledDateTime(selectedDate, hoursInput, value);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setError(null);

      const matchData: CreateFloorballMatchRequest = {
        ...formData,
        refereeId: undefined,
      };

      await onSubmit(matchData);

      if (mode === 'create') {
        setFormData({
          competitionId: undefined,
          homeTeamId: undefined,
          awayTeamId: undefined,
          refereeId: undefined,
          scheduledDateTime: '',
          venue: '',
        });

        setSelectedDate('');
        setHoursInput('');
        setMinutesInput('');
        createInitialOptions();
      }
    } catch (error) {
      console.error(`Error ${mode === 'create' ? 'creating' : 'updating'} match:`, error);
      setError(error instanceof Error ? error.message : `Failed to ${mode} match`);
    }
  };

  const handleCancel = () => {
    setFormData({
      competitionId: '',
      homeTeamId: '',
      awayTeamId: '',
      refereeId: undefined,
      scheduledDateTime: '',
      venue: '',
    });

    setSelectedDate('');
    setHoursInput('');
    setMinutesInput('');
    setError(null);
    createInitialOptions();
    onCancel();
  };

  const handleCancelMatchConfirm = async () => {
    if (!initialData || !onCancelMatch) return;

    try {
      setCancelLoading(true);
      setError(null);
      await onCancelMatch(initialData.id);
      setShowCancelConfirm(false);
      handleCancel();
    } catch (error) {
      console.error('Error canceling match:', error);
      setError(error instanceof Error ? error.message : 'Failed to cancel match');
    } finally {
      setCancelLoading(false);
    }
  };

  const handleReactivateMatchConfirm = async () => {
    if (!initialData || !onReactivateMatch) return;

    try {
      setReactivateLoading(true);
      setError(null);
      await onReactivateMatch(initialData.id);
      setShowReactivateConfirm(false);
      handleCancel();
    } catch (error) {
      console.error('Error reactivating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to reactivate match');
    } finally {
      setReactivateLoading(false);
    }
  };

  return (
    <>
      {error && <ErrorPopup message={error} />}

      <form onSubmit={handleSubmit} className="modal-form">
        <div className="form-group create-match-form-row">
          <label htmlFor="competition">{competitionLabel} *</label>
          <div className="input-wrapper">
            <SearchableInfiniteDropdown
              placeholder={competitionPlaceholder}
              value={formData.competitionId}
              onChange={(value) =>
                setFormData((prev) => ({
                  ...prev,
                  competitionId: value,
                }))
              }
              onSearch={searchCompetitionsWithInitial}
              searchPlaceholder={competitionSearchPlaceholder}
              emptyMessage={competitionEmptyMessage}
              required
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>

        {/*
          Teams are optional at create-time: an admin may publish the fixture before knowing the
          participants (future league round, playoff slot waiting on a feeder). The "Assign teams"
          flow is used later to fill the slots in. We keep the visual label (no asterisk) and add a
          small helper sentence so users understand the new behavior.
        */}
        <div className="form-group create-match-form-row">
          <label htmlFor="homeTeam">{t('floorball.matches.homeTeamLabel', 'Home Team')}</label>
          <div className="input-wrapper">
            <SearchableInfiniteDropdown
              placeholder={t('floorball.matches.homeTeamPlaceholder', 'Select Home Team (optional)')}
              value={formData.homeTeamId}
              onChange={(value) =>
                setFormData((prev) => ({
                  ...prev,
                  homeTeamId: value,
                }))
              }
              onSearch={searchHomeTeamsWithInitial}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>

        <div className="form-group create-match-form-row">
          <label htmlFor="awayTeam">{t('floorball.matches.awayTeamLabel', 'Away Team')}</label>
          <div className="input-wrapper">
            <SearchableInfiniteDropdown
              placeholder={t('floorball.matches.awayTeamPlaceholder', 'Select Away Team (optional)')}
              value={formData.awayTeamId}
              onChange={(value) =>
                setFormData((prev) => ({
                  ...prev,
                  awayTeamId: value,
                }))
              }
              onSearch={searchAwayTeamsWithInitial}
              searchPlaceholder="Search teams..."
              emptyMessage="No teams found"
              loadInitialDataOnMount={mode === 'edit'}
            />
          </div>
        </div>

        <div className="form-help-text">
          {t(
            'floorball.matches.teamsOptionalHint',
            'Voit luoda ottelun ilman joukkueita ja asettaa ne myöhemmin "Aseta joukkueet" -toiminnolla.'
          )}
        </div>

        <div className="form-group create-match-form-row">
          <label>Date & Time *</label>
          <div className="input-wrapper">
            <div className="datetime-input-group">
              <div className="date-input">
                <input
                  type="date"
                  value={selectedDate}
                  onChange={handleDateChange}
                  required
                />
              </div>

              {dateError && (
                <div className="field-error" role="alert">
                  {dateError}
                </div>
              )}

              <div className="time-input-group">
                <input
                  type="number"
                  placeholder="HH"
                  value={hoursInput}
                  onChange={(e) => handleHoursChange(e.target.value)}
                  min="0"
                  max="23"
                  className="time-input hours"
                  required
                />

                <span className="time-separator">:</span>

                <input
                  type="number"
                  placeholder="MM"
                  value={minutesInput}
                  onChange={(e) => handleMinutesChange(e.target.value)}
                  min="0"
                  max="59"
                  className="time-input minutes"
                  required
                />
              </div>
            </div>
          </div>
        </div>

        <div className="form-group create-match-form-row">
          <label htmlFor="venue">Venue</label>
          <div className="input-wrapper">
            <input
              type="text"
              id="venue"
              value={formData.venue}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  venue: e.target.value,
                }))
              }
              placeholder="Enter venue"
            />
          </div>
        </div>

        <div className="form-actions">
          <button type="button" onClick={handleCancel} className="cancel-button">
            {t('common.cancel', 'Cancel')}
          </button>

          {mode === 'edit' &&
            initialData &&
            initialData.status === 'Cancelled' &&
            onReactivateMatch && (
              <button
                type="button"
                onClick={() => setShowReactivateConfirm(true)}
                disabled={reactivateLoading}
                className="reactivate-match-button"
              >
                {reactivateLoading
                  ? t('floorball.matches.reactivating', 'Reactivating...')
                  : t('floorball.matches.actions.reactivate', 'Reactivate Match')}
              </button>
            )}

          {mode === 'edit' &&
            initialData &&
            onCancelMatch &&
            initialData.status !== 'Cancelled' &&
            initialData.status !== 'Completed' && (
              <button
                type="button"
                onClick={() => setShowCancelConfirm(true)}
                disabled={cancelLoading}
                className="cancel-match-button"
              >
                {cancelLoading
                  ? t('floorball.matches.cancelling', 'Cancelling...')
                  : t('floorball.matches.actions.cancel', 'Cancel Match')}
              </button>
            )}

          <button type="submit" disabled={loading} className="submit-button">
            {loading
              ? mode === 'create'
                ? t('floorball.matches.creating', 'Creating...')
                : t('floorball.matches.updating', 'Updating...')
              : mode === 'create'
                ? t('floorball.matches.createMatch', 'Create Match')
                : t('floorball.matches.updateMatch', 'Update Match')}
          </button>
        </div>
      </form>

      <ConfirmationDialog
        isOpen={showCancelConfirm}
        icon="⚠️"
        title={t('floorball.matches.confirmCancel.title', 'Cancel Match')}
        message={t(
          'floorball.matches.confirmCancel.message',
          'Are you sure you want to cancel this match? This will mark the match as cancelled.'
        )}
        confirmText={t('floorball.matches.confirmCancel.confirm', 'Yes, Cancel Match')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={cancelLoading}
        onConfirm={handleCancelMatchConfirm}
        onCancel={() => setShowCancelConfirm(false)}
      />

      <ConfirmationDialog
        isOpen={showReactivateConfirm}
        icon="✅"
        title={t('floorball.matches.confirmReactivate.title', 'Reactivate Match')}
        message={t(
          'floorball.matches.confirmReactivate.message',
          'Are you sure you want to reactivate this match? This will set the match back to Scheduled status.'
        )}
        confirmText={t('floorball.matches.confirmReactivate.confirm', 'Yes, Reactivate Match')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={reactivateLoading}
        onConfirm={handleReactivateMatchConfirm}
        onCancel={() => setShowReactivateConfirm(false)}
      />
    </>
  );
};

export default MatchForm;