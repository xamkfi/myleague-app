import { useMemo } from 'react';
import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import {
  FloorballPosition,
  type FloorballActiveLineupPlayer,
} from '../../../../../types/floorball/floorballTypes';
import './ActiveRosterCard.scss';

interface ActiveRosterCardProps {
  leftTeamName?: string;
  rightTeamName?: string;
  leftPlayers: FloorballPlayerDto[];
  rightPlayers: FloorballPlayerDto[];
  leftLineup: FloorballActiveLineupPlayer[];
  rightLineup: FloorballActiveLineupPlayer[];
  leftGoalieId: string;
  rightGoalieId: string;
  onEditLineup: () => void;
  disabled?: boolean;
}

interface RosterPanelProps {
  teamName: string;
  players: FloorballPlayerDto[];
  lineup: FloorballActiveLineupPlayer[];
  goalieId: string;
}

interface ChipPlayer {
  id: string;
  jersey: number | undefined;
  fullName: string;
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
}

const ChipsList = ({ players, emptyLabel }: ChipsListProps): ReactElement => {
  if (players.length === 0) {
    return <div className="arc-panel__empty">{emptyLabel}</div>;
  }

  return (
    <ul className="arc-panel__chip-list">
      {players.map((p: ChipPlayer) => (
        <li key={p.id} className="arc-chip">
          {p.jersey !== undefined && <span className="arc-chip__jersey">#{p.jersey}</span>}
          <span className="arc-chip__name">{p.fullName}</span>
        </li>
      ))}
    </ul>
  );
};

const RosterPanel = ({ teamName, players, lineup, goalieId }: RosterPanelProps): ReactElement => {
  const { t } = useTranslation();

  const playerLookup: Map<string, FloorballPlayerDto> = useMemo(() => {
    const map = new Map<string, FloorballPlayerDto>();
    for (const player of players) {
      map.set(player.id, player);
    }
    return map;
  }, [players]);

  const { defenders, forwards, totalCount } = useMemo(() => {
    const ds: ChipPlayer[] = [];
    const fs: ChipPlayer[] = [];

    for (const entry of lineup) {
      const player: FloorballPlayerDto | undefined = playerLookup.get(entry.playerId);
      if (!player) continue;

      const chip: ChipPlayer = {
        id: player.id,
        jersey: player.jerseyNumber,
        fullName: `${player.person.firstName} ${player.person.lastName}`,
      };

      // Treat backend Center as Forward in this two-bucket UI to keep things simple.
      if (entry.position === FloorballPosition.Defender) {
        ds.push(chip);
      } else {
        fs.push(chip);
      }
    }

    ds.sort(sortByJerseyThenName);
    fs.sort(sortByJerseyThenName);

    return { defenders: ds, forwards: fs, totalCount: ds.length + fs.length };
  }, [lineup, playerLookup]);

  const goalie: FloorballPlayerDto | undefined = goalieId ? playerLookup.get(goalieId) : undefined;

  return (
    <div className="arc-panel">
      <div className="arc-panel__header">
        <span className="arc-panel__team-name">{teamName}</span>
        <span className="arc-panel__count">
          {t('floorball.matches.lineup.playerCount', '{{count}} players', { count: totalCount })}
        </span>
      </div>

      <div className="arc-panel__role-group">
        <div className="arc-panel__role-label">
          <span>{t('floorball.matches.lineup.defenders', 'Defenders')}</span>
          <span className="arc-panel__role-count">{defenders.length}</span>
        </div>
        <ChipsList
          players={defenders}
          emptyLabel={t('floorball.matches.lineup.noDefenders', 'No defenders selected.')}
        />
      </div>

      <div className="arc-panel__role-group">
        <div className="arc-panel__role-label">
          <span>{t('floorball.matches.lineup.forwards', 'Forwards')}</span>
          <span className="arc-panel__role-count">{forwards.length}</span>
        </div>
        <ChipsList
          players={forwards}
          emptyLabel={t('floorball.matches.lineup.noForwards', 'No forwards selected.')}
        />
      </div>

      <div className={`arc-panel__goalie ${goalie ? '' : 'arc-panel__goalie--missing'}`}>
        <div className="arc-panel__goalie-label">
          <span className="arc-panel__goalie-icon" aria-hidden="true">
            <i className="fas fa-shield-alt"></i>
          </span>
          {t('floorball.matches.lineup.goalkeeper', 'Goalkeeper')}
        </div>
        {goalie ? (
          <div className="arc-panel__goalie-name">
            {goalie.jerseyNumber !== undefined && (
              <span className="arc-panel__goalie-jersey">#{goalie.jerseyNumber}</span>
            )}
            <span>
              {goalie.person.firstName} {goalie.person.lastName}
            </span>
          </div>
        ) : (
          <div className="arc-panel__goalie-missing-text">
            <i className="fas fa-exclamation-triangle" aria-hidden="true"></i>
            {t('floorball.matches.lineup.noGoalkeeper', 'No goalkeeper selected (required to start match)')}
          </div>
        )}
      </div>
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
  leftGoalieId,
  rightGoalieId,
  onEditLineup,
  disabled = false,
}: ActiveRosterCardProps): ReactElement => {
  const { t } = useTranslation();

  return (
    <section className="active-roster-card" aria-label={t('floorball.matches.lineup.activeLineup', 'Active lineup')}>
      <div className="active-roster-card__header">
        <h3 className="active-roster-card__title">
          {t('floorball.matches.lineup.activeLineup', 'Active lineup')}
        </h3>
        <button
          type="button"
          className="active-roster-card__edit"
          onClick={onEditLineup}
          disabled={disabled}
        >
          <i className="fas fa-pen" aria-hidden="true"></i>
          {t('floorball.matches.lineup.editLineup', 'Edit lineup')}
        </button>
      </div>

      <div className="active-roster-card__panels">
        <RosterPanel
          teamName={leftTeamName ?? t('floorball.matches.lineup.homeTeam', 'Home team')}
          players={leftPlayers}
          lineup={leftLineup}
          goalieId={leftGoalieId}
        />
        <RosterPanel
          teamName={rightTeamName ?? t('floorball.matches.lineup.awayTeam', 'Away team')}
          players={rightPlayers}
          lineup={rightLineup}
          goalieId={rightGoalieId}
        />
      </div>
    </section>
  );
};

export default ActiveRosterCard;
