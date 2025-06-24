import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import type { 
  FloorballMatchDto, 
  FloorballTeam,
  CreateFloorballMatchRequest,
  FloorballMatchStatus
} from '../../../../types/floorball/floorballTypes';
import './MatchManagementPage.scss';

interface MatchManagementPageProps {}

const MatchManagementPage: React.FC<MatchManagementPageProps> = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  // State management
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [teams, setTeams] = useState<FloorballTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  // Form state
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [createForm, setCreateForm] = useState<CreateFloorballMatchRequest>({
    seasonId: '',
    homeTeamId: '',
    awayTeamId: '',
    scheduledDateTime: '',
    venue: ''
  });

  // Fetch all required data
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch seasons, teams, and matches in parallel
      const [seasonsResponse, teamsResponse, matchesResponse] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballTeamService.getAll(),
        floorballMatchService.getAll({ pageSize: 100 })
      ]);

      if (seasonsResponse.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
      }

      if (teamsResponse.success && teamsResponse.data) {
        setTeams(teamsResponse.data);
      }

      if (matchesResponse.success && matchesResponse.data) {
        setMatches(matchesResponse.data);
      }

    } catch (error) {
      console.error('Error fetching data:', error);
      setError(error instanceof Error ? error.message : 'Failed to fetch data');
    } finally {
      setLoading(false);
    }
  }, []);

  // Filter matches by selected season
  const filteredMatches = selectedSeasonId 
    ? matches.filter(match => match.seasonId === selectedSeasonId)
    : matches;

  // Initialize data on component mount
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Handle create form submission
  const handleCreateMatch = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!createForm.seasonId || !createForm.homeTeamId || !createForm.awayTeamId || !createForm.scheduledDateTime) {
      setError('Please fill in all required fields');
      return;
    }

    if (createForm.homeTeamId === createForm.awayTeamId) {
      setError('Home team and away team cannot be the same');
      return;
    }

    try {
      setActionLoading('create');
      setError(null);

      const response = await floorballMatchService.create(createForm);
      
      if (response.success && response.data) {
        setMatches(prev => [...prev, response.data!]);
        setShowCreateForm(false);
        setCreateForm({
          seasonId: '',
          homeTeamId: '',
          awayTeamId: '',
          scheduledDateTime: '',
          venue: ''
        });
      }

    } catch (error) {
      console.error('Error creating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to create match');
    } finally {
      setActionLoading(null);
    }
  };

  // Handle match status changes
  const handleStartMatch = async (matchId: string) => {
    try {
      setActionLoading(`start-${matchId}`);
      setError(null);

      const response = await floorballMatchService.start(matchId);
      
      if (response.success && response.data) {
        setMatches(prev => prev.map(match => 
          match.id === matchId ? response.data! : match
        ));
      }

    } catch (error) {
      console.error('Error starting match:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match');
    } finally {
      setActionLoading(null);
    }
  };

  const handleCompleteMatch = async (matchId: string) => {
    try {
      setActionLoading(`complete-${matchId}`);
      setError(null);

      const response = await floorballMatchService.complete(matchId);
      
      if (response.success && response.data) {
        setMatches(prev => prev.map(match => 
          match.id === matchId ? response.data! : match
        ));
      }

    } catch (error) {
      console.error('Error completing match:', error);
      setError(error instanceof Error ? error.message : 'Failed to complete match');
    } finally {
      setActionLoading(null);
    }
  };

  // Format date for display (dd-mm-yyyy 24-hour format)
  const formatDateTime = (dateTime: string) => {
    const date = new Date(dateTime);
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    
    return `${day}-${month}-${year}, ${hours}:${minutes}`;
  };

  // Get status badge styling
  const getStatusClass = (status: FloorballMatchStatus) => {
    const baseClass = "match-management__status";
    
    switch (status) {
      case 'Scheduled':
        return `${baseClass} ${baseClass}--scheduled`;
      case 'InProgress':
        return `${baseClass} ${baseClass}--progress`;
      case 'Completed':
        return `${baseClass} ${baseClass}--completed`;
      case 'Cancelled':
        return `${baseClass} ${baseClass}--cancelled`;
      case 'Postponed':
        return `${baseClass} ${baseClass}--postponed`;
      default:
        return `${baseClass} ${baseClass}--completed`;
    }
  };

  // Helper function to format season display name
  const formatSeasonDisplayName = (season: FloorballSeasonDto) => {
    const startYear = new Date(season.startDate).getFullYear();
    const endYear = new Date(season.endDate).getFullYear();
    return `${season.name} (${startYear}-${endYear})`;
  };

  // Handle Live button click
  const handleLiveMatch = (match: FloorballMatchDto) => {
    // TODO: Implement live match tracking
    alert(`🔴 Live match tracking for "${match.homeTeamName} vs ${match.awayTeamName}" coming soon!`);
  };

  // Handle Edit button click
  const handleEditMatch = (match: FloorballMatchDto) => {
    // TODO: Implement match editing
    alert(`✏️ Edit match "${match.homeTeamName} vs ${match.awayTeamName}" coming soon!`);
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.matches.title', 'Manage Matches')}>
        <div className="match-management">
          <div className="match-management__container">
            <div className="match-management__loading">
              <div className="match-management__loading-header"></div>
              <div className="match-management__loading-container">
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="match-management__loading-row"></div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.matches.title', 'Manage Matches')}>
      <div className="match-management">
        <div className="match-management__container">
          {/* Enhanced Header */}
          <div className="match-management__header">
            <div className="match-management__header-actions">
              {/* Back Button */}
              <button
                onClick={() => navigate('/admin/floorball')}
                className="match-management__back-btn"
              >
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
                {t('common.back', 'Back to Floorball Management')}
              </button>

              {/* Create Button */}
              <button
                onClick={() => setShowCreateForm(true)}
                className="match-management__create-btn"
              >
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                {t('floorball.matches.createNew', 'Create New Match')}
              </button>
            </div>

            {/* Centered Title */}
            <div className="match-management__header-title-section">
              <h1 className="match-management__header-title">
                {t('floorball.matches.title', 'Match Management')}
              </h1>
              <p className="match-management__header-subtitle">
                Manage your floorball matches, track live games, and organize your season
              </p>
            </div>
          </div>

          {/* Error Display */}
          {error && (
            <div className="match-management__error">
              <div className="match-management__error-content">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>{error}</span>
                <button 
                  onClick={() => setError(null)}
                  className="match-management__error-close"
                >
                  <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>
          )}

          {/* Statistics Section */}
          <div className="match-management__stats">
            <div className="match-management__stats-content">
              <div className="match-management__stats-left">
                <div className="match-management__stats-total">
                  {filteredMatches.length}
                  <span>
                    {selectedSeasonId ? 'matches in season' : 'total matches'}
                  </span>
                </div>
                
                <div className="match-management__stats-indicators">
                  <div className="match-management__stats-indicator">
                    <div className="match-management__stats-indicator-dot match-management__stats-indicator-dot--scheduled"></div>
                    <span className="match-management__stats-indicator-text">
                      <span>{matches.filter(m => m.status === 'Scheduled').length}</span> Scheduled
                    </span>
                  </div>
                  <div className="match-management__stats-indicator">
                    <div className="match-management__stats-indicator-dot match-management__stats-indicator-dot--progress"></div>
                    <span className="match-management__stats-indicator-text">
                      <span>{matches.filter(m => m.status === 'InProgress').length}</span> In Progress
                    </span>
                  </div>
                  <div className="match-management__stats-indicator">
                    <div className="match-management__stats-indicator-dot match-management__stats-indicator-dot--completed"></div>
                    <span className="match-management__stats-indicator-text">
                      <span>{matches.filter(m => m.status === 'Completed').length}</span> Completed
                    </span>
                  </div>
                </div>
              </div>

              {/* Season Filter */}
              <div className="match-management__stats-filter">
                <label>Filter by Season:</label>
                <select
                  value={selectedSeasonId}
                  onChange={(e) => setSelectedSeasonId(e.target.value)}
                >
                  <option value="">All Seasons</option>
                  {seasons.map(season => (
                    <option key={season.id} value={season.id}>
                      {formatSeasonDisplayName(season)}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Matches Table */}
          <div className="match-management__table">
            <div className="match-management__table-container">
              <table>
                <thead>
                  <tr>
                    <th>Match</th>
                    <th>Date & Time</th>
                    <th>Venue</th>
                    <th>Score</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredMatches.length === 0 ? (
                    <tr>
                      <td colSpan={6}>
                        <div className="match-management__empty">
                          <svg className="match-management__empty-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                          </svg>
                          <h3 className="match-management__empty-title">
                            {selectedSeasonId ? 'No matches found for selected season' : 'No matches found'}
                          </h3>
                          <p className="match-management__empty-description">Create your first match to get started</p>
                          <button
                            onClick={() => setShowCreateForm(true)}
                            className="match-management__empty-button"
                          >
                            Create New Match
                          </button>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    filteredMatches.map((match) => (
                      <tr key={match.id}>
                        <td>
                          <div className="match-management__table-match-name">
                            {match.homeTeamName} vs {match.awayTeamName}
                          </div>
                        </td>
                        <td className="no-wrap">
                          <div className="match-management__table-date">
                            {formatDateTime(match.scheduledDateTime)}
                          </div>
                        </td>
                        <td className="no-wrap">
                          <div className={`match-management__table-venue ${!match.venue ? 'match-management__table-venue--tbd' : ''}`}>
                            {match.venue || 'TBD'}
                          </div>
                        </td>
                        <td className="no-wrap">
                          <div className={`match-management__table-score ${match.status === 'Scheduled' ? 'match-management__table-score--empty' : 'match-management__table-score--filled'}`}>
                            {match.status === 'Scheduled' ? '-' : `${match.homeScore} - ${match.awayScore}`}
                          </div>
                        </td>
                        <td className="no-wrap">
                          <span className={getStatusClass(match.status)}>
                            {match.status}
                          </span>
                        </td>
                        <td>
                          <div className="match-management__table-actions">
                            {/* Live Button */}
                            <button
                              onClick={() => handleLiveMatch(match)}
                              className="match-management__live-btn"
                            >
                              <svg fill="currentColor" viewBox="0 0 20 20">
                                <circle cx="10" cy="10" r="3" />
                              </svg>
                              Live
                            </button>
                            
                            {/* Edit Button */}
                            <button
                              onClick={() => handleEditMatch(match)}
                              className="match-management__edit-btn"
                            >
                              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                              Edit
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Create Form Modal */}
          {showCreateForm && (
            <div className="modal">
              <div className="modal__content">
                <div className="modal__header">
                  <div className="modal__header-content">
                    <h2 className="modal__header-title">Create New Match</h2>
                    <button
                      onClick={() => setShowCreateForm(false)}
                      className="modal__header-close"
                    >
                      <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                </div>
                
                <form onSubmit={handleCreateMatch} className="modal__form">
                  {/* Season Selection */}
                  <div className="form-group">
                    <label>Season *</label>
                    <select
                      value={createForm.seasonId}
                      onChange={(e) => setCreateForm(prev => ({ ...prev, seasonId: e.target.value }))}
                      required
                    >
                      <option value="">Select Season</option>
                      {seasons.map(season => (
                        <option key={season.id} value={season.id}>
                          {formatSeasonDisplayName(season)}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Home Team Selection */}
                  <div className="form-group">
                    <label>Home Team *</label>
                    <select
                      value={createForm.homeTeamId}
                      onChange={(e) => setCreateForm(prev => ({ ...prev, homeTeamId: e.target.value }))}
                      required
                    >
                      <option value="">Select Home Team</option>
                      {teams.map(team => (
                        <option key={team.id} value={team.id}>
                          {team.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Away Team Selection */}
                  <div className="form-group">
                    <label>Away Team *</label>
                    <select
                      value={createForm.awayTeamId}
                      onChange={(e) => setCreateForm(prev => ({ ...prev, awayTeamId: e.target.value }))}
                      required
                    >
                      <option value="">Select Away Team</option>
                      {teams.filter(team => team.id !== createForm.homeTeamId).map(team => (
                        <option key={team.id} value={team.id}>
                          {team.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Scheduled Date/Time */}
                  <div className="form-group">
                    <label>Scheduled Date & Time *</label>
                    <input
                      type="datetime-local"
                      value={createForm.scheduledDateTime}
                      onChange={(e) => setCreateForm(prev => ({ ...prev, scheduledDateTime: e.target.value }))}
                      required
                    />
                  </div>

                  {/* Venue */}
                  <div className="form-group">
                    <label>Venue</label>
                    <input
                      type="text"
                      value={createForm.venue}
                      onChange={(e) => setCreateForm(prev => ({ ...prev, venue: e.target.value }))}
                      placeholder="Enter venue name"
                    />
                  </div>

                  {/* Form Actions */}
                  <div className="modal__form-actions">
                    <button
                      type="submit"
                      disabled={actionLoading === 'create'}
                      className="primary"
                    >
                      {actionLoading === 'create' ? (
                        <>
                          <svg className="spinner" fill="none" viewBox="0 0 24 24">
                            <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                            <path fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                          </svg>
                          Creating...
                        </>
                      ) : (
                        'Create Match'
                      )}
                    </button>
                    <button
                      type="button"
                      onClick={() => setShowCreateForm(false)}
                      className="secondary"
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </div>
      </div>
    </PageTemplate>
  );
};

export default MatchManagementPage; 