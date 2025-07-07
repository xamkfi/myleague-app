import { useTranslation } from 'react-i18next';
import PlayerTransferList from './PlayerTransferList';
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
      <PlayerTransferList
        displayRoster={displayRoster}
        availablePlayers={availablePlayers}
        allPlayers={allPlayers}
        playerEdits={playerEdits}
        removedPlayers={removedPlayers}
        addedPlayers={addedPlayers}
        loadingPlayers={loadingPlayers}
        addPlayerToTeam={addPlayerToTeam}
        removePlayerFromTeam={removePlayerFromTeam}
        updatePlayerPosition={updatePlayerPosition}
        updatePlayerJerseyNumber={updatePlayerJerseyNumber}
        togglePlayerActive={togglePlayerActive}
        loadAllPlayers={loadAllPlayers}
      />

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