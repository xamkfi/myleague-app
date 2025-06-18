import React from 'react';
import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { FloorballPosition, type FloorballTeamPlayer } from '../../../../../types/floorball/floorballTypes';
import './RosterPlayerItem.scss';

interface RosterPlayerItemProps {
  player: FloorballTeamPlayer;
  allPlayers: FloorballPlayerDto[];
  edits: {
    position?: FloorballPosition;
    jerseyNumber?: number;
    isActive?: boolean;
  };
  isMarkedForRemoval: boolean;
  isNewlyAdded: boolean;
  updatePlayerPosition: (playerId: string, position: FloorballPosition) => void;
  updatePlayerJerseyNumber: (playerId: string, jerseyNumber: number | undefined) => void;
  togglePlayerActive: (playerId: string, isActive: boolean) => void;
  removePlayerFromTeam: (playerId: string) => void;
}

const RosterPlayerItem = ({
  player,
  allPlayers,
  edits,
  isMarkedForRemoval,
  isNewlyAdded,
  updatePlayerPosition,
  updatePlayerJerseyNumber,
  togglePlayerActive,
  removePlayerFromTeam
}: RosterPlayerItemProps) => {
  const { t } = useTranslation();

  const currentPosition = edits.position || player.position;
  const currentJerseyNumber = edits.jerseyNumber !== undefined
    ? edits.jerseyNumber
    : player.jerseyNumber;
  const currentIsActive = edits.isActive !== undefined
    ? edits.isActive
    : player.isActive;

  let playerName = player.playerName;
  if (!playerName || playerName === '' || playerName === 'Unknown Player') {
    const playerDetails = allPlayers.find(p => p.id === player.playerId);
    playerName = playerDetails?.person?.fullName || 'Unknown Player';
  }

  const itemClasses = [
    'player-item',
    'editable',
    isMarkedForRemoval ? 'marked-for-removal' : '',
    isNewlyAdded ? 'newly-added' : '',
  ].filter(Boolean).join(' ');

  return (
    <div className={itemClasses}>
      <div className="player-info">
        <div style={{ display: 'flex', alignItems: 'center' }}>
          <span className="player-name">{playerName}</span>
        </div>
        
        <div className="player-controls">
          <div className="position-control">
            <label>{t('floorball.players.position', 'Position')}:</label>
            <select 
              value={currentPosition}
              onChange={(e) => updatePlayerPosition(player.playerId, e.target.value as FloorballPosition)}
              className="position-select"
            >
              <option value={FloorballPosition.None}>{t('floorball.positions.none', 'None')}</option>
              <option value={FloorballPosition.Goalkeeper}>{t('floorball.positions.goalkeeper', 'Goalkeeper')}</option>
              <option value={FloorballPosition.Defender}>{t('floorball.positions.defender', 'Defender')}</option>
              <option value={FloorballPosition.Forward}>{t('floorball.positions.forward', 'Forward')}</option>
            </select>
          </div>
          
          <div className="jersey-control">
            <label>{t('floorball.players.jerseyNumber', 'Jersey #')}:</label>
            <input 
              type="number" 
              min="0" 
              max="99"
              value={currentJerseyNumber || ''} 
              onChange={(e) => updatePlayerJerseyNumber(player.playerId, e.target.value ? parseInt(e.target.value) : undefined)}
              placeholder="#"
              className="jersey-input"
            />
          </div>
          
          <div className="active-control">
            <label>{t('floorball.players.status', 'Status')}:</label>
            <button
              type="button"
              className={`active-toggle ${currentIsActive ? 'active' : 'inactive'}`}
              onClick={() => togglePlayerActive(player.playerId, !currentIsActive)}
              title={currentIsActive ? t('floorball.players.setInactive', 'Set Inactive') : t('floorball.players.setActive', 'Set Active')}
            >
              {currentIsActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
            </button>
          </div>
        </div>
      </div>
      <button
        className="remove-button"
        onClick={() => removePlayerFromTeam(player.playerId)}
        title={t('floorball.teams.removeFromRoster', 'Remove from roster')}
      >
        ✕
      </button>
    </div>
  );
};

export default RosterPlayerItem; 