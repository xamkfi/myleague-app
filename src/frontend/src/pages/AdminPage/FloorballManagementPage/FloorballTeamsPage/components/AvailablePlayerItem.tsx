import React from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';

interface AvailablePlayerItemProps {
  player: FloorballPlayerDto;
  addPlayerToTeam: (player: FloorballPlayerDto) => void;
}

const AvailablePlayerItem = ({ player, addPlayerToTeam }: AvailablePlayerItemProps) => {
  const { t } = useTranslation();

  return (
    <div className="player-item">
      <div className="player-info">
        <span className="player-name">{player.person.fullName}</span>
        {player.position && (
          <span className="player-position">
            {t(`floorball.positions.${player.position.toLowerCase()}`, player.position)}
          </span>
        )}
        <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
          {player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
        </span>
      </div>
      <button
        className="add-button"
        onClick={() => addPlayerToTeam(player)}
        title={t('floorball.teams.addToRoster', 'Add to roster')}
      >
        +
      </button>
    </div>
  );
};

export default AvailablePlayerItem; 