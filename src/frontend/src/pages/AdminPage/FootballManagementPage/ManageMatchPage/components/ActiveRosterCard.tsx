import { useMemo } from 'react';
import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchRules,
} from '../../../../../types/football/footballTypes';
import {
  getBenchPlayers,
  getOnFieldPlayers,
  getSentOffPlayers,
  resolveMatchRules,
} from '../utils/lineupValidation';
import './ActiveRosterCard.scss';

interface ActiveRosterCardProps {
  leftTeamName?: string;
  rightTeamName?: string;
  leftPlayers: FootballPlayerDto[];
  rightPlayers: FootballPlayerDto[];
  leftLineup: FootballLineupPlayer[];
  rightLineup: FootballLineupPlayer[];
  matchRules?: FootballMatchRules;
  onEditLineup: () => void;
  disabled?: boolean;
}

interface RosterPanelProps {
  teamName: string;
  players: FootballPlayerDto[];
  lineup: FootballLineupPlayer[];
  playersOnFieldRequired: number;
  requireGoalkeeper: boolean;
}

interface ChipPlayer {
  id: string;
  jersey: number | undefined;
  fullName: string;
  position: FootballPosition;
  isSentOff: boolean;
}

const sortByJerseyThenName = (a: ChipPlayer, b: ChipPlayer): number => {
  const numA: number = a.jersey ?? Number.MAX_SAFE_INTEGER;
  const numB: number = b.jersey ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) return numA - numB;
  return a.fullName.localeCompare(b.fullName, undefined, { sensitivity: 'base' });
};

interface ChipsListProps {
  players: ChipPlayer[];
  emptyLabel: string;
  sentOff?: boolean;
}

