import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ChangeEvent, ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { hockeyMatchService } from '../../../../../api/hockey/hockeyMatchService';
import { hockeyTeamService } from '../../../../../api/hockey/hockeyTeamService';
import ConfirmationDialog from '../../../../../components/ConfirmationDialog/ConfirmationDialog';
import { formatPersonName, rosterDisplayName } from '../../../../../types/loanGoalkeeper';
import { parseLoanPlayerCount } from '../../../../../types/loanPlayer';
import type {
  HockeyMatchDto,
  HockeyPosition,
  HockeyTeamDto,
  HockeyTeamPlayerDto,
} from '../../../../../types/hockey/hockeyTypes';
import { hockeyAwayTeam, hockeyHomeTeam } from '../../../../../types/hockey/hockeyTypes';
import './EditActiveRosterDialog.scss';

type PositionFilter = 'all' | 'field' | 'goalkeeper';
type FieldRole = 'Defenseman' | 'Forward';
// A dressed goalie who is not the starting goalie.
type LineupRole = FieldRole | 'BackupGoalie';

const FIELD_ROLES: FieldRole[] = ['Defenseman', 'Forward'];

interface HockeyLineupPlayer {
  id: string;
  jerseyNumber: number | undefined;
  firstName: string;
  lastName: string;
  fullName: string;
  position: HockeyPosition;
}

interface TeamLineupState {
  players: Map<string, LineupRole>;
  goalieId: string;
}

interface EditActiveRosterDialogProps {
  isOpen: boolean;
  match: HockeyMatchDto;
  homeTeam: HockeyTeamDto | undefined;
  awayTeam: HockeyTeamDto | undefined;
  playerNames: Map<string, string>;
  onLoanGoalkeeperAdded: (teamId: string, row: HockeyTeamPlayerDto, displayName: string) => void;
  onClose: () => void;
  onSaved: (updated: HockeyMatchDto) => void;
  onError: (message: string | null) => void;
}

interface TeamColumnProps {
  teamLabel: string;
  players: HockeyLineupPlayer[];
  state: TeamLineupState;
  onAddPlayer: (playerId: string, role: LineupRole) => void;
  onRemovePlayer: (playerId: string) => void;
  onSetGoalie: (goalieId: string) => void;
  onUseLoanGoalkeeper: () => void;
  onUseLoanPlayers: (count: number) => void;
  loanGoalkeeperBusy: boolean;
}

type PendingLoanAction =
  | { kind: 'goalkeeper'; side: 'home' | 'away' }
  | { kind: 'players'; side: 'home' | 'away'; count: number };

interface RoleChipsRowProps {
  label: string;
  emptyLabel: string;
  players: HockeyLineupPlayer[];
  onRemove: (playerId: string) => void;
  removeAriaLabel: string;
}

const toLineupPlayers = (
  roster: HockeyTeamPlayerDto[],
  playerNames: Map<string, string>,
  competitionId: string | null,
): HockeyLineupPlayer[] => {
  return roster
    .filter((row) => row.isActive && row.competitionId === competitionId)
    .map((row) => {
      const storedName = playerNames.get(row.id) ?? row.playerId.slice(0, 8);
      const { firstName, lastName } = rosterDisplayName(storedName);
      const fullName = formatPersonName(firstName, lastName);
      return {
        id: row.id,
        jerseyNumber: row.jerseyNumber ?? undefined,
        firstName,
        lastName,
        fullName,
        position: row.position,
      };
    });
};

const fieldRoleFromPosition = (position: string): FieldRole =>
  position === 'Defenseman' ? 'Defenseman' : 'Forward';

const sortPlayers = (a: HockeyLineupPlayer, b: HockeyLineupPlayer): number => {
  const numA = a.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  const numB = b.jerseyNumber ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) {
    return numA - numB;
  }
  return `${a.lastName} ${a.firstName}`.localeCompare(`${b.lastName} ${b.firstName}`, undefined, {
    sensitivity: 'base',
  });
};

const matchesSearch = (player: HockeyLineupPlayer, search: string): boolean => {
  if (!search) {
    return true;
  }
  const haystack = `${player.firstName} ${player.lastName} #${player.jerseyNumber ?? ''}`.toLowerCase();
  return haystack.includes(search.toLowerCase());
};

