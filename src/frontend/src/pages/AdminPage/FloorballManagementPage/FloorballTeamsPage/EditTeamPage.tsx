import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import BackButton from '../../../../components/BackButton/BackButton';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import { getClubs, type Club } from '../../../../api/common/clubService';
import { divisionService } from '../../../../api/common/divisionService';
import { 
  FloorballPosition,
  TeamCategory,
  type FloorballTeam, 
  type FloorballTeamRequest, 
  type FloorballTeamPlayer,
  type UpdateFloorballTeamPlayerRequest
} from '../../../../types/floorball/floorballTypes';
import type { DivisionType } from '../../../../types/common/divisionType';
import './EditTeamPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

const EditTeamPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(false);
  const [loadingTeam, setLoadingTeam] = useState(true);
  const [loadingPlayers, setLoadingPlayers] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const [allPlayers, setAllPlayers] = useState<FloorballPlayerDto[]>([]);
  const [currentTeam, setCurrentTeam] = useState<FloorballTeam | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'players'>('details');
  const [playerEdits, setPlayerEdits] = useState<{[playerId: string]: {position?: FloorballPosition, jerseyNumber?: number, isActive?: boolean}}>({});
  const [removedPlayers, setRemovedPlayers] = useState<Set<string>>(new Set());
  const [addedPlayers, setAddedPlayers] = useState<Set<string>>(new Set());
  const [savingRoster, setSavingRoster] = useState(false);
  
  // Search and selection state
  const [availablePlayersSearch, setAvailablePlayersSearch] = useState('');
  const [rosterPlayersSearch, setRosterPlayersSearch] = useState('');
  const [selectedAvailablePlayers, setSelectedAvailablePlayers] = useState<Set<string>>(new Set());
  const [selectedRosterPlayers, setSelectedRosterPlayers] = useState<Set<string>>(new Set());
  
  const [formData, setFormData] = useState<FloorballTeamRequest>({
    name: '',
    divisionId: '',
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    category: 'Adult' as TeamCategory,
    secondaryJerseyColor: ''
  });
  
  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoadingTeam(true);
      const team = await floorballTeamService.getById(teamId);
      
      setCurrentTeam(team);
      setFormData({
        name: team.name,
        divisionId: team.divisionId,
        clubId: team.club.id,
        homeArena: team.homeArena,
        primaryJerseyColor: team.primaryJerseyColor,
        category: 'Adult' as TeamCategory, // Default since it's not in the response
        secondaryJerseyColor: team.secondaryJerseyColor || ''
      });

      console.log('Team loaded:', team);
    } catch (err) {
      console.error('Error loading team data:', err);
      setError('Failed to load team data');
    } finally {
      setLoadingTeam(false);
    }
  }, [teamId]);

  // Load team data when component mounts
  useEffect(() => {
    if (teamId) {
      loadTeamData();
      loadClubs();
      loadDivisions();
      loadAllPlayers();
    }
  }, [teamId, loadTeamData]);

  const loadClubs = async () => {
    try {
      const clubsData = await getClubs();
      setClubs(clubsData);
    } catch (err) {
      console.error('Error loading clubs:', err);
    }
  };

  const loadDivisions = async () => {
    try {
      const response = await divisionService.getAll();
      setDivisions(response.data);
    } catch (err) {
      console.error('Error loading divisions:', err);
      setDivisions([]);
    }
  };

  const loadAllPlayers = async () => {
    try {
      setLoadingPlayers(true);
      
      const pageSize = 50;
      let currentPage = 1;
      let combined: FloorballPlayerDto[] = [];

      while (true) {
        const resp = await floorballPlayerService.getAll({ page: currentPage, pageSize });

        if (resp?.data && Array.isArray(resp.data)) {
          combined = combined.concat(resp.data);
          if (resp.data.length < pageSize) {
            break;
          }
        } else {
          console.warn('Unexpected players response format on page', currentPage);
          break;
        }

        currentPage += 1;
      }

      setAllPlayers(combined);
    } catch (err) {
      console.error('Error loading players:', err);
      setAllPlayers([]);
    } finally {
      setLoadingPlayers(false);
    }
  };

  const handleInputChange = (field: keyof FloorballTeamRequest, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    
    try {
      // Prepare update data with proper validation
      const updateData: FloorballTeamRequest = {
        name: formData.name,
        divisionId: formData.divisionId,
        clubId: formData.clubId,
        homeArena: formData.homeArena,
        primaryJerseyColor: formData.primaryJerseyColor,
        category: formData.category,
        // Only include secondaryJerseyColor if it's valid (2-50 characters) or omit it entirely
        ...(formData.secondaryJerseyColor && formData.secondaryJerseyColor.length >= 2 && formData.secondaryJerseyColor.length <= 50
          ? { secondaryJerseyColor: formData.secondaryJerseyColor }
          : {})
      };
      
      await floorballTeamService.update(teamId!, updateData);
      
      // Navigate back to teams list
      navigate('/admin/floorball/teams');
    } catch (error) {
      console.error('Error saving team:', error);
      setError(error instanceof Error ? error.message : 'Failed to save team');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/admin/floorball/teams');
  };

  // Player management functions
  const addPlayerToTeam = (player: FloorballPlayerDto) => {
    setAddedPlayers(prev => new Set([...prev, player.id]));
    setRemovedPlayers(prev => {
      const newSet = new Set(prev);
      newSet.delete(player.id);
      return newSet;
    });
  };

  const removePlayerFromTeam = (playerId: string) => {
    setRemovedPlayers(prev => new Set([...prev, playerId]));
    setAddedPlayers(prev => {
      const newSet = new Set(prev);
      newSet.delete(playerId);
      return newSet;
    });
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

        await floorballTeamService.addPlayerToTeam(teamId, playerId, position, jerseyNumber);
      }

      // 3. Handle updates for ALL players with edits
      for (const [playerId, edits] of Object.entries(playerEdits)) {
        if (removedPlayers.has(playerId)) continue;

        const existingRosterPlayer = currentTeam?.roster?.find(p => p.playerId === playerId);
        const basePlayer = allPlayers.find(p => p.id === playerId);

        if (!basePlayer) continue;

        const updateData: UpdateFloorballTeamPlayerRequest = {
          position: edits.position ?? existingRosterPlayer?.position ?? (basePlayer.position as FloorballPosition) ?? FloorballPosition.None,
          jerseyNumber: edits.jerseyNumber !== undefined ? edits.jerseyNumber : existingRosterPlayer?.jerseyNumber,
          isActive: edits.isActive !== undefined ? edits.isActive : existingRosterPlayer?.isActive ?? basePlayer.isActive,
        };

        await floorballTeamService.updateTeamPlayer(teamId, playerId, updateData);
      }
      
      // Clear local state and refresh team data
      setPlayerEdits({});
      setRemovedPlayers(new Set());
      setAddedPlayers(new Set());
      
      // Refresh team data to show updated roster
      await loadTeamData();
      
    } catch (error) {
      console.error('Error saving roster changes:', error);
      setError('Failed to save roster changes');
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
    .filter(playerId => !originalRosterPlayerIds.has(playerId))
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

  // Filtered lists based on search
  const filteredAvailablePlayers = availablePlayers.filter(player =>
    player.person.fullName.toLowerCase().includes(availablePlayersSearch.toLowerCase())
  );

  const filteredRosterPlayers = displayRoster.filter(player =>
    player.playerName.toLowerCase().includes(rosterPlayersSearch.toLowerCase())
  );

  // Selection management functions
  const toggleAvailablePlayerSelection = (playerId: string) => {
    setSelectedAvailablePlayers(prev => {
      const newSet = new Set(prev);
      if (newSet.has(playerId)) {
        newSet.delete(playerId);
      } else {
        newSet.add(playerId);
      }
      return newSet;
    });
  };

  const toggleRosterPlayerSelection = (playerId: string) => {
    setSelectedRosterPlayers(prev => {
      const newSet = new Set(prev);
      if (newSet.has(playerId)) {
        newSet.delete(playerId);
      } else {
        newSet.add(playerId);
      }
      return newSet;
    });
  };

  const selectAllAvailablePlayers = () => {
    setSelectedAvailablePlayers(new Set(filteredAvailablePlayers.map(p => p.id)));
  };

  const clearAvailableSelection = () => {
    setSelectedAvailablePlayers(new Set());
  };

  const selectAllRosterPlayers = () => {
    setSelectedRosterPlayers(new Set(filteredRosterPlayers.map(p => p.playerId)));
  };

  const clearRosterSelection = () => {
    setSelectedRosterPlayers(new Set());
  };

  const addSelectedPlayersToRoster = () => {
    selectedAvailablePlayers.forEach(playerId => {
      const player = allPlayers.find(p => p.id === playerId);
      if (player) {
        addPlayerToTeam(player);
      }
    });
    setSelectedAvailablePlayers(new Set());
  };

  const removeSelectedPlayersFromRoster = () => {
    selectedRosterPlayers.forEach(playerId => {
      removePlayerFromTeam(playerId);
    });
    setSelectedRosterPlayers(new Set());
  };

  // Check if there are any pending changes
  const hasRosterChanges = Object.keys(playerEdits).length > 0 || removedPlayers.size > 0 || addedPlayers.size > 0;

  if (loadingTeam) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="edit-team-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!teamId) {
    return (
      <PageTemplate title={t('floorball.teams.editTeam', 'Edit Team')}>
        <ErrorPopup message={'Team ID is required'} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.teams.editTeam', 'Edit Team')}>
      <div className="edit-team-page">
        <BackButton 
          to="/admin/floorball/teams" 
          text={t('common.back', 'Back to Teams')} 
        />
        
        <div className="edit-team-header">
          <h1>{t('floorball.teams.editTeam', 'Edit Team')}: {currentTeam?.name}</h1>
        </div>

        <ErrorPopup message={error} />

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
          >
            {t('floorball.teams.manageRoster', 'Manage Roster')} ({displayRoster.length})
          </button>
        </div>

        {/* Team Details Tab */}
        {activeTab === 'details' && (
          <form onSubmit={handleSubmit} className="edit-team-form">
            <div className="form-group">
              <label htmlFor="teamName">{t('floorball.teams.name', 'Team Name')} *</label>
              <input
                id="teamName"
                type="text"
                value={formData.name}
                onChange={(e) => handleInputChange('name', e.target.value)}
                required
                placeholder={t('floorball.teams.namePlaceholder', 'Enter team name')}
              />
            </div>

            <div className="form-group">
              <label htmlFor="clubId">{t('floorball.teams.club', 'Club')} *</label>
              <select
                id="clubId"
                value={formData.clubId}
                onChange={(e) => handleInputChange('clubId', e.target.value)}
                required
              >
                <option value="">{t('floorball.teams.selectClub', 'Select a club')}</option>
                {clubs.map(club => (
                  <option key={club.id} value={club.id}>{club.name}</option>
                ))}
              </select>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="division">{t('floorball.teams.division', 'Division')} *</label>
                <select
                  id="division"
                  value={formData.divisionId}
                  onChange={(e) => handleInputChange('divisionId', e.target.value)}
                  required
                >
                  <option value="">{t('floorball.teams.selectDivision', 'Select division...')}</option>
                  {divisions.map(division => (
                    <option key={division.id} value={division.id}>{division.name}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="category">{t('floorball.teams.category', 'Category')} *</label>
                <select
                  id="category"
                  value={formData.category}
                  onChange={(e) => handleInputChange('category', e.target.value as TeamCategory)}
                  required
                >
                  <option value="Adult">{t('floorball.categories.adult', 'Adult')}</option>
                  <option value="Youth">{t('floorball.categories.youth', 'Youth')}</option>
                  <option value="Women">{t('floorball.categories.women', 'Women')}</option>
                </select>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="homeArena">{t('floorball.teams.homeArena', 'Home Arena')} *</label>
              <input
                id="homeArena"
                type="text"
                value={formData.homeArena}
                onChange={(e) => handleInputChange('homeArena', e.target.value)}
                required
                placeholder={t('floorball.teams.homeArenaPlaceholder', 'Enter home arena')}
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="primaryColor">{t('floorball.teams.primary', 'Primary Jersey Color')} *</label>
                <div className="color-input-group">
                  <input
                    id="primaryColor"
                    type="color"
                    value={formData.primaryJerseyColor}
                    onChange={(e) => handleInputChange('primaryJerseyColor', e.target.value)}
                    required
                  />
                  <input
                    type="text"
                    value={formData.primaryJerseyColor}
                    onChange={(e) => handleInputChange('primaryJerseyColor', e.target.value)}
                    placeholder="#000000"
                  />
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="secondaryColor">{t('floorball.teams.secondary', 'Secondary Jersey Color')}</label>
                <div className="color-input-group">
                  <input
                    id="secondaryColor"
                    type="color"
                    value={formData.secondaryJerseyColor || '#ffffff'}
                    onChange={(e) => handleInputChange('secondaryJerseyColor', e.target.value)}
                  />
                  <input
                    type="text"
                    value={formData.secondaryJerseyColor || ''}
                    onChange={(e) => handleInputChange('secondaryJerseyColor', e.target.value)}
                    placeholder={t('floorball.teams.optional', 'Optional')}
                    minLength={2}
                    maxLength={50}
                  />
                </div>
                {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 0 && formData.secondaryJerseyColor.length < 2 && (
                  <div className="validation-error">
                    {t('floorball.teams.secondaryColorTooShort', 'Secondary color must be at least 2 characters')}
                  </div>
                )}
                {formData.secondaryJerseyColor && formData.secondaryJerseyColor.length > 50 && (
                  <div className="validation-error">
                    {t('floorball.teams.secondaryColorTooLong', 'Secondary color must be no more than 50 characters')}
                  </div>
                )}
              </div>
            </div>

            <div className="form-actions">
              <button type="button" onClick={handleCancel} className="cancel-button" disabled={loading}>
                {t('common.cancel', 'Cancel')}
              </button>
              <button type="submit" disabled={loading} className="submit-button">
                {loading ? t('common.saving', 'Saving...') : t('common.save', 'Save Changes')}
              </button>
            </div>
          </form>
        )}

        {/* Players Management Tab */}
        {activeTab === 'players' && (
          <div className="players-management">
            <div className="roster-management-container">
              {/* Available Players Panel */}
              <div className="players-panel available-panel">
                <div className="panel-header">
                  <h3>{t('floorball.teams.availablePlayers', 'Available Players')}</h3>
                  <span className="player-count">({availablePlayers.length})</span>
                </div>
                
                <div className="panel-controls">
                  <input
                    type="text"
                    placeholder={t('common.search', 'Search')}
                    className="search-input"
                    value={availablePlayersSearch}
                    onChange={(e) => setAvailablePlayersSearch(e.target.value)}
                  />
                  <div className="control-buttons">
                    <button
                      type="button"
                      className="control-btn"
                      onClick={selectAllAvailablePlayers}
                      disabled={filteredAvailablePlayers.length === 0}
                    >
                      {t('common.selectAll', 'Select All')} ({selectedAvailablePlayers.size})
                    </button>
                    <button
                      type="button"
                      className="control-btn"
                      onClick={clearAvailableSelection}
                      disabled={selectedAvailablePlayers.size === 0}
                    >
                      {t('common.clear', 'Clear')}
                    </button>
                  </div>
                </div>

                <div className="players-list-container">
                  {loadingPlayers ? (
                    <div className="loading-state">
                      <p>{t('common.loading', 'Loading players...')}</p>
                    </div>
                  ) : filteredAvailablePlayers.length === 0 ? (
                    <div className="empty-state">
                      <p>{t('floorball.teams.noAvailablePlayers', 'No available players')}</p>
                    </div>
                  ) : (
                    <div className="players-list">
                      {filteredAvailablePlayers.map((player) => (
                        <div
                          key={player.id}
                          className={`player-card ${selectedAvailablePlayers.has(player.id) ? 'selected' : ''}`}
                          onClick={() => toggleAvailablePlayerSelection(player.id)}
                        >
                          <div className="player-info">
                            <div className="player-name">{player.person.fullName}</div>
                            <div className="player-details">
                              <span className="position">{player.position || 'None'}</span>
                              <span className="status">{player.isActive ? 'AKTIVINEN' : 'Inactive'}</span>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              {/* Transfer Controls */}
              <div className="transfer-controls">
                <button
                  type="button"
                  className="transfer-btn add-btn"
                  onClick={addSelectedPlayersToRoster}
                  disabled={selectedAvailablePlayers.size === 0}
                  title={t('floorball.teams.addToRoster', 'Add to roster')}
                >
                  <span className="arrow">→</span>
                  <span className="count">({selectedAvailablePlayers.size})</span>
                </button>
                <button
                  type="button"
                  className="transfer-btn remove-btn"
                  onClick={removeSelectedPlayersFromRoster}
                  disabled={selectedRosterPlayers.size === 0}
                  title={t('floorball.teams.removeFromRoster', 'Remove from roster')}
                >
                  <span className="arrow">←</span>
                  <span className="count">({selectedRosterPlayers.size})</span>
                </button>
              </div>

              {/* Current Roster Panel */}
              <div className="players-panel roster-panel">
                <div className="panel-header">
                  <h3>{t('floorball.teams.currentRoster', 'Current Roster')}</h3>
                  <span className="player-count">({displayRoster.length})</span>
                </div>
                
                <div className="panel-controls">
                  <input
                    type="text"
                    placeholder={t('common.search', 'Search')}
                    className="search-input"
                    value={rosterPlayersSearch}
                    onChange={(e) => setRosterPlayersSearch(e.target.value)}
                  />
                  <div className="control-buttons">
                    <button
                      type="button"
                      className="control-btn"
                      onClick={selectAllRosterPlayers}
                      disabled={filteredRosterPlayers.length === 0}
                    >
                      {t('common.selectAll', 'Select All')} ({selectedRosterPlayers.size})
                    </button>
                    <button
                      type="button"
                      className="control-btn"
                      onClick={clearRosterSelection}
                      disabled={selectedRosterPlayers.size === 0}
                    >
                      {t('common.clear', 'Clear')}
                    </button>
                  </div>
                </div>

                <div className="players-list-container">
                  {filteredRosterPlayers.length === 0 ? (
                    <div className="empty-state">
                      <p>{t('floorball.teams.noPlayers', 'No players in roster')}</p>
                    </div>
                  ) : (
                    <div className="players-list">
                      {filteredRosterPlayers.map((rosterPlayer) => {
                        const edits = playerEdits[rosterPlayer.playerId] || {};
                        const currentPosition = edits.position ?? rosterPlayer.position ?? FloorballPosition.None;
                        const currentJerseyNumber = edits.jerseyNumber !== undefined ? edits.jerseyNumber : rosterPlayer.jerseyNumber;
                        const currentIsActive = edits.isActive !== undefined ? edits.isActive : rosterPlayer.isActive;
                        
                        return (
                          <div
                            key={rosterPlayer.playerId}
                            className={`player-card roster-card ${selectedRosterPlayers.has(rosterPlayer.playerId) ? 'selected' : ''}`}
                            onClick={() => toggleRosterPlayerSelection(rosterPlayer.playerId)}
                          >
                            <div className="player-info">
                              <div className="player-name">{rosterPlayer.playerName}</div>
                              <div className="player-details">
                                <div className="detail-row">
                                  <label>{t('floorball.players.position', 'Position')}:</label>
                                  <select
                                    value={currentPosition}
                                    onChange={(e) => {
                                      e.stopPropagation();
                                      updatePlayerPosition(rosterPlayer.playerId, e.target.value as FloorballPosition);
                                    }}
                                    onClick={(e) => e.stopPropagation()}
                                  >
                                    <option value="Puolustaja">Puolustaja</option>
                                    <option value="Forward">Forward</option>
                                    <option value="Maalivahti">Maalivahti</option>
                                    <option value={FloorballPosition.None}>None</option>
                                  </select>
                                </div>
                                <div className="detail-row">
                                  <label>{t('floorball.players.jerseyNumber', 'Jersey #')}:</label>
                                  <input
                                    type="number"
                                    min="1"
                                    max="99"
                                    value={currentJerseyNumber || ''}
                                    onChange={(e) => {
                                      e.stopPropagation();
                                      updatePlayerJerseyNumber(rosterPlayer.playerId, e.target.value ? parseInt(e.target.value) : undefined);
                                    }}
                                    onClick={(e) => e.stopPropagation()}
                                    placeholder="--"
                                  />
                                </div>
                                <div className="detail-row">
                                  <label>{t('floorball.players.status', 'Status')}:</label>
                                  <select
                                    value={currentIsActive ? 'Aktiivinen' : 'Inactive'}
                                    onChange={(e) => {
                                      e.stopPropagation();
                                      togglePlayerActive(rosterPlayer.playerId, e.target.value === 'Aktiivinen');
                                    }}
                                    onClick={(e) => e.stopPropagation()}
                                  >
                                    <option value="Aktiivinen">Aktiivinen</option>
                                    <option value="Inactive">Inactive</option>
                                  </select>
                                </div>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {hasRosterChanges && (
              <div className="roster-actions">
                <button
                  type="button"
                  className="save-roster-button"
                  onClick={saveRosterChanges}
                  disabled={savingRoster}
                >
                  {savingRoster ? t('common.saving', 'Saving...') : t('floorball.teams.saveRosterChanges', 'Save Roster Changes')}
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </PageTemplate>
  );
};

export default EditTeamPage;
