import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { 
  FloorballSeasonDto, 
  UpdateFloorballSeasonRequest
} from '../../../../../api/floorball/floorballSeasonService';
import { floorballSeasonService } from '../../../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { type FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import { useDivisions } from '../../../../../hooks/useDivisions';

interface EditSeasonModalProps {
  season: FloorballSeasonDto;
  onSave: (seasonData: UpdateFloorballSeasonRequest) => Promise<void>;
  onClose: () => void;
  onTeamsChanged?: () => Promise<void>;
}

export const EditSeasonModal = ({
  season,
  onSave,
  onClose,
  onTeamsChanged
}: EditSeasonModalProps) => {
  const { t } = useTranslation();
  
  const [formData, setFormData] = useState<UpdateFloorballSeasonRequest>({
    name: season.name,
    startDate: season.startDate.split('T')[0], // Convert to YYYY-MM-DD format
    endDate: season.endDate.split('T')[0],
    divisionId: season.divisionId
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'teams'>('details');
  
  // Team management state
  const [allTeams, setAllTeams] = useState<FloorballTeam[]>([]);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const [savingTeams, setSavingTeams] = useState(false);
  const [addedTeams, setAddedTeams] = useState<Set<string>>(new Set());
  const [removedTeams, setRemovedTeams] = useState<Set<string>>(new Set());

  // Load teams when modal opens
  useEffect(() => {
    loadAllTeams();
  }, []);

  const loadAllTeams = async () => {
    try {
      setLoadingTeams(true);
      const response = await floorballTeamService.getAll({
        pageSize: 50 // Get all teams
      });
      
      if (response && response.data && Array.isArray(response.data)) {
        setAllTeams(response.data);
      } else {
        setAllTeams([]);
      }
    } catch (err) {
      console.error('Error loading teams:', err);
      setAllTeams([]);
    } finally {
      setLoadingTeams(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const parseApiError = (error: unknown): string => {
    const errorMessage = error instanceof Error ? error.message : String(error);
    
    // Handle network errors
    if (errorMessage?.includes('Failed to fetch') || errorMessage?.includes('NetworkError')) {
      return t('floorball.seasons.errors.networkError', 'Network error. Please check your connection and try again.');
    }

    // Handle HTTP errors with specific status codes
    if (errorMessage?.includes('HTTP 400')) {
      return t('floorball.seasons.errors.validationError', 'Invalid data provided. Please check your input and try again.');
    }
    
    if (errorMessage?.includes('HTTP 404')) {
      return t('floorball.seasons.errors.notFound', 'Season not found. It may have been deleted.');
    }
    
    if (errorMessage?.includes('HTTP 409')) {
      return t('floorball.seasons.errors.conflictError', 'A season with overlapping dates already exists.');
    }
    
    if (errorMessage?.includes('HTTP 500')) {
      return t('floorball.seasons.errors.serverError', 'Server error. Please try again later.');
    }

    // Handle specific business logic errors
    
    if (errorMessage.includes('Cannot update a completed season')) {
      return t('floorball.seasons.errors.cannotUpdateCompleted', 'Cannot update a completed season.');
    }
    
    if (errorMessage.includes('overlapping dates') || errorMessage.includes('overlaps with')) {
      return t('floorball.seasons.errors.overlappingDates', 'A season already exists that overlaps with the specified dates.');
    }
    
    if (errorMessage.includes('Season name is required')) {
      return t('floorball.seasons.validation.nameRequired', 'Season name is required.');
    }
    
    if (errorMessage.includes('Season name cannot exceed 100 characters')) {
      return t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters.');
    }
    
    if (errorMessage.includes('Division is required') || errorMessage.includes('Invalid division')) {
      return t('floorball.seasons.validation.divisionRequired', 'Please select a valid division.');
    }
    
    if (errorMessage.includes('Cannot change division because some teams')) {
      return t('floorball.seasons.errors.cannotChangeDivisionWithTeams', 'Cannot change division because some teams belong to a different division.');
    }
    
    if (errorMessage.includes('Start date is required')) {
      return t('floorball.seasons.validation.startDateRequired', 'Start date is required.');
    }
    
    if (errorMessage.includes('End date is required')) {
      return t('floorball.seasons.validation.endDateRequired', 'End date is required.');
    }
    
    if (errorMessage.includes('End date must be after start date')) {
      return t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date.');
    }

    // Default error message
    return errorMessage || t('floorball.seasons.errors.updateFailed', 'Failed to update season. Please try again.');
  };

  const parseTeamError = (error: unknown): string => {
    const errorMessage = error instanceof Error ? error.message : String(error);
    
    if (errorMessage.includes('Cannot add a team to a completed season')) {
      return t('floorball.seasons.errors.cannotAddTeamToCompleted', 'Cannot add teams to a completed season.');
    }
    
    if (errorMessage.includes('Cannot remove a team from a completed season')) {
      return t('floorball.seasons.errors.cannotRemoveTeamFromCompleted', 'Cannot remove teams from a completed season.');
    }
    
    if (errorMessage.includes('Team division') && errorMessage.includes('does not match season division')) {
      return t('floorball.seasons.errors.teamDivisionMismatch', 'Team division does not match season division.');
    }
    
    if (errorMessage.includes('Cannot remove team that is part of scheduled matches')) {
      return t('floorball.seasons.errors.cannotRemoveTeamWithMatches', 'Cannot remove team that has scheduled matches.');
    }
    
    if (errorMessage.includes('Season not found') || errorMessage.includes('HTTP 404')) {
      return t('floorball.seasons.errors.notFound', 'Season not found. It may have been deleted.');
    }
    
    if (errorMessage.includes('Team') && errorMessage.includes('not found')) {
      return t('floorball.seasons.errors.teamNotFound', 'Team not found. It may have been deleted.');
    }

    return parseApiError(error);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Client-side validation
      if (!formData.name.trim()) {
        throw new Error(t('floorball.seasons.validation.nameRequired', 'Season name is required'));
      }

      if (formData.name.trim().length > 100) {
        throw new Error(t('floorball.seasons.validation.nameTooLong', 'Season name cannot exceed 100 characters'));
      }

      if (!formData.divisionId) {
        throw new Error(t('floorball.seasons.validation.divisionRequired', 'Division is required'));
      }

      if (!formData.startDate) {
        throw new Error(t('floorball.seasons.validation.startDateRequired', 'Start date is required'));
      }

      if (!formData.endDate) {
        throw new Error(t('floorball.seasons.validation.endDateRequired', 'End date is required'));
      }

      // Validate dates
      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        throw new Error(t('floorball.seasons.validation.invalidDate', 'Please enter valid dates'));
      }

      if (endDate <= startDate) {
        throw new Error(t('floorball.seasons.validation.endDateAfterStart', 'End date must be after start date'));
      }

      // Check if season is too long (more than 2 years)
      const maxDuration = 2 * 365 * 24 * 60 * 60 * 1000; // 2 years in milliseconds
      if (endDate.getTime() - startDate.getTime() > maxDuration) {
        throw new Error(t('floorball.seasons.validation.seasonTooLong', 'Season duration cannot exceed 2 years'));
      }

      await onSave(formData);
    } catch (err) {
      setError(parseApiError(err));
    } finally {
      setLoading(false);
    }
  };

  const addTeamToSeason = (team: FloorballTeam) => {
    setError(null); // Clear any existing errors
    
    // Check if season is completed
    if (season.isCompleted) {
      setError(t('floorball.seasons.errors.cannotAddTeamToCompleted', 'Cannot add teams to a completed season.'));
      return;
    }
    
    // Check division match
    if (team.divisionId !== season.divisionId) {
      setError(t('floorball.seasons.errors.teamDivisionMismatch', 'Team division does not match season division.'));
      return;
    }
    
    setAddedTeams(prev => new Set([...prev, team.id]));
    setRemovedTeams(prev => {
      const newSet = new Set(prev);
      newSet.delete(team.id);
      return newSet;
    });
  };

  const removeTeamFromSeason = (teamId: string) => {
    setError(null); // Clear any existing errors
    
    // Check if season is completed
    if (season.isCompleted) {
      setError(t('floorball.seasons.errors.cannotRemoveTeamFromCompleted', 'Cannot remove teams from a completed season.'));
      return;
    }
    
    setRemovedTeams(prev => new Set([...prev, teamId]));
    setAddedTeams(prev => {
      const newSet = new Set(prev);
      newSet.delete(teamId);
      return newSet;
    });
  };

  const saveTeamChanges = async () => {
    setSavingTeams(true);
    setError(null);
    
    try {
      // Handle removals first
      for (const teamId of removedTeams) {
        try {
          await floorballSeasonService.removeTeamFromSeason(season.id, teamId);
        } catch (error) {
          console.error(`Error removing team ${teamId}:`, error);
          throw new Error(`Failed to remove team: ${parseTeamError(error)}`);
        }
      }

      // Handle additions
      const originalTeamIds = new Set(season.teams?.map(team => team.id) || []);
      const teamsToAdd = Array.from(addedTeams).filter(teamId => !originalTeamIds.has(teamId));
      
      for (const teamId of teamsToAdd) {
        try {
          await floorballSeasonService.addTeamToSeason(season.id, teamId);
        } catch (error) {
          console.error(`Error adding team ${teamId}:`, error);
          throw new Error(`Failed to add team: ${parseTeamError(error)}`);
        }
      }

      // Clear local state
      setAddedTeams(new Set());
      setRemovedTeams(new Set());
      
      // Trigger parent refresh if callback provided
      if (onTeamsChanged) {
        await onTeamsChanged();
      }
      
      // Close modal
      onClose();
    } catch (error) {
      console.error('Error saving team changes:', error);
      setError(parseTeamError(error));
    } finally {
      setSavingTeams(false);
    }
  };

  const { divisions } = useDivisions();

  // Calculate display data for teams
  const seasonTeams = season.teams?.filter(team => !removedTeams.has(team.id)) || [];
  const originalTeamIds = new Set(season.teams?.map(team => team.id) || []);
  
  const locallyAddedTeams = Array.from(addedTeams)
    .filter(teamId => !originalTeamIds.has(teamId))
    .map(teamId => allTeams.find(team => team.id === teamId))
    .filter(Boolean) as FloorballTeam[];
  
  const displayTeams = [...seasonTeams, ...locallyAddedTeams];
  const availableTeams = allTeams.filter(team => 
    !displayTeams.find(seasonTeam => seasonTeam.id === team.id)
  );
  
  const hasTeamChanges = addedTeams.size > 0 || removedTeams.size > 0;

  return (
    <div className="modal-overlay">
      <div className="modal-content edit-season-modal">
        <div className="modal-header">
          <h3>{t('floorball.seasons.edit.title', 'Edit Season')}</h3>
          <button 
            className="modal-close-btn"
            onClick={onClose}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>

        {/* Tab Navigation */}
        <div className="tab-navigation">
          <button 
            className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
            onClick={() => setActiveTab('details')}
          >
            {t('floorball.seasons.seasonDetails', 'Season Details')}
          </button>
          <button 
            className={`tab-button ${activeTab === 'teams' ? 'active' : ''}`}
            onClick={() => setActiveTab('teams')}
          >
            {t('floorball.seasons.manageTeams', 'Manage Teams')} ({displayTeams.length})
          </button>
        </div>
        
        {/* Season Details Tab */}
        {activeTab === 'details' && (
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              {error && (
                <div className="error-message">
                  <i className="fas fa-exclamation-circle"></i>
                  {error}
                </div>
              )}

              <div className="form-group">
                <label htmlFor="edit-name">
                  {t('floorball.seasons.fields.name', 'Name')} *
                </label>
                <input
                  type="text"
                  id="edit-name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                  placeholder={t('floorball.seasons.placeholders.name', 'Enter season name')}
                />
              </div>

              <div className="form-group">
                <label htmlFor="edit-division">
                  {t('floorball.seasons.fields.division', 'Division')} *
                </label>
                <select
                  id="edit-division"
                  name="divisionId"
                  value={formData.divisionId}
                  onChange={handleInputChange}
                  required
                  disabled={loading}
                >
                  <option value="">{t('floorball.seasons.placeholders.selectDivision', 'Select division')}</option>
                  {divisions.map(division => (
                    <option key={division.id} value={division.id}>
                      {division.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="edit-startDate">
                    {t('floorball.seasons.fields.startDate', 'Start Date')} *
                  </label>
                  <input
                    type="date"
                    id="edit-startDate"
                    name="startDate"
                    value={formData.startDate}
                    onChange={handleInputChange}
                    required
                    disabled={loading}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="edit-endDate">
                    {t('floorball.seasons.fields.endDate', 'End Date')} *
                  </label>
                  <input
                    type="date"
                    id="edit-endDate"
                    name="endDate"
                    value={formData.endDate}
                    onChange={handleInputChange}
                    required
                    disabled={loading}
                    min={formData.startDate}
                  />
                </div>
              </div>
            </div>
            
            <div className="modal-footer">
              <button 
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
                disabled={loading}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button 
                type="submit"
                className="btn btn-primary"
                disabled={loading}
              >
                {loading ? (
                  <>
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.saving', 'Saving...')}
                  </>
                ) : (
                  t('common.save', 'Save')
                )}
              </button>
            </div>
          </form>
        )}

        {/* Teams Management Tab */}
        {activeTab === 'teams' && (
          <div className="teams-management">
            <div className="modal-body">
              {error && (
                <div className="error-message">
                  <i className="fas fa-exclamation-circle"></i>
                  {error}
                </div>
              )}

              <div className="teams-sections-container">
                {/* Current Teams */}
                <div className="teams-section">
                  <h4>{t('floorball.seasons.currentTeams', 'Current Teams')} ({displayTeams.length})</h4>
                  {displayTeams.length === 0 ? (
                    <p className="no-teams">{t('floorball.seasons.noTeams', 'No teams in this season')}</p>
                  ) : (
                    <div className="teams-list">
                      {displayTeams.map(team => (
                        <div key={team.id} className="team-item">
                          <div className="team-info">
                            <span className="team-name">{team.name}</span>
                            <span className="team-club">{team.club.name}</span>
                            <span className={`team-division division-${team.divisionId}`}>
                              {divisions.find(d => d.id == team.divisionId)?.name || ''}
                            </span>
                          </div>
                          <button
                            type="button"
                            className="btn btn-danger btn-sm"
                            onClick={() => removeTeamFromSeason(team.id)}
                            disabled={savingTeams}
                          >
                            🗑️ {t('common.remove', 'Remove')}
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Available Teams */}
                <div className="teams-section">
                  <h4>{t('floorball.seasons.availableTeams', 'Available Teams')} ({availableTeams.length})</h4>
                  {loadingTeams ? (
                    <p>{t('common.loading', 'Loading...')}</p>
                  ) : availableTeams.length === 0 ? (
                    <p className="no-teams">{t('floorball.seasons.noAvailableTeams', 'No available teams')}</p>
                  ) : (
                    <div className="teams-list">
                      {availableTeams.map(team => (
                        <div key={team.id} className="team-item">
                          <div className="team-info">
                            <span className="team-name">{team.name}</span>
                            <span className="team-club">{team.club.name}</span>
                            <span className={`team-division division-${team.divisionId.toLowerCase()}`}>
                              {divisions.find(d => d.id == team.divisionId)?.name || ''}
                            </span>
                          </div>
                          <button
                            type="button"
                            className="btn btn-primary btn-sm"
                            onClick={() => addTeamToSeason(team)}
                            disabled={savingTeams}
                          >
                            ➕ {t('common.add', 'Add')}
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div className="modal-footer">
              <button 
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
                disabled={savingTeams}
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button 
                type="button"
                className="btn btn-primary"
                onClick={saveTeamChanges}
                disabled={savingTeams || !hasTeamChanges}
              >
                {savingTeams ? (
                  <>
                    <i className="fas fa-spinner fa-spin"></i>
                    {t('common.saving', 'Saving...')}
                  </>
                ) : (
                  t('floorball.seasons.saveTeamChanges', 'Save Team Changes')
                )}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}; 