const matchesPosition = (player: HockeyLineupPlayer, filter: PositionFilter): boolean => {
  if (filter === 'all') {
    return true;
  }
  if (filter === 'goalkeeper') {
    return player.position === 'Goalie';
  }
  return player.position !== 'Goalie';
};

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
        {players.map((player) => (
          <li key={player.id} className="eard-chip">
            {player.jerseyNumber != null && (
              <span className="eard-chip__jersey">#{player.jerseyNumber}</span>
            )}
            <span className="eard-chip__name">
              {player.firstName} {player.lastName}
            </span>
            <button
              type="button"
              className="eard-chip__remove"
              onClick={() => onRemove(player.id)}
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
  const [search, setSearch] = useState('');
  const [positionFilter, setPositionFilter] = useState<PositionFilter>('all');
  const [loanPlayerCount, setLoanPlayerCount] = useState<string>('1');
  const parsedLoanPlayerCount: number | null = parseLoanPlayerCount(loanPlayerCount);

  const sortedPlayers = useMemo(
    () => [...players].sort((left, right) => {
      const leftGoalie = left.position === 'Goalie';
      const rightGoalie = right.position === 'Goalie';
      if (leftGoalie !== rightGoalie) {
        return leftGoalie ? -1 : 1;
      }
      return sortPlayers(left, right);
    }),
    [players],
  );

  const defenders = useMemo(
    () => sortedPlayers.filter((player) => state.players.get(player.id) === 'Defenseman'),
    [sortedPlayers, state.players],
  );

  const forwards = useMemo(
    () => sortedPlayers.filter((player) => state.players.get(player.id) === 'Forward'),
    [sortedPlayers, state.players],
  );

  const backupGoalies = useMemo(
    () => sortedPlayers.filter((player) => state.players.get(player.id) === 'BackupGoalie'),
    [sortedPlayers, state.players],
  );

  const availablePlayers = useMemo(() => {
    return sortedPlayers.filter(
      (player) =>
        !state.players.has(player.id) &&
        player.id !== state.goalieId &&
        matchesSearch(player, search) &&
        matchesPosition(player, positionFilter),
    );
  }, [sortedPlayers, state.players, state.goalieId, search, positionFilter]);

  const totalSelected = state.players.size;

  return (
    <div className="eard-column">
      <div className="eard-column__header">
        <h3 className="eard-column__team">{teamLabel}</h3>
        <span className="eard-column__count">
          {t('hockey.matches.lineup.playerCount', '{{count}} players', { count: totalSelected })}
        </span>
      </div>

      <label className="eard-field">
        <span className="eard-field__label eard-field__label--required">
          {t('hockey.matches.lineup.goalkeeper', 'Goalkeeper')}
          <span className="eard-required-marker" aria-hidden="true">*</span>
        </span>
        <div className="eard-field__controls">
          <select
            className="eard-field__select"
            value={state.goalieId}
            onChange={(event: ChangeEvent<HTMLSelectElement>) => onSetGoalie(event.target.value)}
          >
            <option value="">{t('hockey.matches.lineup.selectGoalkeeper', 'Select goalkeeper')}</option>
            {sortedPlayers.map((player) => (
              <option key={player.id} value={player.id}>
                {player.jerseyNumber != null ? `#${player.jerseyNumber} ` : ''}
                {formatPersonName(player.firstName, player.lastName)}
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
              : t('hockey.matches.lineup.loanGoalkeeper', 'Lainavahti')}
          </button>
        </div>
        {!state.goalieId && (
          <span className="eard-field__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t('hockey.matches.lineup.selectGoalkeeperRequired', 'Goalkeeper is required to start the match')}
          </span>
        )}
      </label>
      <p className="eard-field__hint">
        {t('hockey.matches.lineup.loanGoalkeeperHint', 'Creates a loan goalkeeper for the team and sets them as the goalkeeper. If one already exists, that same player is selected.')}
      </p>
      <div className="eard-loan-players">
        <label className="eard-loan-players__count">
          <span>{t('hockey.matches.lineup.loanPlayersCount', 'Count')}</span>
          <input
            type="number"
            min={1}
            max={20}
            value={loanPlayerCount}
            onChange={(event: ChangeEvent<HTMLInputElement>) => setLoanPlayerCount(event.target.value)}
            disabled={loanGoalkeeperBusy}
            aria-label={t('hockey.matches.lineup.loanPlayersCount', 'Count')}
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
            : t('hockey.matches.lineup.loanPlayers', 'Lainapelaajat')}
        </button>
      </div>
      <p className="eard-field__hint">
        {t('hockey.matches.lineup.loanPlayersHint', 'Adds the requested number of loan players to this match. Existing loan players are used first, and only the missing ones are created.')}
      </p>

      <div className="eard-section">
        <div className="eard-section__header">
          <h4 className="eard-section__title">
            {t('hockey.matches.lineup.players', 'Field players')}
          </h4>
          <span className="eard-section__count">
            {t('hockey.matches.lineup.selectedCount', '{{count}} selected', { count: totalSelected })}
          </span>
        </div>

        <div className="eard-role-groups">
          <RoleChipsRow
            label={t('hockey.matches.lineup.defenders', 'Defenders')}
            emptyLabel={t('hockey.matches.lineup.noDefenders', 'No defenders selected.')}
            players={defenders}
            onRemove={onRemovePlayer}
            removeAriaLabel={t('hockey.matches.lineup.removePlayer', 'Remove from lineup')}
          />
          <RoleChipsRow
            label={t('hockey.matches.lineup.forwards', 'Forwards')}
            emptyLabel={t('hockey.matches.lineup.noForwards', 'No forwards selected.')}
            players={forwards}
            onRemove={onRemovePlayer}
            removeAriaLabel={t('hockey.matches.lineup.removePlayer', 'Remove from lineup')}
          />
          <RoleChipsRow
            label={t('hockey.matches.lineup.backupGoalies', 'Backup goalies')}
            emptyLabel={t('hockey.matches.lineup.noBackupGoalies', 'No backup goalie.')}
            players={backupGoalies}
            onRemove={onRemovePlayer}
            removeAriaLabel={t('hockey.matches.lineup.removePlayer', 'Remove from lineup')}
          />
        </div>

        <div className="eard-filters">
          <div className="eard-filters__search">
            <i className="fas fa-search" aria-hidden="true"></i>
            <input
              type="text"
              placeholder={t('hockey.matches.lineup.searchPlayers', 'Search players by name...')}
              value={search}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setSearch(event.target.value)}
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
            onChange={(event: ChangeEvent<HTMLSelectElement>) =>
              setPositionFilter(event.target.value as PositionFilter)
            }
          >
            <option value="all">{t('hockey.matches.lineup.allPositions', 'All positions')}</option>
            <option value="field">{t('hockey.matches.lineup.fieldPlayers', 'Field players')}</option>
            <option value="goalkeeper">{t('hockey.matches.lineup.goalkeepersOnly', 'Goalkeepers')}</option>
          </select>
        </div>

        <div className="eard-table-wrapper">
          {availablePlayers.length === 0 ? (
            <div className="eard-empty">
              {sortedPlayers.length === 0
                ? t('hockey.matches.lineup.noTeamPlayers', 'Team has no players.')
                : t('hockey.matches.lineup.noAvailablePlayers', 'No available players match the current filters.')}
            </div>
          ) : (
            <table className="eard-table">
              <thead>
                <tr>
                  <th className="eard-table__jersey">#</th>
                  <th>{t('hockey.matches.lineup.player', 'Player')}</th>
                  <th>{t('hockey.matches.lineup.position', 'Position')}</th>
                  <th className="eard-table__action">
                    <span className="eard-visually-hidden">
                      {t('hockey.matches.lineup.action', 'Action')}
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {availablePlayers.map((player) => (
                  <tr key={player.id}>
                    <td className="eard-table__jersey">
                      {player.jerseyNumber != null ? `#${player.jerseyNumber}` : '–'}
                    </td>
                    <td>
                      <span className="eard-player-name">
                        {player.firstName} {player.lastName}
                      </span>
                    </td>
                    <td>
                      <span className="eard-position-badge">
                        {t(`hockey.positions.${player.position}`, player.position)}
                      </span>
                    </td>
                    <td className="eard-table__action">
                      <div className="eard-add-buttons">
                        {FIELD_ROLES.map((role) => (
                          <button
                            key={role}
                            type="button"
                            className={`eard-btn eard-btn--sm eard-btn--add eard-btn--add-${role.toLowerCase()}`}
                            onClick={() => onAddPlayer(player.id, role)}
                            title={t(`hockey.matches.lineup.addAs.${role}`, `Add as ${role}`)}
                          >
                            <i className="fas fa-plus" aria-hidden="true"></i>
                            {role === 'Defenseman'
                              ? t('hockey.matches.lineup.addAs.Defenseman', 'Defender')
                              : t('hockey.matches.lineup.addAs.Forward', 'Forward')}
                          </button>
                        ))}
                        {player.position === 'Goalie' && (
                          <button
                            type="button"
                            className="eard-btn eard-btn--sm eard-btn--add eard-btn--add-backupgoalie"
                            onClick={() => onAddPlayer(player.id, 'BackupGoalie')}
                            title={t('hockey.matches.lineup.addAs.BackupGoalie', 'Backup goalie')}
                          >
                            <i className="fas fa-plus" aria-hidden="true"></i>
                            {t('hockey.matches.lineup.addAs.BackupGoalie', 'Backup goalie')}
                          </button>
                        )}
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

const lineupFromMatch = (
  match: HockeyMatchDto,
  side: 'home' | 'away',
): TeamLineupState => {
  const matchTeam = side === 'home' ? hockeyHomeTeam(match) : hockeyAwayTeam(match);
  const active = matchTeam?.activePlayers ?? [];
  const goalieId =
    matchTeam?.activeGoalieMatchPlayerId
      ? (active.find((player) => player.id === matchTeam.activeGoalieMatchPlayerId)?.teamPlayerId ?? '')
      : (active.find((player) => player.isGoalie)?.teamPlayerId ?? '');
  const players = new Map<string, LineupRole>();
  for (const player of active) {
    if (player.teamPlayerId === goalieId) {
      continue;
    }
    // Keep other dressed goalies, or every save would drop them from the lineup.
    players.set(
      player.teamPlayerId,
      player.isGoalie || player.position === 'Goalie' ? 'BackupGoalie' : fieldRoleFromPosition(player.position),
    );
  }
  return { players, goalieId };
};

const confirmTeamRoster = async (
  matchId: string,
  matchTeamId: string,
  state: TeamLineupState,
): Promise<HockeyMatchDto> => {
  const teamPlayerIds = [...state.players.keys()];
  if (state.goalieId && !teamPlayerIds.includes(state.goalieId)) {
    teamPlayerIds.push(state.goalieId);
  }
  const confirmed = await hockeyMatchService.confirmRoster(matchId, matchTeamId, teamPlayerIds);
  const matchTeam = confirmed.matchTeams.find((item) => item.id === matchTeamId);
  const goalieActive = matchTeam?.activePlayers.find((player) => player.teamPlayerId === state.goalieId);
  if (goalieActive) {
    return hockeyMatchService.setActiveGoalie(matchId, matchTeamId, goalieActive.id);
  }
  return confirmed;
};

function EditActiveRosterDialog({
  isOpen,
  match,
  homeTeam,
  awayTeam,
  playerNames,
  onLoanGoalkeeperAdded,
  onClose,
  onSaved,
  onError,
}: EditActiveRosterDialogProps): ReactElement | null {
  const { t } = useTranslation();
  const [homeState, setHomeState] = useState<TeamLineupState>(() => lineupFromMatch(match, 'home'));
  const [awayState, setAwayState] = useState<TeamLineupState>(() => lineupFromMatch(match, 'away'));
  const [saving, setSaving] = useState(false);
  const [loanSide, setLoanSide] = useState<'home' | 'away' | null>(null);
  const [pendingLoan, setPendingLoan] = useState<PendingLoanAction | null>(null);

  useEffect(() => {
    if (!isOpen) {
      setPendingLoan(null);
      return;
    }
    setHomeState(lineupFromMatch(match, 'home'));
    setAwayState(lineupFromMatch(match, 'away'));
  }, [isOpen, match]);

  const homePlayers = useMemo(
    () => toLineupPlayers(homeTeam?.roster ?? [], playerNames, match.competitionId),
    [homeTeam, playerNames, match.competitionId],
  );
  const awayPlayers = useMemo(
    () => toLineupPlayers(awayTeam?.roster ?? [], playerNames, match.competitionId),
    [awayTeam, playerNames, match.competitionId],
  );

  const updateTeamState = useCallback(
    (side: 'home' | 'away', updater: (prev: TeamLineupState) => TeamLineupState): void => {
      if (side === 'home') {
        setHomeState((prev) => updater(prev));
      } else {
        setAwayState((prev) => updater(prev));
      }
    },
    [],
  );

  const addPlayer = useCallback(
    (side: 'home' | 'away', playerId: string, role: LineupRole): void => {
      updateTeamState(side, (prev) => {
        if (playerId === prev.goalieId) {
          return prev;
        }
        const next = new Map(prev.players);
        next.set(playerId, role);
        return { ...prev, players: next };
      });
    },
    [updateTeamState],
  );

  const removePlayer = useCallback(
    (side: 'home' | 'away', playerId: string): void => {
      updateTeamState(side, (prev) => {
        if (!prev.players.has(playerId)) {
          return prev;
        }
        const next = new Map(prev.players);
        next.delete(playerId);
        return { ...prev, players: next };
      });
    },
    [updateTeamState],
  );

  const setGoalie = useCallback(
    (side: 'home' | 'away', goalieId: string): void => {
      updateTeamState(side, (prev) => {
        const next = new Map(prev.players);
        if (goalieId) {
          next.delete(goalieId);
        }
        return { players: next, goalieId };
      });
    },
    [updateTeamState],
  );

  const addLoanGoalkeeper = useCallback(
    async (side: 'home' | 'away'): Promise<void> => {
      const team = side === 'home' ? homeTeam : awayTeam;
      if (!team) {
        return;
      }
      try {
        setLoanSide(side);
        onError(null);
        const result = await hockeyTeamService.ensureLoanGoalkeeper(team.id, match.competitionId);
        const row: HockeyTeamPlayerDto = {
          id: result.rosterEntryId,
          teamId: team.id,
          playerId: result.playerId,
          competitionId: match.competitionId,
          position: 'Goalie',
          captainRole: 'None',
          rosterStatus: 'Active',
          jerseyNumber: null,
          requestedJerseyNumber: null,
          isActive: true,
          joinedAt: new Date().toISOString(),
        };
        onLoanGoalkeeperAdded(team.id, row, result.displayName);
        setGoalie(side, result.rosterEntryId);
      } catch (error) {
        onError(error instanceof Error
          ? error.message
          : t('hockey.matches.lineup.loanGoalkeeperFailed', 'Failed to add loan goalkeeper'));
      } finally {
        setLoanSide(null);
        setPendingLoan(null);
      }
    },
    [awayTeam, homeTeam, match.competitionId, onError, onLoanGoalkeeperAdded, setGoalie, t],
  );

  const addLoanPlayers = useCallback(
    async (side: 'home' | 'away', count: number): Promise<void> => {
      const team = side === 'home' ? homeTeam : awayTeam;
      if (!team) {
        return;
      }
      try {
        setLoanSide(side);
        onError(null);
        const results = await hockeyTeamService.ensureLoanPlayers(team.id, count, match.competitionId);
        const rosterEntryIds: string[] = [];
        for (const result of results) {
          const row: HockeyTeamPlayerDto = {
            id: result.rosterEntryId,
            teamId: team.id,
            playerId: result.playerId,
            competitionId: match.competitionId,
            position: 'Center',
            captainRole: 'None',
            rosterStatus: 'Active',
            jerseyNumber: result.jerseyNumber,
            requestedJerseyNumber: null,
            isActive: true,
            joinedAt: new Date().toISOString(),
          };
          onLoanGoalkeeperAdded(team.id, row, result.displayName);
          rosterEntryIds.push(result.rosterEntryId);
        }
        updateTeamState(side, (prev) => {
          const next = new Map(prev.players);
          for (const rosterEntryId of rosterEntryIds) {
            if (rosterEntryId === prev.goalieId || next.has(rosterEntryId)) {
              continue;
            }
            next.set(rosterEntryId, 'Forward');
          }
          return { ...prev, players: next };
        });
      } catch (error) {
        onError(error instanceof Error
          ? error.message
          : t('hockey.matches.lineup.loanPlayersFailed', 'Failed to add loan players'));
      } finally {
        setLoanSide(null);
        setPendingLoan(null);
      }
    },
    [awayTeam, homeTeam, match.competitionId, onError, onLoanGoalkeeperAdded, t, updateTeamState],
  );

  const homeMatchTeam = hockeyHomeTeam(match);
  const awayMatchTeam = hockeyAwayTeam(match);
  const canSave = Boolean(homeState.goalieId) && Boolean(awayState.goalieId) && Boolean(homeMatchTeam) && Boolean(awayMatchTeam) && !saving && loanSide === null;

  const handleSave = useCallback(async (): Promise<void> => {
    if (!canSave || !homeMatchTeam || !awayMatchTeam) {
      return;
    }
    try {
      setSaving(true);
      onError(null);
      await confirmTeamRoster(match.id, homeMatchTeam.id, homeState);
      const updated = await confirmTeamRoster(match.id, awayMatchTeam.id, awayState);
      onSaved(updated);
      onClose();
    } catch (error) {
      onError(error instanceof Error ? error.message : t('hockey.matches.errors.rosterFailed', 'Failed to save roster'));
    } finally {
      setSaving(false);
    }
  }, [canSave, homeMatchTeam, awayMatchTeam, match.id, homeState, awayState, onClose, onSaved, onError, t]);

  const pendingTeamName: string = pendingLoan?.side === 'home'
    ? (homeTeam?.name ?? t('hockey.matches.home', 'Home'))
    : pendingLoan?.side === 'away'
      ? (awayTeam?.name ?? t('hockey.matches.away', 'Away'))
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

  if (!isOpen) {
    return null;
  }

  return (
    <>
    <div className="eard-overlay" onClick={onClose} role="presentation">
      <div
        className="eard-dialog"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="eard-title"
      >
        <header className="eard-header">
          <h2 id="eard-title" className="eard-header__title">
            {t('hockey.matches.lineup.editLineupTitle', 'Edit active lineup')}
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
            teamLabel={homeTeam?.name ?? t('hockey.matches.home', 'Home')}
            players={homePlayers}
            state={homeState}
            onAddPlayer={(id, role) => addPlayer('home', id, role)}
            onRemovePlayer={(id) => removePlayer('home', id)}
            onSetGoalie={(id) => setGoalie('home', id)}
            onUseLoanGoalkeeper={() => setPendingLoan({ kind: 'goalkeeper', side: 'home' })}
            onUseLoanPlayers={(count) => setPendingLoan({ kind: 'players', side: 'home', count })}
            loanGoalkeeperBusy={loanSide !== null || !homeTeam}
          />
          <TeamColumn
            teamLabel={awayTeam?.name ?? t('hockey.matches.away', 'Away')}
            players={awayPlayers}
            state={awayState}
            onAddPlayer={(id, role) => addPlayer('away', id, role)}
            onRemovePlayer={(id) => removePlayer('away', id)}
            onSetGoalie={(id) => setGoalie('away', id)}
            onUseLoanGoalkeeper={() => setPendingLoan({ kind: 'goalkeeper', side: 'away' })}
            onUseLoanPlayers={(count) => setPendingLoan({ kind: 'players', side: 'away', count })}
            loanGoalkeeperBusy={loanSide !== null || !awayTeam}
          />
        </div>

        <footer className="eard-footer">
          <button type="button" className="eard-btn eard-btn--ghost" onClick={onClose} disabled={saving}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button type="button" className="eard-btn eard-btn--primary" onClick={() => void handleSave()} disabled={!canSave}>
            {saving ? (
              <>
                <i className="fas fa-spinner fa-spin" aria-hidden="true"></i>
                {t('common.saving', 'Saving...')}
              </>
            ) : (
              <>
                <i className="fas fa-check" aria-hidden="true"></i>
                {t('hockey.matches.lineup.saveLineup', 'Save lineup')}
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
            ? t('hockey.matches.lineup.loanPlayersConfirmTitle', 'Add loan players')
            : t('hockey.matches.lineup.loanGoalkeeperConfirmTitle', 'Set loan goalkeeper')}
          message={pendingLoan.kind === 'players'
            ? t('hockey.matches.lineup.loanPlayersConfirmMessage', 'Add {{count}} loan players for {{team}}? Existing loan players are used first.', { team: pendingTeamName, count: pendingLoan.count })
            : t('hockey.matches.lineup.loanGoalkeeperConfirmMessage', 'Set a loan goalkeeper for {{team}}? The player is created if this team does not have one yet. Otherwise the existing loan goalkeeper is selected.', { team: pendingTeamName })}
          confirmText={pendingLoan.kind === 'players'
            ? t('hockey.matches.lineup.loanPlayersConfirm', 'Yes, add loan players')
            : t('hockey.matches.lineup.loanGoalkeeperConfirm', 'Yes, set loan goalkeeper')}
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={loanSide !== null}
          onConfirm={confirmLoan}
          onCancel={() => setPendingLoan(null)}
        />
      </div>
    )}
    </>
  );
}

export default EditActiveRosterDialog;
