import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ChangeEvent, ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import ConfirmationDialog from '../../../../../components/ConfirmationDialog/ConfirmationDialog';
import { formatPersonName } from '../../../../../types/loanGoalkeeper';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchDto,
  type FootballMatchRules,
  type LineupPlayerRequest,
} from '../../../../../types/football/footballTypes';
import {
  lineupPositionOrDefault,
  resolveMatchRules,
  validateTeamLineup,
  type LineupDraftPlayer,
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
  onPlayerAdded: (teamId: string, player: FootballPlayerDto) => void;
  onClose: () => void;
  onSaved: (updatedMatch: FootballMatchDto) => void;
  onError: (message: string | null) => void;
}

interface PlayerLineupState {
  position: FootballPosition;
  isOnField: boolean;
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
      isOnField: saved?.isOnField === true && saved.isSentOff !== true,
      isSentOff: saved?.isSentOff === true,
    });
  }
  return next;
};

const toDraft = (state: TeamLineupMap): LineupDraftPlayer[] =>
  Array.from(state.entries()).map(([playerId, entry]) => ({
    playerId,
    position: entry.position,
    isOnField: entry.isOnField,
    isSentOff: entry.isSentOff,
  }));

interface TeamColumnProps {
  teamLabel: string;
  players: FootballPlayerDto[];
  state: TeamLineupMap;
  rules: FootballMatchRules;
  onToggleOnField: (playerId: string) => void;
  onChangePosition: (playerId: string, position: FootballPosition) => void;
  onUseLoanGoalkeeper: () => void;
  loanGoalkeeperBusy: boolean;
}

