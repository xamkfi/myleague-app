import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ChangeEvent, ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import ConfirmationDialog from '../../../../../components/ConfirmationDialog/ConfirmationDialog';
import { formatPersonName } from '../../../../../types/loanGoalkeeper';
import { parseLoanPlayerCount } from '../../../../../types/loanPlayer';
import {
  FloorballPosition,
  type FloorballActiveLineupPlayer,
  type FloorballMatchDto,
} from '../../../../../types/floorball/floorballTypes';
import './EditActiveRosterDialog.scss';

type PositionFilter = 'all' | 'field' | 'goalkeeper';

interface EditActiveRosterDialogProps {
  isOpen: boolean;
  matchId: string;
  homeTeamId: string;
  awayTeamId: string;
  homeTeamName: string;
  awayTeamName: string;
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  initialHomeLineup: FloorballActiveLineupPlayer[];
  initialAwayLineup: FloorballActiveLineupPlayer[];
  initialHomeGoalieId: string;
  initialAwayGoalieId: string;
  competitionId?: string | null;
  onPlayerAdded: (teamId: string, player: FloorballPlayerDto) => void;
  onClose: () => void;
  onSaved: (updatedMatch: FloorballMatchDto) => void;
  onError: (message: string | null) => void;
}

/**
 * Field roles offered when adding a player. Goalies are picked via a separate combo box.
 */
type FieldRole = FloorballPosition.Defender | FloorballPosition.Forward;

const FIELD_ROLES: FieldRole[] = [FloorballPosition.Defender, FloorballPosition.Forward];

interface TeamLineupState {
  /** Map of playerId -> per-match role. */
  players: Map<string, FieldRole>;
  goalieId: string;
}

interface TeamColumnProps {
  teamLabel: string;
  players: FloorballPlayerDto[];
  state: TeamLineupState;
  onAddPlayer: (playerId: string, role: FieldRole) => void;
  onRemovePlayer: (playerId: string) => void;
  onSetGoalie: (goalieId: string) => void;
  onUseLoanGoalkeeper: () => void;
  onUseLoanPlayers: (count: number) => void;
  loanGoalkeeperBusy: boolean;
}

const sortPlayers = (a: FloorballPlayerDto, b: FloorballPlayerDto): number => {
  const numA: number = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  const numB: number = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) return numA - numB;
  return `${a.person.lastName} ${a.person.firstName}`.localeCompare(
    `${b.person.lastName} ${b.person.firstName}`,
    undefined,
    { sensitivity: 'base' }
  );
};

const matchesSearch = (player: FloorballPlayerDto, search: string): boolean => {
  if (!search) return true;
  const haystack: string = `${player.person.firstName} ${player.person.lastName} #${player.jerseyNumber ?? ''}`.toLowerCase();
  return haystack.includes(search.toLowerCase());
};

const matchesPosition = (player: FloorballPlayerDto, filter: PositionFilter): boolean => {
  if (filter === 'all') return true;
  if (filter === 'goalkeeper') return player.position === FloorballPosition.Goalkeeper;
  return player.position !== FloorballPosition.Goalkeeper;
};

interface RoleChipsRowProps {
  label: string;
  emptyLabel: string;
  players: FloorballPlayerDto[];
  onRemove: (playerId: string) => void;
  removeAriaLabel: string;
}

const RoleChipsRow = ({
  label,
  emptyLabel,
  players,
  onRemove,
  removeAriaLabel,
}: RoleChipsRowProps): ReactElement => (
  <div className="eard-role-row">
    <div className="eard-role-row__label">
      <span className="eard-role-row__title">{label}</span>
      <span className="eard-role-row__count">{players.length}</span>
    </div>
    {players.length === 0 ? (
      <div className="eard-role-row__empty">{emptyLabel}</div>
    ) : (
      <ul className="eard-selected-chips">
        {players.map((p: FloorballPlayerDto) => (
          <li key={p.id} className="eard-chip">
            {p.jerseyNumber != null && (
              <span className="eard-chip__jersey">#{p.jerseyNumber}</span>
            )}
            <span className="eard-chip__name">
              {p.person.firstName} {p.person.lastName}
            </span>
            <button
              type="button"
              className="eard-chip__remove"
              onClick={() => onRemove(p.id)}
              aria-label={removeAriaLabel}
              title={removeAriaLabel}
            >
              <i className="fas fa-times" aria-hidden="true"></i>
            </button>
          </li>
        ))}
      </ul>
    )}
  </div>
);

