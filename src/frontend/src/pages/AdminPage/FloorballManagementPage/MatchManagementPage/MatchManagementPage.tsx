import { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import Navbar from '../../../../components/Navigation/Navbar';
import LiveMatchModal from './Components/LiveMatchModal/LiveMatchModal';
import CreateMatchModal from './Components/CreateMatchModal/CreateMatchModal';
import MatchStatsCards from './Components/MatchStatsCards/MatchStatsCards';
import MatchFilters from './Components/MatchFilters/MatchFilters';
import { useLiveMatchState } from './hooks/useLiveMatchState';
import { formatDateTime, getStatusBadge } from './utils/matchFormatters';
import type { 
  FloorballMatchDto, 
  CreateFloorballMatchRequest
} from '../../../../types/floorball/floorballTypes';
import './MatchManagementPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';
import { useTranslation } from 'react-i18next';
  
const MatchManagementPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  // State management
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  
  // Live match modal state
  const [liveModalMatch, setLiveModalMatch] = useState<FloorballMatchDto | null>(null);
  const [isLiveModalOpen, setIsLiveModalOpen] = useState(false);
  
  // Use the live match state hook
  const {
    liveMatches,
    initializeLiveMatch,
    updateLiveMatchState,
    cancelLiveMatch,
    getLiveMatchState,
  }: ReturnType<typeof useLiveMatchState> = useLiveMatchState();

  // Form state
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');

  // Fetch all required data
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const [seasonsResponse, matchesResponse] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballMatchService.getAll({ pageSize: 100 })
      ]);

      if (seasonsResponse.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
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
  const filteredMatches = useMemo(() => {
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

  const handleCreateMatch = async (matchData: CreateFloorballMatchRequest) => {
    try {
      setActionLoading('create');
      setError(null);

      const response = await floorballMatchService.create(matchData);
      
      if (response.success && response.data) {
        // Fetch the complete match data to ensure we have the correct team names
        const completeMatchResponse = await floorballMatchService.getById(response.data.id);
        
        if (completeMatchResponse.success && completeMatchResponse.data) {
          setMatches(prev => [...prev, completeMatchResponse.data!]);
        } else {
          // Fallback to the original response if fetching complete data fails
          setMatches(prev => [...prev, response.data!]);
        }
        setShowCreateForm(false);
      }

    } catch (error) {
      console.error('Error creating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to create match');
      throw error; // Re-throw so the modal can handle it
    } finally {
      setActionLoading(null);
    }
  };

  const handleLiveMatch = (match: FloorballMatchDto) => {
    setLiveModalMatch(match);
    setIsLiveModalOpen(true);
  };

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const handleCloseLiveModal = () => {
    setIsLiveModalOpen(false);
    setLiveModalMatch(null);
  };



  const handleGoLive = (matchId: string, updatedMatch?: FloorballMatchDto) => {
    // Use the hook to initialize live match
    initializeLiveMatch(liveModalMatch!);
    
    // Update match with the response from the backend
    if (updatedMatch) {
      setMatches((prev: FloorballMatchDto[]) => prev.map((m: FloorballMatchDto) => 
        m.id === matchId ? updatedMatch : m
      ));
      setLiveModalMatch(updatedMatch);
    }
  };

  const handleCompleteLive = (matchId: string, updatedMatch?: FloorballMatchDto) => {
    // Use the hook to cancel live match
    cancelLiveMatch(matchId);
    
    // Update match with the response from the backend
    if (updatedMatch) {
      setMatches((prev: FloorballMatchDto[]) => prev.map((m: FloorballMatchDto) => 
        m.id === matchId ? updatedMatch : m
      ));
      setLiveModalMatch(updatedMatch);
    }
    
    // Don't close the modal - let it stay open with "Match Finished" status
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
            <BackButton 
              to="/admin/floorball" 
              text={t('common.back', 'Back to Floorball Management')} 
            />
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
        <MatchStatsCards 
          allMatches={matches}
          filteredMatches={filteredMatches}
          selectedSeasonId={selectedSeasonId}
          onCreateNew={() => setShowCreateForm(true)}
        />

        <MatchFilters 
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
        />

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
                  {filteredMatches.map((match: FloorballMatchDto) => (
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
                            {liveMatches.has(match.id) ? "🔴 Live" : "📊 Manage"}
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
        <CreateMatchModal
          isOpen={showCreateForm}
          onClose={() => setShowCreateForm(false)}
          onSubmit={handleCreateMatch}
          loading={actionLoading === 'create'}
        />

        {/* Live Match Modal */}
        {liveModalMatch && (
          <LiveMatchModal
            match={liveModalMatch}
            isOpen={isLiveModalOpen}
            onClose={handleCloseLiveModal}
            onCompleteLive={handleCompleteLive}
            onGoLive={handleGoLive}
            liveState={getLiveMatchState(liveModalMatch.id)}
            onStateUpdate={(updates) => updateLiveMatchState(liveModalMatch.id, updates)}
            isLive={liveMatches.has(liveModalMatch.id)}
            isFinished={liveModalMatch.status === 'Completed'}
          />
        )}
      </div>
    </div>
  );
};

export default MatchManagementPage; 