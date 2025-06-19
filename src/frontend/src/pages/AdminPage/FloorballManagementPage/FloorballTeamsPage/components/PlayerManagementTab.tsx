import { useTranslation } from 'react-i18next';
import RosterPlayerItem from './RosterPlayerItem';
import AvailablePlayerItem from './AvailablePlayerItem';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballTeamPlayer, FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import './PlayerManagementTab.scss';

interface PlayerManagementTabProps {
  displayRoster: FloorballTeamPlayer[];
  availablePlayers: FloorballPlayerDto[];
  allPlayers: FloorballPlayerDto[];
  playerEdits: { [playerId: string]: { position?: FloorballPosition; jerseyNumber?: number; isActive?: boolean; } };
  removedPlayers: Set<string>;
  addedPlayers: Set<string>;
  loadingPlayers: boolean;
  savingRoster: boolean;
  hasChanges: boolean;
  onClose: () => void;
  saveRosterChanges: () => Promise<void>;
  loadAllPlayers: () => void;
  addPlayerToTeam: (player: FloorballPlayerDto) => void;
  removePlayerFromTeam: (playerId: string) => void;
  updatePlayerPosition: (playerId: string, position: FloorballPosition) => void;
  updatePlayerJerseyNumber: (playerId: string, jerseyNumber: number | undefined) => void;
  togglePlayerActive: (playerId: string, isActive: boolean) => void;
}

const PlayerManagementTab = ({
  displayRoster,
  availablePlayers,
  allPlayers,
  playerEdits,
  removedPlayers,
  addedPlayers,
  loadingPlayers,
  savingRoster,
  hasChanges,
  onClose,
  saveRosterChanges,
  loadAllPlayers,
  addPlayerToTeam,
  removePlayerFromTeam,
  updatePlayerPosition,
  updatePlayerJerseyNumber,
  togglePlayerActive
}: PlayerManagementTabProps) => {
  const { t } = useTranslation();

  return (
    <div className="players-management">
      <div className="players-section">
        <div className="current-roster">
          <h3>{t('floorball.teams.currentRoster', 'Current Roster')} ({displayRoster.length})</h3>
          <div className="players-list">
            {displayRoster.length === 0 ? (
              <p className="no-players">{t('floorball.teams.noPlayersInRoster', 'No players in roster')}</p>
            ) : (
              displayRoster.map(player => (
                <RosterPlayerItem
                  key={player.playerId}
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
              ))
            )}
          </div>
        </div>

        <div className="available-players">
          <h3>{t('floorball.teams.availablePlayers', 'Available Players')} ({availablePlayers.length})</h3>
          {loadingPlayers ? (
            <p>{t('common.loading', 'Loading...')}</p>
          ) : (
            <>
              {allPlayers.length === 0 && (
                <div className="no-players-error">
                  <p>{t('floorball.teams.noAvailablePlayers', 'No available players')}</p>
                  <button
                    className="refresh-button"
                    onClick={loadAllPlayers}
                    disabled={loadingPlayers}
                  >
                    {loadingPlayers ? t('common.loading', 'Loading...') : t('common.retry', 'Retry')}
                  </button>
                </div>
              )}
              <div className="players-list">
                {availablePlayers.length === 0 && allPlayers.length > 0 ? (
                  <p className="no-players">{t('floorball.teams.noAvailablePlayers', 'All players are already assigned to teams')}</p>
                ) : (
                  availablePlayers.map(player => (
                    <AvailablePlayerItem
                      key={player.id}
                      player={player}
                      addPlayerToTeam={addPlayerToTeam}
                    />
                  ))
                )}
              </div>
            </>
          )}
        </div>
      </div>

      <div className="form-actions">
        <button type="button" onClick={onClose} className="cancel-button">
          {t('common.cancel', 'Cancel')}
        </button>
        <button
          type="button"
          onClick={saveRosterChanges}
          className="submit-button"
          disabled={savingRoster || !hasChanges}
        >
          {savingRoster
            ? t('common.saving', 'Saving...')
            : t('floorball.teams.saveRoster', 'Save Roster & Continue')
          }
        </button>
      </div>
    </div>
  );
};

export default PlayerManagementTab; 