const ChipsList = ({ players, emptyLabel, sentOff = false }: ChipsListProps): ReactElement => {
  if (players.length === 0) {
    return <div className="arc-panel__empty">{emptyLabel}</div>;
  }

  return (
    <ul className="arc-panel__chip-list">
      {players.map((p: ChipPlayer) => (
        <li key={p.id} className={`arc-chip${sentOff ? ' arc-chip--sent-off' : ''}`}>
          {p.jersey !== undefined && <span className="arc-chip__jersey">#{p.jersey}</span>}
          <span className="arc-chip__name">{p.fullName}</span>
          {p.position === FootballPosition.Goalkeeper && (
            <span className="arc-chip__role">GK</span>
          )}
        </li>
      ))}
    </ul>
  );
};

const toChip = (
  entry: FootballLineupPlayer,
  playerLookup: Map<string, FootballPlayerDto>,
): ChipPlayer | null => {
  const player: FootballPlayerDto | undefined = playerLookup.get(entry.playerId);
  if (!player) return null;
  return {
    id: player.id,
    jersey: player.jerseyNumber,
    fullName: `${player.person.firstName} ${player.person.lastName}`,
    position: entry.position,
    isSentOff: entry.isSentOff,
  };
};

const RosterPanel = ({
  teamName,
  players,
  lineup,
  playersOnFieldRequired,
  requireGoalkeeper,
}: RosterPanelProps): ReactElement => {
  const { t } = useTranslation();

  const playerLookup: Map<string, FootballPlayerDto> = useMemo(() => {
    const map = new Map<string, FootballPlayerDto>();
    for (const player of players) {
      map.set(player.id, player);
    }
    return map;
  }, [players]);

  const { onField, bench, sentOff } = useMemo(() => {
    const mapChips = (entries: FootballLineupPlayer[]): ChipPlayer[] =>
      entries
        .map((entry) => toChip(entry, playerLookup))
        .filter((chip): chip is ChipPlayer => chip !== null)
        .sort(sortByJerseyThenName);

    return {
      onField: mapChips(getOnFieldPlayers(lineup)),
      bench: mapChips(getBenchPlayers(lineup)),
      sentOff: mapChips(getSentOffPlayers(lineup)),
    };
  }, [lineup, playerLookup]);

  const goalkeeperOnField: number = onField.filter(
    (p) => p.position === FootballPosition.Goalkeeper,
  ).length;
  const onFieldValid: boolean = onField.length === playersOnFieldRequired;
  const goalkeeperValid: boolean = !requireGoalkeeper || goalkeeperOnField === 1;

  return (
    <div className="arc-panel">
      <div className="arc-panel__header">
        <span className="arc-panel__team-name">{teamName}</span>
        <span className={`arc-panel__count${onFieldValid ? '' : ' arc-panel__count--invalid'}`}>
          {t('football.matches.lineup.onFieldCount', '{{current}} / {{required}} on field', {
            current: onField.length,
            required: playersOnFieldRequired,
          })}
        </span>
      </div>

      <div className="arc-panel__role-group">
        <div className="arc-panel__role-label">
          <span>{t('football.matches.lineup.onField', 'On field')}</span>
          <span className="arc-panel__role-count">{onField.length}</span>
        </div>
        <ChipsList
          players={onField}
          emptyLabel={t('football.matches.lineup.noOnField', 'No players on the field.')}
        />
      </div>

      <div className="arc-panel__role-group">
        <div className="arc-panel__role-label">
          <span>{t('football.matches.lineup.bench', 'Bench')}</span>
          <span className="arc-panel__role-count">{bench.length}</span>
        </div>
        <ChipsList
          players={bench}
          emptyLabel={t('football.matches.lineup.noBench', 'No players on the bench.')}
        />
      </div>

      {sentOff.length > 0 && (
        <div className="arc-panel__role-group">
          <div className="arc-panel__role-label">
            <span>{t('football.matches.lineup.sentOff', 'Sent off')}</span>
            <span className="arc-panel__role-count">{sentOff.length}</span>
          </div>
          <ChipsList
            players={sentOff}
            emptyLabel=""
            sentOff
          />
        </div>
      )}

      {requireGoalkeeper && !goalkeeperValid && (
        <div className="arc-panel__goalie arc-panel__goalie--missing">
          <div className="arc-panel__goalie-missing-text">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t(
              'football.matches.lineup.needGoalkeeper',
              'Exactly one goalkeeper must be on the field',
            )}
          </div>
        </div>
      )}
    </div>
  );
};

const ActiveRosterCard = ({
  leftTeamName,
  rightTeamName,
  leftPlayers,
  rightPlayers,
  leftLineup,
  rightLineup,
  matchRules,
  onEditLineup,
  disabled = false,
}: ActiveRosterCardProps): ReactElement => {
  const { t } = useTranslation();
  const rules = resolveMatchRules(matchRules);

  return (
    <section className="active-roster-card" aria-label={t('football.matches.lineup.activeLineup', 'Lineup')}>
      <div className="active-roster-card__header">
        <h3 className="active-roster-card__title">
          {t('football.matches.lineup.activeLineup', 'Lineup')}
        </h3>
        <button
          type="button"
          className="active-roster-card__edit"
          onClick={onEditLineup}
          disabled={disabled}
        >
          <i className="fas fa-pen" aria-hidden="true"></i>
          {t('football.matches.lineup.editLineup', 'Edit lineup')}
        </button>
      </div>

      <div className="active-roster-card__panels">
        <RosterPanel
          teamName={leftTeamName ?? t('football.matches.lineup.homeTeam', 'Home team')}
          players={leftPlayers}
          lineup={leftLineup}
          playersOnFieldRequired={rules.playersOnField}
          requireGoalkeeper={rules.requireGoalkeeper}
        />
        <RosterPanel
          teamName={rightTeamName ?? t('football.matches.lineup.awayTeam', 'Away team')}
          players={rightPlayers}
          lineup={rightLineup}
          playersOnFieldRequired={rules.playersOnField}
          requireGoalkeeper={rules.requireGoalkeeper}
        />
      </div>
    </section>
  );
};

export default ActiveRosterCard;
