import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ChangeEvent, ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { hockeyMatchService } from '../../../../../api/hockey/hockeyMatchService';
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
  players: Map<string, FieldRole>;
  goalieId: string;
}

interface EditActiveRosterDialogProps {
  isOpen: boolean;
  match: HockeyMatchDto;
  homeTeam: HockeyTeamDto | undefined;
  awayTeam: HockeyTeamDto | undefined;
  playerNames: Map<string, string>;
  onClose: () => void;
  onSaved: (updated: HockeyMatchDto) => void;
  onError: (message: string | null) => void;
}

interface TeamColumnProps {
  teamLabel: string;
  players: HockeyLineupPlayer[];
  state: TeamLineupState;
  onAddPlayer: (playerId: string, role: FieldRole) => void;
  onRemovePlayer: (playerId: string) => void;
  onSetGoalie: (goalieId: string) => void;
}

interface RoleChipsRowProps {
  label: string;
  emptyLabel: string;
  players: HockeyLineupPlayer[];
  onRemove: (playerId: string) => void;
  removeAriaLabel: string;
}

const splitName = (fullName: string): { firstName: string; lastName: string } => {
  const trimmed = fullName.trim();
  const spaceIndex = trimmed.indexOf(' ');
  if (spaceIndex === -1) {
    return { firstName: trimmed, lastName: '' };
  }
  return {
    firstName: trimmed.slice(0, spaceIndex),
    lastName: trimmed.slice(spaceIndex + 1),
  };
};

const toLineupPlayers = (
  roster: HockeyTeamPlayerDto[],
  playerNames: Map<string, string>,
): HockeyLineupPlayer[] => {
  return roster
    .filter((row) => row.isActive)
    .map((row) => {
      const fullName = playerNames.get(row.id) ?? row.playerId.slice(0, 8);
      const { firstName, lastName } = splitName(fullName);
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
            {player.jerseyNumber !== undefined && (
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
}: TeamColumnProps): ReactElement => {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [positionFilter, setPositionFilter] = useState<PositionFilter>('all');

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
        <select
          className="eard-field__select"
          value={state.goalieId}
          onChange={(event: ChangeEvent<HTMLSelectElement>) => onSetGoalie(event.target.value)}
        >
          <option value="">{t('hockey.matches.lineup.selectGoalkeeper', 'Select goalkeeper')}</option>
          {sortedPlayers.map((player) => (
            <option key={player.id} value={player.id}>
              {player.jerseyNumber !== undefined ? `#${player.jerseyNumber} ` : ''}
              {player.firstName} {player.lastName}
            </option>
          ))}
        </select>
        {!state.goalieId && (
          <span className="eard-field__warning">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t('hockey.matches.lineup.selectGoalkeeperRequired', 'Goalkeeper is required to start the match')}
          </span>
        )}
      </label>

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
                      {player.jerseyNumber !== undefined ? `#${player.jerseyNumber}` : '–'}
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
  const players = new Map<string, FieldRole>();
  for (const player of active) {
    if (player.teamPlayerId === goalieId || player.isGoalie) {
      continue;
    }
    players.set(player.teamPlayerId, fieldRoleFromPosition(player.position));
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
  onClose,
  onSaved,
  onError,
}: EditActiveRosterDialogProps): ReactElement | null {
  const { t } = useTranslation();
  const [homeState, setHomeState] = useState<TeamLineupState>(() => lineupFromMatch(match, 'home'));
  const [awayState, setAwayState] = useState<TeamLineupState>(() => lineupFromMatch(match, 'away'));
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      return;
    }
    setHomeState(lineupFromMatch(match, 'home'));
    setAwayState(lineupFromMatch(match, 'away'));
  }, [isOpen, match]);

  const homePlayers = useMemo(
    () => toLineupPlayers(homeTeam?.roster ?? [], playerNames),
    [homeTeam, playerNames],
  );
  const awayPlayers = useMemo(
    () => toLineupPlayers(awayTeam?.roster ?? [], playerNames),
    [awayTeam, playerNames],
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
    (side: 'home' | 'away', playerId: string, role: FieldRole): void => {
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

  const homeMatchTeam = hockeyHomeTeam(match);
  const awayMatchTeam = hockeyAwayTeam(match);
  const canSave = Boolean(homeState.goalieId) && Boolean(awayState.goalieId) && Boolean(homeMatchTeam) && Boolean(awayMatchTeam) && !saving;

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

  if (!isOpen) {
    return null;
  }

  return (
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
          />
          <TeamColumn
            teamLabel={awayTeam?.name ?? t('hockey.matches.away', 'Away')}
            players={awayPlayers}
            state={awayState}
            onAddPlayer={(id, role) => addPlayer('away', id, role)}
            onRemovePlayer={(id) => removePlayer('away', id)}
            onSetGoalie={(id) => setGoalie('away', id)}
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
  );
}

export default EditActiveRosterDialog;
