import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ChangeEvent, ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import ConfirmationDialog from '../../../../../components/ConfirmationDialog/ConfirmationDialog';
import { formatPersonName } from '../../../../../types/loanGoalkeeper';
import { parseLoanPlayerCount } from '../../../../../types/loanPlayer';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchDto,
  type FootballMatchRules,
  type LineupPlayerRequest,
} from '../../../../../types/football/footballTypes';
import {
  findLineupProblem,
  lineupPositionOrDefault,
  resolveMatchRules,
  type LineupProblem,
} from '../utils/lineupValidation';
import './EditActiveRosterDialog.scss';

interface EditActiveRosterDialogProps {
  isOpen: boolean;
  matchId: string;
  homeTeamId: string;
  awayTeamId: string;
  homeTeamName: string;
  awayTeamName: string;
  homePlayers: FootballPlayerDto[];
  awayPlayers: FootballPlayerDto[];
  initialHomeLineup: FootballLineupPlayer[];
  initialAwayLineup: FootballLineupPlayer[];
  matchRules?: FootballMatchRules;
  competitionId?: string | null;
  /** Players with recorded events in this match; the backend refuses to remove them. */
  lockedPlayerIds: ReadonlySet<string>;
  onPlayerAdded: (teamId: string, player: FootballPlayerDto) => void;
  onClose: () => void;
  onSaved: (updatedMatch: FootballMatchDto) => void;
  onError: (message: string | null) => void;
}

interface PlayerLineupState {
  position: FootballPosition;
  /** Whether the player is in the match lineup. */
  isSelected: boolean;
  isSentOff: boolean;
}

type TeamLineupMap = Map<string, PlayerLineupState>;

const LINEUP_POSITIONS: FootballPosition[] = [
  FootballPosition.Goalkeeper,
  FootballPosition.Defender,
  FootballPosition.Midfielder,
  FootballPosition.Forward,
];

const sortPlayers = (a: FootballPlayerDto, b: FootballPlayerDto): number => {
  const numA: number = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  const numB: number = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) return numA - numB;
  return `${a.person.lastName} ${a.person.firstName}`.localeCompare(
    `${b.person.lastName} ${b.person.firstName}`,
    undefined,
    { sensitivity: 'base' },
  );
};

const buildInitialState = (
  players: FootballPlayerDto[],
  lineup: FootballLineupPlayer[],
): TeamLineupMap => {
  const existing = new Map(lineup.map((entry) => [entry.playerId, entry]));
  const next: TeamLineupMap = new Map();
  for (const player of players) {
    const saved: FootballLineupPlayer | undefined = existing.get(player.id);
    next.set(player.id, {
      position: lineupPositionOrDefault(saved?.position ?? player.position),
      isSelected: saved !== undefined,
      isSentOff: saved?.isSentOff === true,
    });
  }
  return next;
};

const selectedEntries = (state: TeamLineupMap): PlayerLineupState[] =>
  [...state.values()].filter((entry) => entry.isSelected);

interface TeamColumnProps {
  teamLabel: string;
  players: FootballPlayerDto[];
  state: TeamLineupMap;
  rules: FootballMatchRules;
  lockedPlayerIds: ReadonlySet<string>;
  onToggleSelected: (playerId: string) => void;
  onSelectAll: (selected: boolean) => void;
  onChangePosition: (playerId: string, position: FootballPosition) => void;
  onUseLoanGoalkeeper: () => void;
  onUseLoanPlayers: (count: number) => void;
  loanGoalkeeperBusy: boolean;
}

type PendingLoanAction =
  | { kind: 'goalkeeper'; side: 'home' | 'away' }
  | { kind: 'players'; side: 'home' | 'away'; count: number };

