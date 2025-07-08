import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import RosterPlayerItem from './RosterPlayerItem';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballTeamPlayer, FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import './PlayerTransferList.scss';

interface PlayerTransferListProps {
  displayRoster: FloorballTeamPlayer[];
  availablePlayers: FloorballPlayerDto[];
  allPlayers: FloorballPlayerDto[];
  playerEdits: { [playerId: string]: { position?: FloorballPosition; jerseyNumber?: number; isActive?: boolean; } };
  removedPlayers: Set<string>;
  addedPlayers: Set<string>;
  loadingPlayers: boolean;
  addPlayerToTeam: (player: FloorballPlayerDto) => void;
  removePlayerFromTeam: (playerId: string) => void;
  updatePlayerPosition: (playerId: string, position: FloorballPosition) => void;
  updatePlayerJerseyNumber: (playerId: string, jerseyNumber: number | undefined) => void;
  togglePlayerActive: (playerId: string, isActive: boolean) => void;
  loadAllPlayers: () => void;
}

const PlayerTransferList = ({
  displayRoster,
  availablePlayers,
  allPlayers,
  playerEdits,
  removedPlayers,
  addedPlayers,
  loadingPlayers,
  addPlayerToTeam,
  removePlayerFromTeam,
  updatePlayerPosition,
  updatePlayerJerseyNumber,
  togglePlayerActive,
  loadAllPlayers
}: PlayerTransferListProps) => {
  const { t } = useTranslation();
  const [selectedAvailablePlayers, setSelectedAvailablePlayers] = useState<Set<string>>(new Set());
  const [selectedRosterPlayers, setSelectedRosterPlayers] = useState<Set<string>>(new Set());

  // Search terms
  const [availableSearch, setAvailableSearch] = useState('');
  const [rosterSearch, setRosterSearch] = useState('');

  // Filtered player arrays based on search terms
  const filteredAvailablePlayers = availablePlayers.filter(p =>
    p.person.fullName.toLowerCase().includes(availableSearch.toLowerCase())
  );

  const filteredRoster = displayRoster.filter(p =>
    p.playerName?.toLowerCase().includes(rosterSearch.toLowerCase()) ||
    // fallback: look into allPlayers for full name
    (allPlayers.find(ap => ap.id === p.playerId)?.person.fullName.toLowerCase() || '')
      .includes(rosterSearch.toLowerCase())
  );

  const handleSelectAvailablePlayer = (playerId: string, isSelected: boolean) => {
    setSelectedAvailablePlayers(prev => {
      const newSet = new Set(prev);
      if (isSelected) {
        newSet.add(playerId);
      } else {
        newSet.delete(playerId);
      }
      return newSet;
    });
  };

  const handleSelectRosterPlayer = (playerId: string, isSelected: boolean) => {
    setSelectedRosterPlayers(prev => {
      const newSet = new Set(prev);
      if (isSelected) {
        newSet.add(playerId);
      } else {
        newSet.delete(playerId);
      }
      return newSet;
    });
  };

  const movePlayersToRoster = () => {
    selectedAvailablePlayers.forEach(playerId => {
      const player = availablePlayers.find(p => p.id === playerId);
      if (player) {
        addPlayerToTeam(player);
      }
    });
    setSelectedAvailablePlayers(new Set());
  };

  const movePlayersToAvailable = () => {
    selectedRosterPlayers.forEach(playerId => {
      removePlayerFromTeam(playerId);
    });
    setSelectedRosterPlayers(new Set());
  };

  const selectAllAvailable = () => {
    setSelectedAvailablePlayers(new Set(availablePlayers.map(p => p.id)));
  };

  const selectAllRoster = () => {
    setSelectedRosterPlayers(new Set(displayRoster.map(p => p.playerId)));
  };

  const clearAvailableSelection = () => {
    setSelectedAvailablePlayers(new Set());
  };

  const clearRosterSelection = () => {
    setSelectedRosterPlayers(new Set());
  };

  return (
    <div className="player-transfer-list">
      {/* Available Players Panel */}
      <div className="transfer-panel available-panel">
        <div className="panel-header">
          <h3>{t('floorball.teams.availablePlayers', 'Available Players')}</h3>
          <span className="player-count">({availablePlayers.length})</span>
        </div>
        
        <div className="panel-controls">
          <input
            type="text"
            placeholder={t('common.search', 'Search') as string}
            value={availableSearch}
            onChange={e => setAvailableSearch(e.target.value)}
            className="players-search"
          />
          <button 
            className="select-all-btn"
            onClick={selectAllAvailable}
            disabled={availablePlayers.length === 0}
          >
            {t('common.selectAll', 'Select All')}
          </button>
          <button 
            className="clear-selection-btn"
            onClick={clearAvailableSelection}
            disabled={selectedAvailablePlayers.size === 0}
          >
            {t('common.clearSelection', 'Clear')}
          </button>
        </div>

        <div className="players-list">
          {loadingPlayers ? (
            <div className="loading-state">
              <p>{t('common.loading', 'Loading...')}</p>
            </div>
          ) : filteredAvailablePlayers.length === 0 ? (
            <div className="empty-state">
              <p>{t('floorball.teams.noAvailablePlayers', 'No available players')}</p>
              {allPlayers.length === 0 && (
                <button
                  className="refresh-button"
                  onClick={loadAllPlayers}
                  disabled={loadingPlayers}
                >
                  {t('common.retry', 'Retry')}
                </button>
              )}
            </div>
          ) : (
            filteredAvailablePlayers.map(player => (
              <div 
                key={player.id}
                className={`available-player-item ${selectedAvailablePlayers.has(player.id) ? 'selected' : ''}`}
                onClick={() => handleSelectAvailablePlayer(player.id, !selectedAvailablePlayers.has(player.id))}
              >
                
                <div className="player-info">
                  <span className="player-name">{player.person.fullName}</span>
                  <div className="player-details">
                    <span className="player-position">
                      {player.position ? t(`floorball.positions.${player.position.toLowerCase()}`, player.position) : t('floorball.positions.none', 'None')}
                    </span>
                    <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                      {player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                    </span>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Transfer Controls */}
      <div className="transfer-controls">
        <button
          className="transfer-btn add-to-roster"
          onClick={movePlayersToRoster}
          disabled={selectedAvailablePlayers.size === 0}
          title={t('floorball.teams.addToRoster', 'Add to roster')}
        >
          <span className="arrow">→</span>
          <span className="count">({selectedAvailablePlayers.size})</span>
        </button>
        
        <button
          className="transfer-btn remove-from-roster"
          onClick={movePlayersToAvailable}
          disabled={selectedRosterPlayers.size === 0}
          title={t('floorball.teams.removeFromRoster', 'Remove from roster')}
        >
          <span className="arrow">←</span>
          <span className="count">({selectedRosterPlayers.size})</span>
        </button>
      </div>

      {/* Current Roster Panel */}
      <div className="transfer-panel roster-panel">
        <div className="panel-header">
          <h3>{t('floorball.teams.currentRoster', 'Current Roster')}</h3>
          <span className="player-count">({displayRoster.length})</span>
        </div>
        
        <div className="panel-controls">
          <input
            type="text"
            placeholder={t('common.search', 'Search') as string}
            value={rosterSearch}
            onChange={e => setRosterSearch(e.target.value)}
            className="players-search"
          />
          <button 
            className="select-all-btn"
            onClick={selectAllRoster}
            disabled={displayRoster.length === 0}
          >
            {t('common.selectAll', 'Select All')}
          </button>
          <button 
            className="clear-selection-btn"
            onClick={clearRosterSelection}
            disabled={selectedRosterPlayers.size === 0}
          >
            {t('common.clearSelection', 'Clear')}
          </button>
        </div>

        <div className="players-list">
          {filteredRoster.length === 0 ? (
            <div className="empty-state">
              <p>{t('floorball.teams.noPlayersInRoster', 'No players in roster')}</p>
            </div>
          ) : (
            filteredRoster.map(player => (
              <div 
                key={player.playerId}
                className={`roster-player-container ${selectedRosterPlayers.has(player.playerId) ? 'selected' : ''}`}
                onClick={(e) => {
                  // Don't trigger when clicking on form controls
                  if (e.target instanceof HTMLInputElement || e.target instanceof HTMLSelectElement || e.target instanceof HTMLButtonElement) {
                    return;
                  }
                  handleSelectRosterPlayer(player.playerId, !selectedRosterPlayers.has(player.playerId));
                }}
              >
                
                <div className="roster-player-content">
                  <RosterPlayerItem
                    player={player}
                    allPlayers={allPlayers}
                    edits={playerEdits[player.playerId] || {}}
                    isMarkedForRemoval={removedPlayers.has(player.playerId)}
                    isNewlyAdded={addedPlayers.has(player.playerId) && !removedPlayers.has(player.playerId)}
                    removePlayerFromTeam={removePlayerFromTeam}
                    updatePlayerPosition={updatePlayerPosition}
                    updatePlayerJerseyNumber={updatePlayerJerseyNumber}
                    togglePlayerActive={togglePlayerActive}
                  />
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default PlayerTransferList; 