const TeamColumn = ({
  teamLabel,
  players,
  state,
  onAddPlayer,
  onRemovePlayer,
  onSetGoalie,
  onUseLoanGoalkeeper,
  onUseLoanPlayers,
  loanGoalkeeperBusy,
}: TeamColumnProps): ReactElement => {
  const { t } = useTranslation();
  const [search, setSearch] = useState<string>('');
  const [positionFilter, setPositionFilter] = useState<PositionFilter>('all');
  const [loanPlayerCount, setLoanPlayerCount] = useState<string>('1');
  const parsedLoanPlayerCount: number | null = parseLoanPlayerCount(loanPlayerCount);

  const sortedPlayers: FloorballPlayerDto[] = useMemo(
    () => [...players].sort(sortPlayers),
    [players]
  );

  // Goalie combo: every player on the team — position data may be incomplete.
  const goalieOptions: FloorballPlayerDto[] = sortedPlayers;

  const defenders: FloorballPlayerDto[] = useMemo(
    () => sortedPlayers.filter((p) => state.players.get(p.id) === FloorballPosition.Defender),
    [sortedPlayers, state.players]
  );

  const forwards: FloorballPlayerDto[] = useMemo(
    () => sortedPlayers.filter((p) => state.players.get(p.id) === FloorballPosition.Forward),
    [sortedPlayers, state.players]
  );

  const availablePlayers: FloorballPlayerDto[] = useMemo(() => {
    return sortedPlayers.filter(
      (p) =>
        !state.players.has(p.id) &&
        p.id !== state.goalieId &&
        matchesSearch(p, search) &&
        matchesPosition(p, positionFilter)
    );
  }, [sortedPlayers, state.players, state.goalieId, search, positionFilter]);

  const totalSelected: number = state.players.size;

  return (
    <div className="eard-column">
      <div className="eard-column__header">
        <h3 className="eard-column__team">{teamLabel}</h3>
        <span className="eard-column__count">
          {t('floorball.matches.lineup.playerCount', '{{count}} players', { count: totalSelected })}
        </span>
      </div>

      <label className="eard-field">
        <span className="eard-field__label eard-field__label--required">
          {t('floorball.matches.lineup.goalkeeper', 'Goalkeeper')}
          <span className="eard-required-marker" aria-hidden="true">*</span>
        </span>
        <div className="eard-field__controls">
          <select
            className="eard-field__select"
            value={state.goalieId}
            onChange={(e: ChangeEvent<HTMLSelectElement>) => onSetGoalie(e.target.value)}
          >
            <option value="">{t('floorball.matches.lineup.selectGoalkeeper', 'Select goalkeeper')}</option>
            {goalieOptions.map((p: FloorballPlayerDto) => (
              <option key={p.id} value={p.id}>
                {p.jerseyNumber != null ? `#${p.jerseyNumber} ` : ''}
                {formatPersonName(p.person.firstName, p.person.lastName)}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="eard-btn eard-btn--ghost eard-btn--sm"
            onClick={onUseLoanGoalkeeper}
            disabled={loanGoalkeeperBusy}
          >
            {loanGoalkeeperBusy
              ? t('common.saving', 'Saving...')
              : t('floorball.matches.lineup.loanGoalkeeper', 'Lainavahti')}
          </button>
        </div>
        {!state.goalieId && (
          <span className="eard-field__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t('floorball.matches.lineup.selectGoalkeeperRequired', 'Goalkeeper is required to start the match')}
          </span>
        )}
      </label>
      <p className="eard-field__hint">
        {t('floorball.matches.lineup.loanGoalkeeperHint', 'Creates a loan goalkeeper for the team and sets them as the goalkeeper. If one already exists, that same player is selected.')}
      </p>

      <div className="eard-loan-players">
        <label className="eard-loan-players__count">
          <span>{t('floorball.matches.lineup.loanPlayersCount', 'Count')}</span>
          <input
            type="number"
            min={1}
            max={20}
            value={loanPlayerCount}
            onChange={(event: ChangeEvent<HTMLInputElement>) => setLoanPlayerCount(event.target.value)}
            disabled={loanGoalkeeperBusy}
            aria-label={t('floorball.matches.lineup.loanPlayersCount', 'Count')}
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
            : t('floorball.matches.lineup.loanPlayers', 'Lainapelaajat')}
        </button>
      </div>
      <p className="eard-field__hint">
        {t('floorball.matches.lineup.loanPlayersHint', 'Adds the requested number of loan players to this match. Existing loan players are used first, and only the missing ones are created.')}
      </p>

      <div className="eard-section">
        <div className="eard-section__header">
          <h4 className="eard-section__title">
            {t('floorball.matches.lineup.players', 'Field players')}
          </h4>
          <span className="eard-section__count">
            {t('floorball.matches.lineup.selectedCount', '{{count}} selected', { count: totalSelected })}
          </span>
        </div>

        <div className="eard-role-groups">
          <RoleChipsRow
            label={t('floorball.matches.lineup.defenders', 'Defenders')}
            emptyLabel={t('floorball.matches.lineup.noDefenders', 'No defenders selected.')}
            players={defenders}
            onRemove={onRemovePlayer}
            removeAriaLabel={t('floorball.matches.lineup.removePlayer', 'Remove from lineup')}
          />
          <RoleChipsRow
            label={t('floorball.matches.lineup.forwards', 'Forwards')}
            emptyLabel={t('floorball.matches.lineup.noForwards', 'No forwards selected.')}
            players={forwards}
            onRemove={onRemovePlayer}
            removeAriaLabel={t('floorball.matches.lineup.removePlayer', 'Remove from lineup')}
          />
        </div>

        <div className="eard-filters">
          <div className="eard-filters__search">
            <i className="fas fa-search" aria-hidden="true"></i>
            <input
              type="text"
              placeholder={t('floorball.matches.lineup.searchPlayers', 'Search players by name...')}
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
          <select
            className="eard-filters__category"
            value={positionFilter}
            onChange={(e: ChangeEvent<HTMLSelectElement>) =>
              setPositionFilter(e.target.value as PositionFilter)
            }
          >
            <option value="all">{t('floorball.matches.lineup.allPositions', 'All positions')}</option>
            <option value="field">{t('floorball.matches.lineup.fieldPlayers', 'Field players')}</option>
            <option value="goalkeeper">{t('floorball.matches.lineup.goalkeepersOnly', 'Goalkeepers')}</option>
          </select>
        </div>

        <div className="eard-table-wrapper">
          {availablePlayers.length === 0 ? (
            <div className="eard-empty">
              {sortedPlayers.length === 0
                ? t('floorball.matches.lineup.noTeamPlayers', 'Team has no players.')
                : t('floorball.matches.lineup.noAvailablePlayers', 'No available players match the current filters.')}
            </div>
          ) : (
            <table className="eard-table">
              <thead>
                <tr>
                  <th className="eard-table__jersey">#</th>
                  <th>{t('floorball.matches.lineup.player', 'Player')}</th>
                  <th>{t('floorball.matches.lineup.position', 'Position')}</th>
                  <th className="eard-table__action">
                    <span className="eard-visually-hidden">
                      {t('floorball.matches.lineup.action', 'Action')}
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {availablePlayers.map((player: FloorballPlayerDto) => (
                  <tr key={player.id}>
                    <td className="eard-table__jersey">
                      {player.jerseyNumber != null ? `#${player.jerseyNumber}` : '–'}
                    </td>
                    <td>
                      <span className="eard-player-name">
                        {player.person.firstName} {player.person.lastName}
                      </span>
                    </td>
                    <td>
                      <span className="eard-position-badge">{player.position}</span>
                    </td>
                    <td className="eard-table__action">
                      <div className="eard-add-buttons">
                        {FIELD_ROLES.map((role: FieldRole) => (
                          <button
                            key={role}
                            type="button"
                            className={`eard-btn eard-btn--sm eard-btn--add eard-btn--add-${role.toLowerCase()}`}
                            onClick={() => onAddPlayer(player.id, role)}
                            title={t(`floorball.matches.lineup.addAs.${role}`, `Add as ${role}`)}
                          >
                            <i className="fas fa-plus" aria-hidden="true"></i>
                            {role === FloorballPosition.Defender
                              ? t('floorball.matches.lineup.addAs.Defender', 'Defender')
                              : t('floorball.matches.lineup.addAs.Forward', 'Forward')}
                          </button>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
};

type PendingLoanAction =
  | { kind: 'goalkeeper'; side: 'home' | 'away' }
  | { kind: 'players'; side: 'home' | 'away'; count: number };

const lineupToMap = (lineup: FloorballActiveLineupPlayer[]): Map<string, FieldRole> => {
  const map = new Map<string, FieldRole>();
  for (const entry of lineup) {
    // Coerce backend Center/Forward/Defender into the two-role UI model. Center maps to
    // Forward to keep face-off players visible alongside other attackers.
    const role: FieldRole =
      entry.position === FloorballPosition.Defender ? FloorballPosition.Defender : FloorballPosition.Forward;
    map.set(entry.playerId, role);
  }
  return map;
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
  initialHomeGoalieId,
  initialAwayGoalieId,
  competitionId,
  onPlayerAdded,
  onClose,
  onSaved,
  onError,
}: EditActiveRosterDialogProps): ReactElement | null => {
  const { t } = useTranslation();
  const [homeState, setHomeState] = useState<TeamLineupState>({
    players: lineupToMap(initialHomeLineup),
    goalieId: initialHomeGoalieId,
  });
  const [awayState, setAwayState] = useState<TeamLineupState>({
    players: lineupToMap(initialAwayLineup),
    goalieId: initialAwayGoalieId,
  });
  const [saving, setSaving] = useState<boolean>(false);
  const [loanSide, setLoanSide] = useState<'home' | 'away' | null>(null);
  const [pendingLoan, setPendingLoan] = useState<PendingLoanAction | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setPendingLoan(null);
      return;
    }
    setHomeState({
      players: lineupToMap(initialHomeLineup),
      goalieId: initialHomeGoalieId,
    });
    setAwayState({
      players: lineupToMap(initialAwayLineup),
      goalieId: initialAwayGoalieId,
    });
  }, [
    isOpen,
    initialHomeLineup,
    initialAwayLineup,
    initialHomeGoalieId,
    initialAwayGoalieId,
  ]);

  const updateTeamState = useCallback(
    (
      side: 'home' | 'away',
      updater: (prev: TeamLineupState) => TeamLineupState
    ): void => {
      if (side === 'home') {
        setHomeState((prev) => updater(prev));
      } else {
        setAwayState((prev) => updater(prev));
      }
    },
    []
  );

  const addPlayer = useCallback(
    (side: 'home' | 'away', playerId: string, role: FieldRole): void => {
      updateTeamState(side, (prev: TeamLineupState): TeamLineupState => {
        if (playerId === prev.goalieId) return prev;
        const next: Map<string, FieldRole> = new Map(prev.players);
        next.set(playerId, role);
        return { ...prev, players: next };
      });
    },
    [updateTeamState]
  );

  const removePlayer = useCallback(
    (side: 'home' | 'away', playerId: string): void => {
      updateTeamState(side, (prev: TeamLineupState): TeamLineupState => {
        if (!prev.players.has(playerId)) return prev;
        const next: Map<string, FieldRole> = new Map(prev.players);
        next.delete(playerId);
        return { ...prev, players: next };
      });
    },
    [updateTeamState]
  );

  const setGoalie = useCallback(
    (side: 'home' | 'away', goalieId: string): void => {
      updateTeamState(side, (prev: TeamLineupState): TeamLineupState => {
        // Goalie cannot also be in the field player list.
        const next: Map<string, FieldRole> = new Map(prev.players);
        if (goalieId) next.delete(goalieId);
        return { players: next, goalieId };
      });
    },
    [updateTeamState]
  );

  const addLoanGoalkeeper = useCallback(
    async (side: 'home' | 'away'): Promise<void> => {
      const teamId = side === 'home' ? homeTeamId : awayTeamId;
      try {
        setLoanSide(side);
        onError(null);
        const result = await floorballTeamService.ensureLoanGoalkeeper(teamId, competitionId);
        const player: FloorballPlayerDto = {
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
          position: FloorballPosition.Goalkeeper,
          careerGoals: 0,
          careerAssists: 0,
        };
        onPlayerAdded(teamId, player);
        setGoalie(side, result.playerId);
      } catch (error) {
        const message = error instanceof Error
          ? error.message
          : t('floorball.matches.lineup.loanGoalkeeperFailed', 'Failed to add loan goalkeeper');
        onError(message);
      } finally {
        setLoanSide(null);
        setPendingLoan(null);
      }
    },
    [awayTeamId, competitionId, homeTeamId, onError, onPlayerAdded, setGoalie, t],
  );

  const addLoanPlayers = useCallback(
    async (side: 'home' | 'away', count: number): Promise<void> => {
      const teamId = side === 'home' ? homeTeamId : awayTeamId;
      try {
        setLoanSide(side);
        onError(null);
        const results = await floorballTeamService.ensureLoanPlayers(teamId, count, competitionId);
        for (const result of results) {
          const player: FloorballPlayerDto = {
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
            position: FloorballPosition.Forward,
            careerGoals: 0,
            careerAssists: 0,
            jerseyNumber: result.jerseyNumber,
          };
          onPlayerAdded(teamId, player);
        }
        const playerIds: string[] = results.map((result) => result.playerId);
        updateTeamState(side, (prev: TeamLineupState): TeamLineupState => {
          const next: Map<string, FieldRole> = new Map(prev.players);
          for (const playerId of playerIds) {
            if (playerId === prev.goalieId || next.has(playerId)) {
              continue;
            }
            next.set(playerId, FloorballPosition.Forward);
          }
          return { ...prev, players: next };
        });
      } catch (error) {
        const message = error instanceof Error
          ? error.message
          : t('floorball.matches.lineup.loanPlayersFailed', 'Failed to add loan players');
        onError(message);
      } finally {
        setLoanSide(null);
        setPendingLoan(null);
      }
    },
    [awayTeamId, competitionId, homeTeamId, onError, onPlayerAdded, t, updateTeamState],
  );

  const canSave: boolean = Boolean(homeState.goalieId) && Boolean(awayState.goalieId) && !saving && loanSide === null;

  const handleSave = useCallback(async (): Promise<void> => {
    if (!canSave) return;
    try {
      setSaving(true);
      onError(null);

      const homePayload = {
        players: Array.from(homeState.players.entries()).map(([playerId, position]) => ({
          playerId,
          position,
        })),
        goalieId: homeState.goalieId || null,
      };
      const awayPayload = {
        players: Array.from(awayState.players.entries()).map(([playerId, position]) => ({
          playerId,
          position,
        })),
        goalieId: awayState.goalieId || null,
      };

      // Save sequentially. Each backend handler builds its response DTO from its own
      // EF context, so a parallel Promise.all run can return a stale view of the OTHER
      // team (e.g. away response missing freshly-committed home changes). Running them
      // in order guarantees the second handler loads the first's committed state, so
      // its response is authoritative for the entire match.
      const homeResp = await floorballMatchService.setActiveRoster(matchId, homeTeamId, homePayload);
      const awayResp = await floorballMatchService.setActiveRoster(matchId, awayTeamId, awayPayload);

      const updatedMatch: FloorballMatchDto | undefined = awayResp.data ?? homeResp.data;
      if (updatedMatch) {
        onSaved(updatedMatch);
      }
      onClose();
    } catch (error) {
      const message: string = error instanceof Error ? error.message : 'Failed to save active roster';
      onError(message);
    } finally {
      setSaving(false);
    }
  }, [canSave, homeState, awayState, matchId, homeTeamId, awayTeamId, onClose, onSaved, onError]);

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
            {t('floorball.matches.lineup.editLineupTitle', 'Edit active lineup')}
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
            onAddPlayer={(id, role) => addPlayer('home', id, role)}
            onRemovePlayer={(id) => removePlayer('home', id)}
            onSetGoalie={(id) => setGoalie('home', id)}
            onUseLoanGoalkeeper={() => setPendingLoan({ kind: 'goalkeeper', side: 'home' })}
            onUseLoanPlayers={(count) => setPendingLoan({ kind: 'players', side: 'home', count })}
            loanGoalkeeperBusy={loanSide !== null}
          />
          <TeamColumn
            teamLabel={awayTeamName}
            players={awayPlayers}
            state={awayState}
            onAddPlayer={(id, role) => addPlayer('away', id, role)}
            onRemovePlayer={(id) => removePlayer('away', id)}
            onSetGoalie={(id) => setGoalie('away', id)}
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
                {t('floorball.matches.lineup.saveLineup', 'Save lineup')}
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
            ? t('floorball.matches.lineup.loanPlayersConfirmTitle', 'Add loan players')
            : t('floorball.matches.lineup.loanGoalkeeperConfirmTitle', 'Set loan goalkeeper')}
          message={pendingLoan.kind === 'players'
            ? t('floorball.matches.lineup.loanPlayersConfirmMessage', 'Add {{count}} loan players for {{team}}? Existing loan players are used first.', { team: pendingTeamName, count: pendingLoan.count })
            : t('floorball.matches.lineup.loanGoalkeeperConfirmMessage', 'Set a loan goalkeeper for {{team}}? The player is created if this team does not have one yet. Otherwise the existing loan goalkeeper is selected.', { team: pendingTeamName })}
          confirmText={pendingLoan.kind === 'players'
            ? t('floorball.matches.lineup.loanPlayersConfirm', 'Yes, add loan players')
            : t('floorball.matches.lineup.loanGoalkeeperConfirm', 'Yes, set loan goalkeeper')}
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
