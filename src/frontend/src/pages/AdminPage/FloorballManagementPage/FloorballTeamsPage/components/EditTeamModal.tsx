import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { getClubs, type Club } from '../../../../../api/clubService';
import type { 
  FloorballTeam, 
  FloorballTeamRequest, 
  FloorballTeamPlayer,
  UpdateFloorballTeamPlayerRequest
} from '../../../../../types/floorball/floorballTypes';
import { 
  FloorballDivision, 
  TeamCategory,
  FloorballPosition 
} from '../../../../../types/floorball/floorballTypes';
import TeamDetailsForm from './TeamDetailsForm';
import PlayerManagementTab from './PlayerManagementTab';

interface EditTeamModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (teamData: FloorballTeamRequest) => Promise<void>;
  teamId: string | null;
}

const EditTeamModal = ({ isOpen, onClose, onSubmit, teamId }: EditTeamModalProps) => {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [loadingTeam, setLoadingTeam] = useState(false);
  const [loadingPlayers, setLoadingPlayers] = useState(false);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [allPlayers, setAllPlayers] = useState<FloorballPlayerDto[]>([]);
  const [currentTeam, setCurrentTeam] = useState<FloorballTeam | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'players'>('details');
  const [playerEdits, setPlayerEdits] = useState<{[playerId: string]: {position?: FloorballPosition, jerseyNumber?: number, isActive?: boolean}}>({});
  const [removedPlayers, setRemovedPlayers] = useState<Set<string>>(new Set());
  const [addedPlayers, setAddedPlayers] = useState<Set<string>>(new Set());
  const [savingRoster, setSavingRoster] = useState(false);
  
  const [formData, setFormData] = useState<FloorballTeamRequest>({
    name: '',
    division: FloorballDivision.Premier,
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    category: TeamCategory.Adult,
    secondaryJerseyColor: ''
  });

  // Load team data when modal opens
  useEffect(() => {
    if (isOpen && teamId) {
      loadTeamData();
      loadClubs();
      loadAllPlayers();
    } else if (isOpen) {
      // Reset form for new team
      resetForm();
      loadClubs();
      loadAllPlayers();
    }
  }, [isOpen, teamId]); // eslint-disable-line react-hooks/exhaustive-deps

  const resetForm = () => {
    setFormData({
      name: '',
      division: FloorballDivision.Premier,
      clubId: '',
      homeArena: '',
      primaryJerseyColor: '#000000',
      category: TeamCategory.Adult,
      secondaryJerseyColor: ''
    });
    setCurrentTeam(null);
    setPlayerEdits({});
    setRemovedPlayers(new Set());
    setAddedPlayers(new Set());
  };

  const loadTeamData = async () => {
    if (!teamId) return;
    
    try {
      setLoadingTeam(true);
      const team = await floorballTeamService.getById(teamId);
      
      setCurrentTeam(team);
      setFormData({
        name: team.name,
        division: team.division,
        clubId: team.club.id,
        homeArena: team.homeArena,
        primaryJerseyColor: team.primaryJerseyColor,
        category: TeamCategory.Adult, // Default since it's not in the response
        secondaryJerseyColor: team.secondaryJerseyColor || ''
      });

      // Use the roster data from the team
      if (team.roster && team.roster.length > 0) {
        // Team has roster data available for editing
        console.log('Team roster loaded:', team.roster.length, 'players');
      }
    } catch (err) {
      console.error('Error loading team data:', err);
    } finally {
      setLoadingTeam(false);
    }
  };

  const loadClubs = async () => {
    try {
      const clubsData = await getClubs();
      setClubs(clubsData);
    } catch (err) {
      console.error('Error loading clubs:', err);
    }
  };

  const loadAllPlayers = async () => {
    try {
      setLoadingPlayers(true);
      
      const response = await floorballPlayerService.getAll({
        pageSize: 50 // Use max allowed page size
      });
      
      
      if (response && response.data && Array.isArray(response.data)) {
        setAllPlayers(response.data);
      } else {
        console.warn('Invalid players response format, setting empty array');
          setAllPlayers([]);
      }
    } catch (err) {
      console.error('Error loading players:', err);
        setAllPlayers([]);
    } finally {
      setLoadingPlayers(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      // Prepare update data with proper validation
      const updateData: FloorballTeamRequest = {
        name: formData.name,
        division: formData.division,
        clubId: formData.clubId,
        homeArena: formData.homeArena,
        primaryJerseyColor: formData.primaryJerseyColor,
        category: formData.category,
        // Only include secondaryJerseyColor if it's valid (2-50 characters) or omit it entirely
        ...(formData.secondaryJerseyColor && formData.secondaryJerseyColor.length >= 2 && formData.secondaryJerseyColor.length <= 50
          ? { secondaryJerseyColor: formData.secondaryJerseyColor }
          : {})
      };
      
      await onSubmit(updateData);
      onClose();
    } catch (error) {
      console.error('Error saving team:', error);
      // Don't close modal on error so user can see the issue and retry
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: keyof FloorballTeamRequest, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const addPlayerToTeam = (player: FloorballPlayerDto) => {
    // Add to local state
    setAddedPlayers(prev => new Set([...prev, player.id]));
    
    // Remove from removed players if it was there
    setRemovedPlayers(prev => {
      const newSet = new Set(prev);
      newSet.delete(player.id);
      return newSet;
    });
    
  };

  const removePlayerFromTeam = (playerId: string) => {
    // Add to removed players set
    setRemovedPlayers(prev => new Set([...prev, playerId]));
    
    // Remove from added players if it was there
    setAddedPlayers(prev => {
      const newSet = new Set(prev);
      newSet.delete(playerId);
      return newSet;
    });
    
    // Remove any edits for this player
    setPlayerEdits(prev => {
      const newEdits = { ...prev };
      delete newEdits[playerId];
      return newEdits;
    });
    
  };

  const updatePlayerPosition = (playerId: string, position: FloorballPosition) => {
    setPlayerEdits(prev => ({
      ...prev,
      [playerId]: {
        ...prev[playerId],
        position
      }
    }));
  };

  const updatePlayerJerseyNumber = (playerId: string, jerseyNumber: number | undefined) => {
    setPlayerEdits(prev => ({
      ...prev,
      [playerId]: {
        ...prev[playerId],
        jerseyNumber
      }
    }));
  };

  const togglePlayerActive = (playerId: string, isActive: boolean) => {
    setPlayerEdits(prev => ({
      ...prev,
      [playerId]: {
        ...prev[playerId],
        isActive
      }
    }));
  };

  const saveRosterChanges = async () => {
    if (!teamId) return;
    
    setSavingRoster(true);
    try {
      // 1. Handle removals
      for (const playerId of removedPlayers) {
        await floorballTeamService.removePlayerFromTeam(teamId, playerId);
      }

      // 2. Handle additions of new players
      const originalRosterPlayerIds = new Set(currentTeam?.roster?.map(p => p.playerId) || []);
      const playersToAdd = Array.from(addedPlayers).filter(playerId => !originalRosterPlayerIds.has(playerId));
      for (const playerId of playersToAdd) {
          const player = allPlayers.find(p => p.id === playerId);
          if (!player) continue;

          const edits = playerEdits[playerId];
          const position = edits?.position ?? (player.position as FloorballPosition) ?? FloorballPosition.None;
          const jerseyNumber = edits?.jerseyNumber;

          // Add the player with their initial edited properties
          await floorballTeamService.addPlayerToTeam(teamId, playerId, position, jerseyNumber);
      }

      // 3. Handle updates for ALL players with edits (both new and existing)
      // This is because addPlayerToTeam might not handle all editable properties (e.g., isActive).
      for (const [playerId, edits] of Object.entries(playerEdits)) {
          // Don't try to update a player that was removed in this transaction
          if (removedPlayers.has(playerId)) continue;

          // Get player's current state from the roster if they existed before,
          // otherwise get their base state from `allPlayers` list.
          const existingRosterPlayer = currentTeam?.roster?.find(p => p.playerId === playerId);
          const basePlayer = allPlayers.find(p => p.id === playerId);

          // We must have at least a base player to proceed
          if (!basePlayer) continue;

          const updateData: UpdateFloorballTeamPlayerRequest = {
              // Use edit if available, otherwise use existing state, otherwise use base state, otherwise default
              position: edits.position ?? existingRosterPlayer?.position ?? (basePlayer.position as FloorballPosition) ?? FloorballPosition.None,
              jerseyNumber: edits.jerseyNumber !== undefined ? edits.jerseyNumber : existingRosterPlayer?.jerseyNumber,
              isActive: edits.isActive !== undefined ? edits.isActive : existingRosterPlayer?.isActive ?? basePlayer.isActive,
          };

          await floorballTeamService.updateTeamPlayer(teamId, playerId, updateData);
      }
      
      // 4. All operations succeeded. Clear local state and call parent submit.
      setPlayerEdits({});
      setRemovedPlayers(new Set());
      setAddedPlayers(new Set());
      
      // This will trigger a refresh in the parent and close the modal.
      await onSubmit(formData);

    } catch (error) {
      console.error('Error saving roster changes:', error);
      throw error; // Let parent handle UI notification
    } finally {
      setSavingRoster(false);
    }
  };

  // Get current team roster (excluding removed players)
  const teamRoster = currentTeam?.roster?.filter(player => !removedPlayers.has(player.playerId)) || [];
  
  // Get the original team roster player IDs for comparison
  const originalRosterPlayerIds = new Set(currentTeam?.roster?.map(p => p.playerId) || []);
  
  // Add players that were added locally (but exclude players who were originally in the roster)
  const locallyAddedPlayers = Array.from(addedPlayers)
    .filter(playerId => !originalRosterPlayerIds.has(playerId)) // Only add truly new players
    .map(playerId => allPlayers.find(p => p.id === playerId))
    .filter(Boolean)
    .map(player => ({
      playerId: player!.id,
      playerName: player!.person.fullName,
      position: (player!.position as FloorballPosition) || FloorballPosition.None,
      jerseyNumber: undefined,
      isActive: player!.isActive
    } as FloorballTeamPlayer));
  
  const displayRoster = [...teamRoster, ...locallyAddedPlayers];
  
  // Filter available players (not currently in display roster)
  const availablePlayers = allPlayers.filter(player => 
    !displayRoster.find(rosterPlayer => rosterPlayer.playerId === player.id)
  );

  // Check if there are any pending changes
  const hasChanges = Object.keys(playerEdits).length > 0 || removedPlayers.size > 0 || addedPlayers.size > 0;

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content edit-team-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{teamId ? t('floorball.teams.editTeam', 'Edit Team') : t('floorball.teams.createNew', 'Create New Team')}</h2>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>

        {loadingTeam ? (
          <div className="loading-container">
            <p>{t('common.loading', 'Loading...')}</p>
          </div>
        ) : (
          <>
            {/* Tab Navigation */}
            <div className="tab-navigation">
              <button 
                className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
                onClick={() => setActiveTab('details')}
              >
                {t('floorball.teams.teamDetails', 'Team Details')}
              </button>
              <button 
                className={`tab-button ${activeTab === 'players' ? 'active' : ''}`}
                onClick={() => setActiveTab('players')}
                disabled={!teamId}
                title={!teamId ? t('floorball.teams.saveTeamFirst', 'Save team details first to manage roster') : undefined}
              >
                {t('floorball.teams.manageRoster', 'Manage Roster')} ({displayRoster.length})
              </button>
            </div>

            {/* Team Details Tab */}
            {activeTab === 'details' && (
              <TeamDetailsForm
                formData={formData}
                handleInputChange={handleInputChange}
                clubs={clubs}
                loading={loading}
                handleSubmit={handleSubmit}
                onClose={onClose}
              />
            )}

            {/* Players Management Tab */}
            {activeTab === 'players' && teamId && (
              <PlayerManagementTab
                displayRoster={displayRoster}
                availablePlayers={availablePlayers}
                allPlayers={allPlayers}
                playerEdits={playerEdits}
                removedPlayers={removedPlayers}
                addedPlayers={addedPlayers}
                loadingPlayers={loadingPlayers}
                savingRoster={savingRoster}
                hasChanges={hasChanges}
                onClose={onClose}
                saveRosterChanges={saveRosterChanges}
                loadAllPlayers={loadAllPlayers}
                addPlayerToTeam={addPlayerToTeam}
                removePlayerFromTeam={removePlayerFromTeam}
                updatePlayerPosition={updatePlayerPosition}
                updatePlayerJerseyNumber={updatePlayerJerseyNumber}
                togglePlayerActive={togglePlayerActive}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default EditTeamModal; 