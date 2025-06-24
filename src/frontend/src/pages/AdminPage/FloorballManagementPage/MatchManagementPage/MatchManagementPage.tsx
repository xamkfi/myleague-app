import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import Navbar from '../../../../components/Navigation/Navbar';
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
  const [liveMatches, setLiveMatches] = useState<Set<string>>(new Set());

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

  // Filter and sort matches: upcoming first (ascending), then past (descending)
  const filteredMatches = React.useMemo(() => {
    const now = new Date();
    
    // First filter by season if selected
    const filtered = selectedSeasonId 
      ? matches.filter(match => match.seasonId === selectedSeasonId)
      : matches;
    
    // Separate upcoming and past matches
    const upcomingMatches = filtered.filter(match => {
      const matchDate = new Date(match.scheduledDateTime);
      return matchDate >= now || match.status === 'Scheduled' || match.status === 'InProgress';
    });
    
    const pastMatches = filtered.filter(match => {
      const matchDate = new Date(match.scheduledDateTime);
      return matchDate < now && (match.status === 'Completed' || match.status === 'Cancelled' || match.status === 'Postponed');
    });
    
    // Sort upcoming matches by date ascending (soonest first)
    const sortedUpcoming = upcomingMatches.sort((a, b) => {
      const dateA = new Date(a.scheduledDateTime);
      const dateB = new Date(b.scheduledDateTime);
      return dateA.getTime() - dateB.getTime();
    });
    
    // Sort past matches by date descending (most recent first)
    const sortedPast = pastMatches.sort((a, b) => {
      const dateA = new Date(a.scheduledDateTime);
      const dateB = new Date(b.scheduledDateTime);
      return dateB.getTime() - dateA.getTime();
    });
    
    // Combine: upcoming first, then past
    return [...sortedUpcoming, ...sortedPast];
  }, [matches, selectedSeasonId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

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

  const handleLiveMatch = (match: FloorballMatchDto) => {
    const isCurrentlyLive = liveMatches.has(match.id);
    
    if (isCurrentlyLive) {
      // If already live, navigate to live match page
      navigate(`/admin/floorball/matches/${match.id}/live`);
    } else {
      // If not live, mark as live (Go Live -> Live) and update status
      setLiveMatches(prev => new Set([...prev, match.id]));
      
      // Update match status to InProgress
      setMatches(prev => prev.map(m => 
        m.id === match.id 
          ? { ...m, status: 'InProgress' as FloorballMatchStatus }
          : m
      ));
    }
  };

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const formatDateTime = (dateTime: string) => {
    const date = new Date(dateTime);
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    
    return `${day}-${month}-${year}, ${hours}:${minutes}`;
  };

  const getStatusBadge = (status: FloorballMatchStatus) => {
    const statusClasses = {
      'Scheduled': 'status-scheduled',
      'InProgress': 'status-progress',
      'Completed': 'status-completed',
      'Cancelled': 'status-cancelled',
      'Postponed': 'status-postponed'
    };
    
    return `status-badge ${statusClasses[status] || 'status-completed'}`;
  };

  const formatSeasonDisplayName = (season: FloorballSeasonDto) => {
    return `${season.name} (${season.startDate.split('-')[0]}-${season.endDate.split('-')[0]})`;
  };

  if (loading) {
    return (
      <div className="match-management">
        <Navbar />
        <div className="match-management__content">
          <div className="loading-spinner">
            <div className="spinner"></div>
            <p>Loading matches...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="match-management">
      <Navbar />
      <div className="match-management__content">
        {/* Header Section */}
        <div className="page-header">
          <div className="page-header__top">
            <button 
              onClick={() => navigate('/admin/floorball')}
              className="back-button"
            >
              ← Back to Admin
            </button>
            <button 
              onClick={() => setShowCreateForm(true)}
              className="create-button"
            >
              + Create New Match
            </button>
          </div>
          <div className="page-header__main">
            <h1 className="page-title">Match Management</h1>
            <p className="page-subtitle">Manage your floorball matches, track live games, and organize your season</p>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="error-alert">
            <span className="error-icon">⚠️</span>
            <span className="error-text">{error}</span>
            <button onClick={() => setError(null)} className="error-close">×</button>
          </div>
        )}

        {/* Stats and Filter Section */}
        <div className="stats-section">
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-number">{filteredMatches.length}</div>
              <div className="stat-label">{selectedSeasonId ? 'Season Matches' : 'Total Matches'}</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{matches.filter(m => m.status === 'Scheduled').length}</div>
              <div className="stat-label">Scheduled</div>
              <div className="stat-indicator scheduled"></div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{matches.filter(m => m.status === 'InProgress').length}</div>
              <div className="stat-label">In Progress</div>
              <div className="stat-indicator progress"></div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{matches.filter(m => m.status === 'Completed').length}</div>
              <div className="stat-label">Completed</div>
              <div className="stat-indicator completed"></div>
            </div>
          </div>

          <div className="filter-section">
            <label htmlFor="season-filter">Filter by Season:</label>
            <select
              id="season-filter"
              value={selectedSeasonId}
              onChange={(e) => setSelectedSeasonId(e.target.value)}
              className="season-filter"
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

        {/* Matches Table */}
        <div className="matches-section">
          <div className="section-header">
            <h2>Matches</h2>
          </div>
          
          {filteredMatches.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <h3>No matches found</h3>
              <p>{selectedSeasonId ? 'No matches found for the selected season' : 'Create your first match to get started'}</p>
              <button onClick={() => setShowCreateForm(true)} className="create-button">
                Create New Match
              </button>
            </div>
          ) : (
            <div className="matches-table-container">
              <table className="matches-table">
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
                  {filteredMatches.map((match) => (
                    <tr key={match.id}>
                      <td className="match-cell">
                        <div className="match-teams">
                          {match.homeTeamName} vs {match.awayTeamName}
                        </div>
                      </td>
                      <td className="date-cell">
                        {formatDateTime(match.scheduledDateTime)}
                      </td>
                      <td className="venue-cell">
                        {match.venue || <span className="tbd">TBD</span>}
                      </td>
                      <td className="score-cell">
                        {match.status === 'Scheduled' ? (
                          <span className="no-score">-</span>
                        ) : (
                          <span className="score">{match.homeScore} - {match.awayScore}</span>
                        )}
                      </td>
                      <td className="status-cell">
                        <span className={getStatusBadge(match.status)}>
                          {match.status}
                        </span>
                      </td>
                      <td className="actions-cell">
                        <div className="action-buttons">
                          <button
                            onClick={() => handleLiveMatch(match)}
                            className={liveMatches.has(match.id) ? "live-button" : "go-live-button"}
                            disabled={actionLoading !== null}
                          >
                            {liveMatches.has(match.id) ? "🔴 Live" : "🟢 Go Live"}
                          </button>
                          <button
                            onClick={() => handleEditMatch(match)}
                            className="edit-button"
                            disabled={actionLoading !== null}
                          >
                            ✏️ Edit
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Create Match Modal */}
        {showCreateForm && (
          <div className="modal-overlay">
            <div className="modal">
              <div className="modal-header">
                <h2>Create New Match</h2>
                <button onClick={() => setShowCreateForm(false)} className="modal-close">×</button>
              </div>
              <form onSubmit={handleCreateMatch} className="modal-form">
                <div className="form-group">
                  <label htmlFor="season">Season *</label>
                  <select
                    id="season"
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
                
                <div className="form-group">
                  <label htmlFor="homeTeam">Home Team *</label>
                  <select
                    id="homeTeam"
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
                
                <div className="form-group">
                  <label htmlFor="awayTeam">Away Team *</label>
                  <select
                    id="awayTeam"
                    value={createForm.awayTeamId}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, awayTeamId: e.target.value }))}
                    required
                  >
                    <option value="">Select Away Team</option>
                    {teams.map(team => (
                      <option key={team.id} value={team.id}>
                        {team.name}
                      </option>
                    ))}
                  </select>
                </div>
                
                <div className="form-group">
                  <label htmlFor="dateTime">Date & Time *</label>
                  <input
                    type="datetime-local"
                    id="dateTime"
                    value={createForm.scheduledDateTime}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, scheduledDateTime: e.target.value }))}
                    required
                  />
                </div>
                
                <div className="form-group">
                  <label htmlFor="venue">Venue</label>
                  <input
                    type="text"
                    id="venue"
                    value={createForm.venue}
                    onChange={(e) => setCreateForm(prev => ({ ...prev, venue: e.target.value }))}
                    placeholder="Enter venue (optional)"
                  />
                </div>
                
                <div className="modal-actions">
                  <button type="button" onClick={() => setShowCreateForm(false)} className="cancel-button">
                    Cancel
                  </button>
                  <button type="submit" disabled={actionLoading === 'create'} className="submit-button">
                    {actionLoading === 'create' ? 'Creating...' : 'Create Match'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MatchManagementPage; 