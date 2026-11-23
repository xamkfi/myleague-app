import { useMemo } from 'react';
import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchRules,
} from '../../../../../types/football/footballTypes';
import { findLineupProblem, getSentOffPlayers, resolveMatchRules, type LineupProblem } from '../utils/lineupValidation';
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
  rules: FootballMatchRules;
}

interface ChipPlayer {
  id: string;
  jersey: number | undefined;
  fullName: string;
  position: FootballPosition;
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
  const { t } = useTranslation();
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
            <span className="arc-chip__role">{t('football.matches.lineup.goalkeeperShort', 'GK')}</span>
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
  };
};

const RosterPanel = ({ teamName, players, lineup, rules }: RosterPanelProps): ReactElement => {
  const { t } = useTranslation();

  const { inMatch, sentOff } = useMemo(() => {
    const playerLookup = new Map(players.map((player) => [player.id, player] as const));
    const mapChips = (entries: FootballLineupPlayer[]): ChipPlayer[] =>
      entries
        .map((entry) => toChip(entry, playerLookup))
        .filter((chip): chip is ChipPlayer => chip !== null)
        .sort(sortByJerseyThenName);

    return {
      inMatch: mapChips(lineup.filter((entry) => !entry.isSentOff)),
      sentOff: mapChips(getSentOffPlayers(lineup)),
    };
  }, [lineup, players]);

  const problem: LineupProblem | null = findLineupProblem(lineup, rules);

  return (
    <div className="arc-panel">
      <div className="arc-panel__header">
        <span className="arc-panel__team-name">{teamName}</span>
        <span className="arc-panel__count">
          {t('football.matches.lineup.playerCount', '{{count}} players', { count: lineup.length })}
        </span>
      </div>

      <div className="arc-panel__role-group">
        <ChipsList
          players={inMatch}
          emptyLabel={t('football.matches.lineup.noPlayers', 'The lineup has not been set yet.')}
        />
      </div>

      {sentOff.length > 0 && (
        <div className="arc-panel__role-group">
          <div className="arc-panel__role-label">
            <span>{t('football.matches.lineup.sentOff', 'Sent off')}</span>
            <span className="arc-panel__role-count">{sentOff.length}</span>
          </div>
          <ChipsList players={sentOff} emptyLabel="" sentOff />
        </div>
      )}

      {problem === 'needGoalkeeper' && (
        <div className="arc-panel__goalie arc-panel__goalie--missing">
          <div className="arc-panel__goalie-missing-text">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t('football.matches.lineup.needGoalkeeper', 'Choose at least one goalkeeper')}
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
          title={disabled ? t('football.matches.lineup.lockedHint', 'Reopen the match to edit the lineup.') : undefined}
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
          rules={rules}
        />
        <RosterPanel
          teamName={rightTeamName ?? t('football.matches.lineup.awayTeam', 'Away team')}
          players={rightPlayers}
          lineup={rightLineup}
          rules={rules}
        />
      </div>
    </section>
  );
};

export default ActiveRosterCard;
