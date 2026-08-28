import { useMemo } from 'react';
import type { ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyMatchActivePlayerDto } from '../../../../../types/hockey/hockeyTypes';
import './ActiveRosterCard.scss';

interface ActiveRosterCardProps {
  leftTeamName?: string;
  rightTeamName?: string;
  leftPlayers: HockeyMatchActivePlayerDto[];
  rightPlayers: HockeyMatchActivePlayerDto[];
  playerNames: Map<string, string>;
  leftGoalieId: string;
  rightGoalieId: string;
  onEditLineup: () => void;
  onSelectPlayer?: (playerId: string, side: 'left' | 'right') => void;
  selectedPlayerId?: string;
  disabled?: boolean;
}

interface ChipPlayer {
  id: string;
  jersey: number | undefined;
  fullName: string;
  isGoalie: boolean;
}

const sortByJerseyThenName = (a: ChipPlayer, b: ChipPlayer): number => {
  const numA = a.jersey ?? Number.MAX_SAFE_INTEGER;
  const numB = b.jersey ?? Number.MAX_SAFE_INTEGER;
  if (numA !== numB) {
    return numA - numB;
  }
  return a.fullName.localeCompare(b.fullName, undefined, { sensitivity: 'base' });
};

function toChips(players: HockeyMatchActivePlayerDto[], names: Map<string, string>): ChipPlayer[] {
  return players.map((player) => ({
    id: player.id,
    jersey: player.jerseyNumber,
    fullName: names.get(player.teamPlayerId) ?? `#${player.jerseyNumber}`,
    isGoalie: player.isGoalie || player.position === 'Goalie',
  })).sort(sortByJerseyThenName);
}

function ActiveRosterCard({
  leftTeamName,
  rightTeamName,
  leftPlayers,
  rightPlayers,
  playerNames,
  leftGoalieId,
  rightGoalieId,
  onEditLineup,
  onSelectPlayer,
  selectedPlayerId,
  disabled = false,
}: ActiveRosterCardProps): ReactElement {
  const { t } = useTranslation();
  const leftChips = useMemo(() => toChips(leftPlayers, playerNames), [leftPlayers, playerNames]);
  const rightChips = useMemo(() => toChips(rightPlayers, playerNames), [rightPlayers, playerNames]);

  const renderPanel = (teamName: string | undefined, chips: ChipPlayer[], goalieId: string, side: 'left' | 'right') => {
    const goalie = chips.find((player) => player.id === goalieId) ?? chips.find((player) => player.isGoalie);
    const skaters = chips.filter((player) => player.id !== goalie?.id);
    return (
      <div className="arc-panel">
        <h4 className="arc-panel__title">{teamName || t('hockey.matches.team', 'Team')}</h4>
        <div className="arc-panel__group">
          <div className="arc-panel__label">{t('hockey.matches.goalie', 'Goalie')}</div>
          {goalie ? (
            <ul className="arc-panel__chip-list">
              <li className="arc-chip">
                {goalie.jersey !== undefined && <span className="arc-chip__jersey">#{goalie.jersey}</span>}
                <span className="arc-chip__name">{goalie.fullName}</span>
              </li>
            </ul>
          ) : (
            <div className="arc-panel__empty">{t('hockey.matches.noGoalie', 'No goalie selected')}</div>
          )}
        </div>
        <div className="arc-panel__group">
          <div className="arc-panel__label">{t('hockey.matches.skaters', 'Skaters')}</div>
          {skaters.length === 0 ? (
            <div className="arc-panel__empty">{t('hockey.matches.confirmRosterFirst', 'Confirm the roster before recording events.')}</div>
          ) : (
            <ul className="arc-panel__chip-list">
              {skaters.map((player) => (
                <li key={player.id}>
                  <button
                    type="button"
                    className={`arc-chip${selectedPlayerId === player.id ? ' is-selected' : ''}`}
                    disabled={disabled || !onSelectPlayer}
                    onClick={() => onSelectPlayer?.(player.id, side)}
                  >
                    {player.jersey !== undefined && <span className="arc-chip__jersey">#{player.jersey}</span>}
                    <span className="arc-chip__name">{player.fullName}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    );
  };

  return (
    <section className="active-roster-card">
      <div className="active-roster-card__header">
        <h3 className="active-roster-card__title">{t('hockey.matches.activeRoster', 'ACTIVE ROSTER')}</h3>
        <button type="button" className="active-roster-card__edit" onClick={onEditLineup} disabled={disabled}>
          {t('hockey.matches.editLineup', 'Edit lineup')}
        </button>
      </div>
      <div className="active-roster-card__panels">
        {renderPanel(leftTeamName, leftChips, leftGoalieId, 'left')}
        {renderPanel(rightTeamName, rightChips, rightGoalieId, 'right')}
      </div>
    </section>
  );
}

export default ActiveRosterCard;
