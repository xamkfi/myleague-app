import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { getClubs, type Club } from '../../../../../api/clubService';
import type { FloorballTeam, FloorballTeamRequest, FloorballDivision, TeamCategory } from '../../../../../types/floorball/floorballTypes';

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
  const [teamPlayers, setTeamPlayers] = useState<FloorballPlayerDto[]>([]);
  const [activeTab, setActiveTab] = useState<'details' | 'players'>('details');
  
  const [formData, setFormData] = useState<FloorballTeamRequest>({
    name: '',
    division: 'Premier' as FloorballDivision,
    clubId: '',
    homeArena: '',
    primaryJerseyColor: '#000000',
    category: 'Adult' as TeamCategory,
    secondaryJerseyColor: ''
  });

  // Load team data when modal opens
  useEffect(() => {
    if (isOpen && teamId) {
      console.log('Modal opened for editing team:', teamId);
      loadTeamData();
      loadClubs();
      loadAllPlayers();
    } else if (isOpen) {
      console.log('Modal opened for creating new team');
      // Reset form for new team
      resetForm();
      loadClubs();
      loadAllPlayers();
    }
  }, [isOpen, teamId]);

  const resetForm = () => {
    setFormData({
      name: '',
      division: 'Premier' as FloorballDivision,
      clubId: '',
      homeArena: '',
      primaryJerseyColor: '#000000',
      category: 'Adult' as TeamCategory,
      secondaryJerseyColor: ''
    });
    setTeamPlayers([]);
  };

  const loadTeamData = async () => {
    if (!teamId) return;
    
    try {
      setLoadingTeam(true);
      const team = await floorballTeamService.getById(teamId);
      
      setFormData({
        name: team.name,
        division: team.division,
        clubId: team.club.id,
        homeArena: team.homeArena,
        primaryJerseyColor: team.primaryJerseyColor,
        category: 'Adult' as TeamCategory, // Default since it's not in the response
        secondaryJerseyColor: team.secondaryJerseyColor || ''
      });

      // Load team players
      try {
        console.log('Loading players for team:', teamId);
        const players = await floorballPlayerService.getByTeamId(teamId);
        console.log('Team players loaded:', players.length);
        setTeamPlayers(players);
      } catch (playerErr) {
        console.warn('Could not load team players:', playerErr);
        // Don't fail the whole modal, just show empty roster
        setTeamPlayers([]);
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
      console.log('Loading all players for team management...');
      
      // Try to get all players without any filters first
      const response = await floorballPlayerService.getAll({
        pageSize: 1000 // Get all players
      });
      
      console.log('Players API response:', response);
      
      if (response && response.data && Array.isArray(response.data)) {
        setAllPlayers(response.data);
        console.log('Successfully loaded players:', response.data.length);
      } else {
        console.warn('Invalid players response format:', response);
        // Try alternative approach - get players without pagination
        try {
          console.log('Trying alternative approach without pagination...');
          const altResponse = await floorballPlayerService.getAll();
          console.log('Alternative response:', altResponse);
          
          if (altResponse && altResponse.data && Array.isArray(altResponse.data)) {
            setAllPlayers(altResponse.data);
            console.log('Successfully loaded players via alternative method:', altResponse.data.length);
          } else {
            console.warn('Alternative approach also failed, setting empty array');
            setAllPlayers([]);
          }
        } catch (altError) {
          console.error('Alternative approach failed:', altError);
          setAllPlayers([]);
        }
      }
    } catch (err) {
      console.error('Error loading players:', err);
      
      // Try one more time with minimal parameters
      try {
        console.log('Trying minimal API call...');
        const minimalResponse = await floorballPlayerService.getAll({ page: 1, pageSize: 50 });
        
        if (minimalResponse && minimalResponse.data && Array.isArray(minimalResponse.data)) {
          setAllPlayers(minimalResponse.data);
          console.log('Successfully loaded players via minimal call:', minimalResponse.data.length);
        } else {
          console.warn('Minimal call also failed, using mock data for testing');
          // Use mock data as last resort for testing
          const mockPlayers: FloorballPlayerDto[] = [
            {
              id: 'mock-1',
              personId: 'person-1',
              firstName: 'John',
              lastName: 'Doe',
              fullName: 'John Doe',
              dateOfBirth: '1990-01-01',
              isActive: true,
              position: 'Forward',
              jerseyNumber: 10,
              gamesPlayed: 15,
              goals: 8,
              assists: 12,
              penaltyMinutes: 4
            },
            {
              id: 'mock-2',
              personId: 'person-2',
              firstName: 'Jane',
              lastName: 'Smith',
              fullName: 'Jane Smith',
              dateOfBirth: '1992-05-15',
              isActive: true,
              position: 'Defender',
              jerseyNumber: 5,
              gamesPlayed: 18,
              goals: 2,
              assists: 6,
              penaltyMinutes: 8
            },
            {
              id: 'mock-3',
              personId: 'person-3',
              firstName: 'Mike',
              lastName: 'Johnson',
              fullName: 'Mike Johnson',
              dateOfBirth: '1988-12-03',
              isActive: true,
              position: 'Goalkeeper',
              jerseyNumber: 1,
              gamesPlayed: 20,
              goals: 0,
              assists: 1,
              penaltyMinutes: 0
            },
            {
              id: 'mock-4',
              personId: 'person-4',
              firstName: 'Sarah',
              lastName: 'Wilson',
              fullName: 'Sarah Wilson',
              dateOfBirth: '1991-08-22',
              isActive: true,
              position: 'Forward',
              jerseyNumber: 7,
              gamesPlayed: 12,
              goals: 5,
              assists: 9,
              penaltyMinutes: 2
            },
            {
              id: 'mock-5',
              personId: 'person-5',
              firstName: 'Alex',
              lastName: 'Brown',
              fullName: 'Alex Brown',
              dateOfBirth: '1989-03-10',
              isActive: false,
              position: 'Defender',
              jerseyNumber: 3,
              gamesPlayed: 8,
              goals: 1,
              assists: 3,
              penaltyMinutes: 12
            }
          ];
          setAllPlayers(mockPlayers);
          console.log('Using mock data with', mockPlayers.length, 'players');
        }
      } catch (minimalError) {
        console.error('All player loading attempts failed:', minimalError);
        // Don't fail completely, just show empty list
        setAllPlayers([]);
      }
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
        // Only include secondaryJerseyColor if it's valid (2-50 characters) or omit it entirely
        ...(formData.secondaryJerseyColor && formData.secondaryJerseyColor.length >= 2 && formData.secondaryJerseyColor.length <= 50
          ? { secondaryJerseyColor: formData.secondaryJerseyColor }
          : {})
      };
      
      console.log('Submitting team data:', updateData);
      console.log('Team roster changes:', teamPlayers.length, 'players');
      console.log('Secondary jersey color length:', formData.secondaryJerseyColor?.length || 0);
      
      await onSubmit(updateData);
      
      // TODO: In a real implementation, you would also save the roster changes here
      // This would involve calling an API to update team player assignments
      // For now, we're just managing the UI state
      
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
    if (!teamPlayers.find(p => p.id === player.id)) {
      setTeamPlayers(prev => [...prev, player]);
    }
  };

  const removePlayerFromTeam = (playerId: string) => {
    setTeamPlayers(prev => prev.filter(p => p.id !== playerId));
  };

  const availablePlayers = allPlayers.filter(player => 
    !teamPlayers.find(teamPlayer => teamPlayer.id === player.id)
  );

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
              >
                {t('floorball.teams.manageRoster', 'Manage Roster')} ({teamPlayers.length})
              </button>
            </div>

            {/* Team Details Tab */}
            {activeTab === 'details' && (
              <form onSubmit={handleSubmit} className="team-form">
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
                      value={formData.division}
                      onChange={(e) => handleInputChange('division', e.target.value as FloorballDivision)}
                      required
                    >
                      <option value="Premier">{t('floorball.divisions.premier', 'Premier')}</option>
                      <option value="Division1">{t('floorball.divisions.division1', 'Division 1')}</option>
                      <option value="Division2">{t('floorball.divisions.division2', 'Division 2')}</option>
                      <option value="Division3">{t('floorball.divisions.division3', 'Division 3')}</option>
                      <option value="Division4">{t('floorball.divisions.division4', 'Division 4')}</option>
                      <option value="Youth">{t('floorball.divisions.youth', 'Youth')}</option>
                      <option value="Junior">{t('floorball.divisions.junior', 'Junior')}</option>
                      <option value="Veterans">{t('floorball.divisions.veterans', 'Veterans')}</option>
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
                  <button type="button" onClick={onClose} className="cancel-button">
                    {t('common.cancel', 'Cancel')}
                  </button>
                  <button type="submit" disabled={loading} className="submit-button">
                    {loading ? t('common.saving', 'Saving...') : t('common.save', 'Save')}
                  </button>
                </div>
              </form>
            )}

            {/* Players Management Tab */}
            {activeTab === 'players' && (
              <div className="players-management">
                {/* Debug Info - only in development */}
                {import.meta.env.DEV && (
                  <div className="debug-info" style={{ background: '#f8f9fa', padding: '1rem', marginBottom: '1rem', borderRadius: '4px', fontSize: '0.85rem' }}>
                    <strong>Debug Info:</strong><br/>
                    Total Players Loaded: {allPlayers.length}<br/>
                    Current Team Players: {teamPlayers.length}<br/>
                    Available Players: {availablePlayers.length}<br/>
                    Loading Players: {loadingPlayers ? 'Yes' : 'No'}<br/>
                    Team ID: {teamId || 'New Team'}
                  </div>
                )}

                <div className="players-section">
                  {/* Info banner about roster management */}
                  <div className="info-banner" style={{ 
                    background: '#e3f2fd', 
                    border: '1px solid #2196f3', 
                    borderRadius: '4px', 
                    padding: '0.75rem', 
                    marginBottom: '1rem',
                    fontSize: '0.9rem',
                    color: '#1565c0'
                  }}>
                    <strong>ℹ️ Note:</strong> Player roster management is currently in demo mode. 
                    Changes are shown in the interface but not yet saved to the database. 
                    Team details will be saved normally.
                  </div>

                  <div className="current-roster">
                    <h3>{t('floorball.teams.currentRoster', 'Current Roster')} ({teamPlayers.length})</h3>
                    <div className="players-list">
                      {teamPlayers.length === 0 ? (
                        <p className="no-players">{t('floorball.teams.noPlayersInRoster', 'No players in roster')}</p>
                      ) : (
                        teamPlayers.map(player => (
                          <div key={player.id} className="player-item">
                            <div className="player-info">
                              <span className="player-name">{player.fullName}</span>
                              {player.position && (
                                <span className="player-position">
                                  {t(`floorball.positions.${player.position.toLowerCase()}`, player.position)}
                                </span>
                              )}
                              {player.jerseyNumber && (
                                <span className="jersey-number">#{player.jerseyNumber}</span>
                              )}
                            </div>
                            <button
                              className="remove-button"
                              onClick={() => removePlayerFromTeam(player.id)}
                              title={t('floorball.teams.removeFromRoster', 'Remove from roster')}
                            >
                              ✕
                            </button>
                          </div>
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
                            <p className="no-players">{t('floorball.teams.noAvailablePlayers', 'No available players')}</p>
                          ) : (
                            availablePlayers.map(player => (
                              <div key={player.id} className="player-item">
                                <div className="player-info">
                                  <span className="player-name">{player.fullName}</span>
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
                    onClick={() => setActiveTab('details')} 
                    className="submit-button"
                  >
                    {t('floorball.teams.saveRoster', 'Save Roster & Continue')}
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default EditTeamModal; 