const TeamColumn = ({
  teamLabel,
  players,
  state,
  rules,
  onToggleOnField,
  onChangePosition,
  onUseLoanGoalkeeper,
  loanGoalkeeperBusy,
}: TeamColumnProps): ReactElement => {
  const { t } = useTranslation();
  const [search, setSearch] = useState<string>('');

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

  const onFieldCount: number = Array.from(state.values()).filter((entry) => entry.isOnField).length;
  const validationMessage: string | null = validateTeamLineup(toDraft(state), rules);

  return (
    <div className="eard-column">
      <div className="eard-column__header">
        <h3 className="eard-column__team">{teamLabel}</h3>
        <span className={`eard-column__count${validationMessage ? ' eard-column__count--invalid' : ''}`}>
          {t('football.matches.lineup.onFieldCount', '{{current}} / {{required}} on field', {
            current: onFieldCount,
            required: rules.playersOnField,
          })}
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

      {validationMessage && (
        <span className="eard-field__warning">
          <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
          {validationMessage}
        </span>
      )}

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
                <th className="eard-table__jersey">#</th>
                <th>{t('football.matches.lineup.player', 'Player')}</th>
                <th>{t('football.matches.lineup.position', 'Position')}</th>
                <th>{t('football.matches.lineup.status', 'Status')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredPlayers.map((player: FootballPlayerDto) => {
                const entry: PlayerLineupState | undefined = state.get(player.id);
                const isSentOff: boolean = entry?.isSentOff === true;
                const isOnField: boolean = entry?.isOnField === true;
                return (
                  <tr key={player.id} className={isSentOff ? 'eard-row--sent-off' : ''}>
                    <td className="eard-table__jersey">
                      {player.jerseyNumber != null ? `#${player.jerseyNumber}` : '–'}
                    </td>
                    <td>
                      <span className="eard-player-name">
                        {formatPersonName(player.person.firstName, player.person.lastName)}
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
                        disabled={isSentOff}
                        onChange={(e: ChangeEvent<HTMLSelectElement>) =>
                          onChangePosition(player.id, e.target.value as FootballPosition)
                        }
                      >
                        {LINEUP_POSITIONS.map((position) => (
                          <option key={position} value={position}>
                            {position}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="eard-table__action">
                      <button
                        type="button"
                        className={`eard-btn eard-btn--sm ${isOnField ? 'eard-btn--add-defender' : 'eard-btn--ghost'}`}
                        disabled={isSentOff}
                        onClick={() => onToggleOnField(player.id)}
                      >
                        {isSentOff
                          ? t('football.matches.lineup.sentOff', 'Sent off')
                          : isOnField
                            ? t('football.matches.lineup.onField', 'On field')
                            : t('football.matches.lineup.bench', 'Bench')}
                      </button>
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
  const [pendingLoanSide, setPendingLoanSide] = useState<'home' | 'away' | null>(null);
  const wasOpenRef = useRef(false);

  useEffect(() => {
    if (!isOpen) {
      wasOpenRef.current = false;
      setPendingLoanSide(null);
      return;
    }
    if (wasOpenRef.current) {
      return;
    }
    wasOpenRef.current = true;
    setHomeState(buildInitialState(homePlayers, initialHomeLineup));
    setAwayState(buildInitialState(awayPlayers, initialAwayLineup));
  }, [isOpen, homePlayers, awayPlayers, initialHomeLineup, initialAwayLineup]);

  const toggleOnField = useCallback((side: 'home' | 'away', playerId: string): void => {
    const updater = (prev: TeamLineupMap): TeamLineupMap => {
      const next = new Map(prev);
      const current = next.get(playerId);
      if (!current || current.isSentOff) return prev;
      next.set(playerId, { ...current, isOnField: !current.isOnField });
      return next;
    };
    if (side === 'home') {
      setHomeState(updater);
    } else {
      setAwayState(updater);
    }
  }, []);

  const changePosition = useCallback((side: 'home' | 'away', playerId: string, position: FootballPosition): void => {
    const updater = (prev: TeamLineupMap): TeamLineupMap => {
      const next = new Map(prev);
      const current = next.get(playerId);
      if (!current) return prev;
      next.set(playerId, { ...current, position });
      return next;
    };
    if (side === 'home') {
      setHomeState(updater);
    } else {
      setAwayState(updater);
    }
  }, []);

  const placeLoanGoalkeeper = useCallback((side: 'home' | 'away', playerId: string): void => {
    const updater = (prev: TeamLineupMap): TeamLineupMap => {
      const next = new Map(prev);
      if (rules.requireGoalkeeper) {
        for (const [id, entry] of next) {
          if (id !== playerId && entry.isOnField && entry.position === FootballPosition.Goalkeeper && !entry.isSentOff) {
            next.set(id, { ...entry, isOnField: false });
          }
        }
      }
      next.set(playerId, {
        position: FootballPosition.Goalkeeper,
        isOnField: true,
        isSentOff: false,
      });
      return next;
    };
    if (side === 'home') {
      setHomeState(updater);
    } else {
      setAwayState(updater);
    }
  }, [rules.requireGoalkeeper]);

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
      setPendingLoanSide(null);
    }
  }, [awayTeamId, competitionId, homeTeamId, onError, onPlayerAdded, placeLoanGoalkeeper, t]);

  const homeError: string | null = validateTeamLineup(toDraft(homeState), rules);
  const awayError: string | null = validateTeamLineup(toDraft(awayState), rules);
  const canSave: boolean = !homeError && !awayError && !saving && loanSide === null;

  const toPayload = (state: TeamLineupMap): LineupPlayerRequest[] =>
    Array.from(state.entries()).map(([playerId, entry]) => ({
      playerId,
      position: entry.position,
      isOnField: entry.isSentOff ? false : entry.isOnField,
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
      const message: string = error instanceof Error ? error.message : 'Failed to save lineup';
      onError(message);
    } finally {
      setSaving(false);
    }
  }, [canSave, homeState, awayState, matchId, homeTeamId, awayTeamId, onClose, onSaved, onError]);

  const pendingTeamName: string = pendingLoanSide === 'home'
    ? homeTeamName
    : pendingLoanSide === 'away'
      ? awayTeamName
      : '';

  const confirmLoanGoalkeeper = (): void => {
    if (pendingLoanSide === null || loanSide !== null) {
      return;
    }
    void addLoanGoalkeeper(pendingLoanSide);
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
            onToggleOnField={(id) => toggleOnField('home', id)}
            onChangePosition={(id, position) => changePosition('home', id, position)}
            onUseLoanGoalkeeper={() => setPendingLoanSide('home')}
            loanGoalkeeperBusy={loanSide !== null}
          />
          <TeamColumn
            teamLabel={awayTeamName}
            players={awayPlayers}
            state={awayState}
            rules={rules}
            onToggleOnField={(id) => toggleOnField('away', id)}
            onChangePosition={(id, position) => changePosition('away', id, position)}
            onUseLoanGoalkeeper={() => setPendingLoanSide('away')}
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
    {pendingLoanSide !== null && (
      <div className="eard-confirm-layer">
        <ConfirmationDialog
          isOpen
          icon="ℹ️"
          title={t('football.matches.lineup.loanGoalkeeperConfirmTitle', 'Set loan goalkeeper')}
          message={t('football.matches.lineup.loanGoalkeeperConfirmMessage', 'Set a loan goalkeeper for {{team}}? The player is created if this team does not have one yet. Otherwise the existing loan goalkeeper is selected.', { team: pendingTeamName })}
          confirmText={t('football.matches.lineup.loanGoalkeeperConfirm', 'Yes, set loan goalkeeper')}
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={loanSide !== null}
          onConfirm={confirmLoanGoalkeeper}
          onCancel={() => setPendingLoanSide(null)}
        />
      </div>
    )}
    </>
  );
};

export default EditActiveRosterDialog;