const TeamColumn = ({
  teamLabel,
  players,
  state,
  rules,
  lockedPlayerIds,
  onToggleSelected,
  onSelectAll,
  onChangePosition,
  onUseLoanGoalkeeper,
  onUseLoanPlayers,
  loanGoalkeeperBusy,
}: TeamColumnProps): ReactElement => {
  const { t } = useTranslation();
  const [search, setSearch] = useState<string>('');
  const [loanPlayerCount, setLoanPlayerCount] = useState<string>('1');
  const parsedLoanPlayerCount: number | null = parseLoanPlayerCount(loanPlayerCount);

  const sortedPlayers: FootballPlayerDto[] = useMemo(
    () => [...players].sort(sortPlayers),
    [players],
  );

  const filteredPlayers: FootballPlayerDto[] = useMemo(() => {
    const needle = search.trim().toLowerCase();
    if (!needle) return sortedPlayers;
    return sortedPlayers.filter((player) => {
      const haystack = `${player.person.firstName} ${player.person.lastName} #${player.jerseyNumber ?? ''}`.toLowerCase();
      return haystack.includes(needle);
    });
  }, [sortedPlayers, search]);

  const selected: PlayerLineupState[] = selectedEntries(state);
  const problem: LineupProblem | null = findLineupProblem(selected, rules);

  return (
    <div className="eard-column">
      <div className="eard-column__header">
        <h3 className="eard-column__team">{teamLabel}</h3>
        <span className={`eard-column__count${problem ? ' eard-column__count--invalid' : ''}`}>
          {t('football.matches.lineup.playerCount', '{{count}} players', { count: selected.length })}
        </span>
      </div>

      <button
        type="button"
        className="eard-btn eard-btn--ghost eard-btn--sm eard-loan-goalkeeper"
        onClick={onUseLoanGoalkeeper}
        disabled={loanGoalkeeperBusy}
      >
        {loanGoalkeeperBusy
          ? t('common.saving', 'Saving...')
          : t('football.matches.lineup.loanGoalkeeper', 'Lainavahti')}
      </button>
      <p className="eard-loan-hint">
        {t('football.matches.lineup.loanGoalkeeperHint', 'Creates a loan goalkeeper for the team and sets them as the goalkeeper. If one already exists, that same player is selected.')}
      </p>
      <div className="eard-loan-players">
        <label className="eard-loan-players__count">
          <span>{t('football.matches.lineup.loanPlayersCount', 'Count')}</span>
          <input
            type="number"
            min={1}
            max={20}
            value={loanPlayerCount}
            onChange={(event: ChangeEvent<HTMLInputElement>) => setLoanPlayerCount(event.target.value)}
            disabled={loanGoalkeeperBusy}
            aria-label={t('football.matches.lineup.loanPlayersCount', 'Count')}
          />
        </label>
        <button
          type="button"
          className="eard-btn eard-btn--ghost eard-btn--sm"
          onClick={() => {
            if (parsedLoanPlayerCount !== null) {
              onUseLoanPlayers(parsedLoanPlayerCount);
            }
          }}
          disabled={loanGoalkeeperBusy || parsedLoanPlayerCount === null}
        >
          {loanGoalkeeperBusy
            ? t('common.saving', 'Saving...')
            : t('football.matches.lineup.loanPlayers', 'Lainapelaajat')}
        </button>
      </div>
      <p className="eard-loan-hint">
        {t('football.matches.lineup.loanPlayersHint', 'Adds the requested number of loan players to this match. Existing loan players are used first, and only the missing ones are created.')}
      </p>

      {problem && (
        <span className="eard-field__warning">
          <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
          {t(`football.matches.lineup.${problem}`)}
        </span>
      )}

      <div className="eard-select-all">
        <button type="button" className="eard-btn eard-btn--ghost eard-btn--sm" onClick={() => onSelectAll(true)}>
          {t('football.matches.lineup.selectAll', 'Select all')}
        </button>
        <button type="button" className="eard-btn eard-btn--ghost eard-btn--sm" onClick={() => onSelectAll(false)}>
          {t('football.matches.lineup.clearAll', 'Clear')}
        </button>
      </div>

      <div className="eard-filters">
        <div className="eard-filters__search">
          <i className="fas fa-search" aria-hidden="true"></i>
          <input
            type="text"
            placeholder={t('football.matches.lineup.searchPlayers', 'Search players by name...')}
            value={search}
            onChange={(e: ChangeEvent<HTMLInputElement>) => setSearch(e.target.value)}
          />
          {search && (
            <button
              type="button"
              className="eard-filters__clear"
              onClick={() => setSearch('')}
              aria-label={t('common.clearSearch', 'Clear search')}
            >
              <i className="fas fa-times" aria-hidden="true"></i>
            </button>
          )}
        </div>
      </div>

      <div className="eard-table-wrapper">
        {filteredPlayers.length === 0 ? (
          <div className="eard-empty">
            {sortedPlayers.length === 0
              ? t('football.matches.lineup.noTeamPlayers', 'Team has no players.')
              : t('football.matches.lineup.noAvailablePlayers', 'No available players match the current filters.')}
          </div>
        ) : (
          <table className="eard-table">
            <thead>
              <tr>
                <th>{t('football.matches.lineup.inLineup', 'In lineup')}</th>
                <th className="eard-table__jersey">#</th>
                <th>{t('football.matches.lineup.player', 'Player')}</th>
                <th>{t('football.matches.lineup.position', 'Position')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredPlayers.map((player: FootballPlayerDto) => {
                const entry: PlayerLineupState | undefined = state.get(player.id);
                const isSentOff: boolean = entry?.isSentOff === true;
                const isSelected: boolean = entry?.isSelected === true;
                const isLocked: boolean = isSelected && (isSentOff || lockedPlayerIds.has(player.id));
                const playerName: string = formatPersonName(player.person.firstName, player.person.lastName);
                return (
                  <tr key={player.id} className={isSentOff ? 'eard-row--sent-off' : ''}>
                    <td>
                      <input
                        type="checkbox"
                        checked={isSelected}
                        disabled={isLocked}
                        onChange={() => onToggleSelected(player.id)}
                        aria-label={t('football.matches.lineup.selectPlayer', '{{name}} in the lineup', { name: playerName })}
                        title={isLocked ? t('football.matches.lineup.hasEvents', 'Player has recorded events in this match and cannot be removed') : undefined}
                      />
                    </td>
                    <td className="eard-table__jersey">
                      {player.jerseyNumber != null ? `#${player.jerseyNumber}` : '–'}
                    </td>
                    <td>
                      <span className="eard-player-name">
                        {playerName}
                      </span>
                      {isSentOff && (
                        <span className="eard-position-badge">
                          {t('football.matches.lineup.sentOff', 'Sent off')}
                        </span>
                      )}
                    </td>
                    <td>
                      <select
                        className="eard-field__select"
                        value={entry?.position ?? FootballPosition.Midfielder}
                        disabled={isSentOff || !isSelected}
                        onChange={(e: ChangeEvent<HTMLSelectElement>) =>
                          onChangePosition(player.id, e.target.value as FootballPosition)
                        }
                      >
                        {LINEUP_POSITIONS.map((position) => (
                          <option key={position} value={position}>
                            {t(`football.positions.${position}`, position)}
                          </option>
                        ))}
                      </select>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

const EditActiveRosterDialog = ({
  isOpen,
  matchId,
  homeTeamId,
  awayTeamId,
  homeTeamName,
  awayTeamName,
  homePlayers,
  awayPlayers,
  initialHomeLineup,
  initialAwayLineup,
  matchRules,
  competitionId,
  lockedPlayerIds,
  onPlayerAdded,
  onClose,
  onSaved,
  onError,
}: EditActiveRosterDialogProps): ReactElement | null => {
  const { t } = useTranslation();
  const rules = resolveMatchRules(matchRules);
  const [homeState, setHomeState] = useState<TeamLineupMap>(() =>
    buildInitialState(homePlayers, initialHomeLineup),
  );
  const [awayState, setAwayState] = useState<TeamLineupMap>(() =>
    buildInitialState(awayPlayers, initialAwayLineup),
  );
  const [saving, setSaving] = useState<boolean>(false);
  const [loanSide, setLoanSide] = useState<'home' | 'away' | null>(null);
  const [pendingLoan, setPendingLoan] = useState<PendingLoanAction | null>(null);
  const wasOpenRef = useRef(false);

  useEffect(() => {
    if (!isOpen) {
      wasOpenRef.current = false;
      setPendingLoan(null);
      return;
    }
    if (wasOpenRef.current) {
      return;
    }
    wasOpenRef.current = true;
    setHomeState(buildInitialState(homePlayers, initialHomeLineup));
    setAwayState(buildInitialState(awayPlayers, initialAwayLineup));
  }, [isOpen, homePlayers, awayPlayers, initialHomeLineup, initialAwayLineup]);

  const updateTeamState = useCallback(
    (side: 'home' | 'away', updater: (prev: TeamLineupMap) => TeamLineupMap): void => {
      if (side === 'home') {
        setHomeState(updater);
      } else {
        setAwayState(updater);
      }
    },
    [],
  );

  const toggleSelected = useCallback((side: 'home' | 'away', playerId: string): void => {
    updateTeamState(side, (prev) => {
      const current = prev.get(playerId);
      if (!current) return prev;
      return new Map(prev).set(playerId, { ...current, isSelected: !current.isSelected });
    });
  }, [updateTeamState]);

  const selectAll = useCallback((side: 'home' | 'away', selected: boolean): void => {
    updateTeamState(side, (prev) => {
      const next: TeamLineupMap = new Map();
      for (const [playerId, entry] of prev) {
        const keepSelected: boolean = entry.isSelected && (entry.isSentOff || lockedPlayerIds.has(playerId));
        next.set(playerId, { ...entry, isSelected: selected || keepSelected });
      }
      return next;
    });
  }, [updateTeamState, lockedPlayerIds]);

  const changePosition = useCallback((side: 'home' | 'away', playerId: string, position: FootballPosition): void => {
    updateTeamState(side, (prev) => {
      const current = prev.get(playerId);
      if (!current) return prev;
      return new Map(prev).set(playerId, { ...current, position });
    });
  }, [updateTeamState]);

  const placeLoanGoalkeeper = useCallback((side: 'home' | 'away', playerId: string): void => {
    updateTeamState(side, (prev) => new Map(prev).set(playerId, {
      position: FootballPosition.Goalkeeper,
      isSelected: true,
      isSentOff: false,
    }));
  }, [updateTeamState]);

  const addLoanGoalkeeper = useCallback(async (side: 'home' | 'away'): Promise<void> => {
    const teamId = side === 'home' ? homeTeamId : awayTeamId;
    try {
      setLoanSide(side);
      onError(null);
      const result = await footballTeamService.ensureLoanGoalkeeper(teamId, competitionId);
      const player: FootballPlayerDto = {
        id: result.playerId,
        personId: '',
        person: {
          id: '',
          firstName: result.displayName,
          lastName: '',
          birthDate: '',
          fullName: result.displayName,
          isRegistered: false,
        },
        isActive: true,
        position: FootballPosition.Goalkeeper,
        careerGoals: 0,
        careerAssists: 0,
      };
      onPlayerAdded(teamId, player);
      placeLoanGoalkeeper(side, result.playerId);
    } catch (error) {
      const message = error instanceof Error
        ? error.message
        : t('football.matches.lineup.loanGoalkeeperFailed', 'Failed to add loan goalkeeper');
      onError(message);
    } finally {
      setLoanSide(null);
      setPendingLoan(null);
    }
  }, [awayTeamId, competitionId, homeTeamId, onError, onPlayerAdded, placeLoanGoalkeeper, t]);

  const placeLoanPlayers = useCallback((side: 'home' | 'away', playerIds: string[]): void => {
    updateTeamState(side, (prev) => {
      const next = new Map(prev);
      for (const playerId of playerIds) {
        const current = next.get(playerId);
        next.set(playerId, {
          position: current?.position ?? FootballPosition.Midfielder,
          isSelected: true,
          isSentOff: current?.isSentOff ?? false,
        });
      }
      return next;
    });
  }, [updateTeamState]);

  const addLoanPlayers = useCallback(async (side: 'home' | 'away', count: number): Promise<void> => {
    const teamId = side === 'home' ? homeTeamId : awayTeamId;
    try {
      setLoanSide(side);
      onError(null);
      const results = await footballTeamService.ensureLoanPlayers(teamId, count, competitionId);
      for (const result of results) {
        const player: FootballPlayerDto = {
          id: result.playerId,
          personId: '',
          person: {
            id: '',
            firstName: result.firstName,
            lastName: result.lastName,
            birthDate: '',
            fullName: result.displayName,
            isRegistered: false,
          },
          isActive: true,
          position: FootballPosition.Midfielder,
          careerGoals: 0,
          careerAssists: 0,
          jerseyNumber: result.jerseyNumber,
        };
        onPlayerAdded(teamId, player);
      }
      placeLoanPlayers(side, results.map((result) => result.playerId));
    } catch (error) {
      const message = error instanceof Error
        ? error.message
        : t('football.matches.lineup.loanPlayersFailed', 'Failed to add loan players');
      onError(message);
    } finally {
      setLoanSide(null);
      setPendingLoan(null);
    }
  }, [awayTeamId, competitionId, homeTeamId, onError, onPlayerAdded, placeLoanPlayers, t]);

  const canSave: boolean = !saving && loanSide === null;

  // Only the players who are in the match. Everyone in the lineup counts as having played.
  const toPayload = (state: TeamLineupMap): LineupPlayerRequest[] =>
    [...state.entries()]
      .filter(([, entry]) => entry.isSelected)
      .map(([playerId, entry]) => ({
        playerId,
        position: entry.position,
        isOnField: !entry.isSentOff,
      }));

  const handleSave = useCallback(async (): Promise<void> => {
    if (!canSave) return;
    try {
      setSaving(true);
      onError(null);

      const homeResp = await footballMatchService.setLineup(matchId, homeTeamId, toPayload(homeState));
      const awayResp = await footballMatchService.setLineup(matchId, awayTeamId, toPayload(awayState));

      const updatedMatch: FootballMatchDto | undefined = awayResp.data ?? homeResp.data;
      if (updatedMatch) {
        onSaved(updatedMatch);
      }
      onClose();
    } catch (error) {
      const message: string = error instanceof Error
        ? error.message
        : t('football.matches.lineup.saveFailed', 'Failed to save lineup');
      onError(message);
    } finally {
      setSaving(false);
    }
  }, [canSave, homeState, awayState, matchId, homeTeamId, awayTeamId, onClose, onSaved, onError, t]);

  const pendingTeamName: string = pendingLoan?.side === 'home'
    ? homeTeamName
    : pendingLoan?.side === 'away'
      ? awayTeamName
      : '';

  const confirmLoan = (): void => {
    if (pendingLoan === null || loanSide !== null) {
      return;
    }
    if (pendingLoan.kind === 'goalkeeper') {
      void addLoanGoalkeeper(pendingLoan.side);
      return;
    }
    void addLoanPlayers(pendingLoan.side, pendingLoan.count);
  };

  if (!isOpen) return null;

  return (
    <>
    <div className="eard-overlay" onClick={onClose}>
      <div
        className="eard-dialog"
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="eard-title"
      >
        <header className="eard-header">
          <h2 id="eard-title" className="eard-header__title">
            {t('football.matches.lineup.editLineupTitle', 'Edit lineup')}
          </h2>
          <button
            type="button"
            className="eard-header__close"
            onClick={onClose}
            aria-label={t('common.close', 'Close')}
            disabled={saving}
          >
            <i className="fas fa-times" aria-hidden="true"></i>
          </button>
        </header>

        <div className="eard-body">
          <TeamColumn
            teamLabel={homeTeamName}
            players={homePlayers}
            state={homeState}
            rules={rules}
            lockedPlayerIds={lockedPlayerIds}
            onToggleSelected={(id) => toggleSelected('home', id)}
            onSelectAll={(selected) => selectAll('home', selected)}
            onChangePosition={(id, position) => changePosition('home', id, position)}
            onUseLoanGoalkeeper={() => setPendingLoan({ kind: 'goalkeeper', side: 'home' })}
            onUseLoanPlayers={(count) => setPendingLoan({ kind: 'players', side: 'home', count })}
            loanGoalkeeperBusy={loanSide !== null}
          />
          <TeamColumn
            teamLabel={awayTeamName}
            players={awayPlayers}
            state={awayState}
            rules={rules}
            lockedPlayerIds={lockedPlayerIds}
            onToggleSelected={(id) => toggleSelected('away', id)}
            onSelectAll={(selected) => selectAll('away', selected)}
            onChangePosition={(id, position) => changePosition('away', id, position)}
            onUseLoanGoalkeeper={() => setPendingLoan({ kind: 'goalkeeper', side: 'away' })}
            onUseLoanPlayers={(count) => setPendingLoan({ kind: 'players', side: 'away', count })}
            loanGoalkeeperBusy={loanSide !== null}
          />
        </div>

        <footer className="eard-footer">
          <button
            type="button"
            className="eard-btn eard-btn--ghost"
            onClick={onClose}
            disabled={saving}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            className="eard-btn eard-btn--primary"
            onClick={handleSave}
            disabled={!canSave}
          >
            {saving ? (
              <>
                <i className="fas fa-spinner fa-spin" aria-hidden="true"></i>
                {t('common.saving', 'Saving...')}
              </>
            ) : (
              <>
                <i className="fas fa-check" aria-hidden="true"></i>
                {t('football.matches.lineup.saveLineup', 'Save lineup')}
              </>
            )}
          </button>
        </footer>
      </div>
    </div>
    {pendingLoan !== null && (
      <div className="eard-confirm-layer">
        <ConfirmationDialog
          isOpen
          icon="ℹ️"
          title={pendingLoan.kind === 'players'
            ? t('football.matches.lineup.loanPlayersConfirmTitle', 'Add loan players')
            : t('football.matches.lineup.loanGoalkeeperConfirmTitle', 'Set loan goalkeeper')}
          message={pendingLoan.kind === 'players'
            ? t('football.matches.lineup.loanPlayersConfirmMessage', 'Add {{count}} loan players for {{team}}? Existing loan players are used first.', { team: pendingTeamName, count: pendingLoan.count })
            : t('football.matches.lineup.loanGoalkeeperConfirmMessage', 'Set a loan goalkeeper for {{team}}? The player is created if this team does not have one yet. Otherwise the existing loan goalkeeper is selected.', { team: pendingTeamName })}
          confirmText={pendingLoan.kind === 'players'
            ? t('football.matches.lineup.loanPlayersConfirm', 'Yes, add loan players')
            : t('football.matches.lineup.loanGoalkeeperConfirm', 'Yes, set loan goalkeeper')}
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={loanSide !== null}
          onConfirm={confirmLoan}
          onCancel={() => setPendingLoan(null)}
        />
      </div>
    )}
    </>
  );
};

export default EditActiveRosterDialog;
