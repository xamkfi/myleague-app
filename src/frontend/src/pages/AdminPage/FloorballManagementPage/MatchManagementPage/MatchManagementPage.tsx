import { useState, useEffect, useCallback, useMemo } from 'react';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import Navbar from '../../../../components/Navigation/Navbar';
import LiveMatchModal from './Components/LiveMatchModal/LiveMatchModal';
import MatchFormModal from './Components/MatchFormModal/MatchFormModal';
import MatchStatsCards from './Components/MatchStatsCards/MatchStatsCards';
import MatchFilters from './Components/MatchFilters/MatchFilters';
import CollapsibleMatchSection from './Components/CollapsibleMatchSection/CollapsibleMatchSection';
import { useLiveMatchState } from './hooks/useLiveMatchState';
import type { 
  FloorballMatchDto, 
  CreateFloorballMatchRequest,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest
} from '../../../../types/floorball/floorballTypes';
import './MatchManagementPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';
import { useTranslation } from 'react-i18next';
  
const MatchManagementPage = () => {
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
  const [showForm, setShowForm] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');
  const [editMatch, setEditMatch] = useState<FloorballMatchDto | undefined>(undefined);
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');

  // Collapsible sections state
  const [collapsedSections, setCollapsedSections] = useState({
    ongoing: false,
    scheduled: false,
    completed: false
  });

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

  // Filter and sort matches by status: ongoing, scheduled (next 7 days), completed (latest 10)
  const filteredMatches = useMemo(() => {
    const now = new Date();
    const oneWeekFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    
    // First filter by season if selected
    const filtered = selectedSeasonId 
      ? matches.filter(match => match.seasonId === selectedSeasonId)
      : matches;
    
    // Separate by status
    const ongoingMatches = filtered.filter(match => match.status === 'InProgress');
    
    const scheduledMatches = filtered.filter(match => {
      if (match.status !== 'Scheduled') return false;
      const matchDate = new Date(match.scheduledDateTime);
      return matchDate <= oneWeekFromNow;
    }).sort((a, b) => {
      const dateA = new Date(a.scheduledDateTime);
      const dateB = new Date(b.scheduledDateTime);
      return dateA.getTime() - dateB.getTime();
    });
    
    const completedMatches = filtered
      .filter(match => match.status === 'Completed')
      .sort((a, b) => {
        const dateA = new Date(a.scheduledDateTime);
        const dateB = new Date(b.scheduledDateTime);
        return dateB.getTime() - dateA.getTime();
      })
      .slice(0, 10); // Only show 10 most recent
    
    return {
      ongoing: ongoingMatches,
      scheduled: scheduledMatches,
      completed: completedMatches
    };
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
        setShowForm(false);
      }

    } catch (error) {
      console.error('Error creating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to create match');
      throw error; // Re-throw so the modal can handle it
    } finally {
      setActionLoading(null);
    }
  };

  const handleUpdateMatch = async (updateData: ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest) => {
    if (!editMatch) return;

    try {
      setActionLoading('edit');
      setError(null);

      let response;
      
      if ('seasonId' in updateData) {
        response = await floorballMatchService.changeSeason(editMatch.id, updateData.seasonId);
      } else if ('homeTeamId' in updateData && 'awayTeamId' in updateData) {
        response = await floorballMatchService.changeTeams(editMatch.id, updateData.homeTeamId, updateData.awayTeamId);
      } else if ('venue' in updateData) {
        response = await floorballMatchService.changeVenue(editMatch.id, updateData.venue);
      } else if ('scheduledDateTime' in updateData) {
        response = await floorballMatchService.changeDateTime(editMatch.id, updateData.scheduledDateTime);
      } else {
        throw new Error('Invalid update data');
      }

      if (response.success && response.data) {
        // Update the match in the list
        setMatches(prev => prev.map(match => 
          match.id === editMatch.id ? response.data! : match
        ));
        setShowForm(false);
        setEditMatch(undefined);
      }

    } catch (error) {
      console.error('Error updating match:', error);
      setError(error instanceof Error ? error.message : 'Failed to update match');
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
    setEditMatch(match);
    setFormMode('edit');
    setShowForm(true);
  };

  const handleCreateNew = () => {
    setEditMatch(undefined);
    setFormMode('create');
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditMatch(undefined);
    setFormMode('create');
  };

  const handleFormSubmit = async (matchData: CreateFloorballMatchRequest | ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest) => {
    if (formMode === 'create') {
      await handleCreateMatch(matchData as CreateFloorballMatchRequest);
    } else {
      await handleUpdateMatch(matchData);
    }
  };

  const handleCloseLiveModal = () => {
    setIsLiveModalOpen(false);
    setLiveModalMatch(null);
  };

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  const handleGoLive = (matchId: string, updatedMatch?: FloorballMatchDto) => {
    // Helper function to detect placeholder team names
    const isPlaceholderTeamName = (name: string) => {
      return !name || name.trim() === '' || name === 'Home Team' || name === 'Away Team';
    };
    
    // Update match with the response from the backend first
    if (updatedMatch) {
      const originalMatch = matches.find(m => m.id === matchId);
      
      setMatches((prev: FloorballMatchDto[]) => prev.map((m: FloorballMatchDto) => {
        if (m.id === matchId) {
          // Preserve team names from the original match if they're missing, empty, or placeholder values
          const preservedMatch = {
            ...updatedMatch,
            homeTeamName: !isPlaceholderTeamName(updatedMatch.homeTeamName) ? updatedMatch.homeTeamName : m.homeTeamName,
            awayTeamName: !isPlaceholderTeamName(updatedMatch.awayTeamName) ? updatedMatch.awayTeamName : m.awayTeamName
          };
          return preservedMatch;
        }
        return m;
      }));
      
      // Create the preserved match for the modal
      const preservedUpdatedMatch = {
        ...updatedMatch,
        homeTeamName: !isPlaceholderTeamName(updatedMatch.homeTeamName) ? updatedMatch.homeTeamName : (originalMatch?.homeTeamName || ''),
        awayTeamName: !isPlaceholderTeamName(updatedMatch.awayTeamName) ? updatedMatch.awayTeamName : (originalMatch?.awayTeamName || '')
      };
      
      setLiveModalMatch(preservedUpdatedMatch);
      
      // Use the preserved match data to initialize live match state
      initializeLiveMatch(preservedUpdatedMatch);
    } else {
      // Fallback to current modal match if no updated data
      initializeLiveMatch(liveModalMatch!);
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
          onCreateNew={handleCreateNew}
        />

        <MatchFilters 
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
        />

        {/* Matches Sections */}
        <div className="matches-section">
          {/* Ongoing Matches Section */}
          <CollapsibleMatchSection
            title={`Ongoing Matches (${filteredMatches.ongoing.length})`}
            matches={filteredMatches.ongoing}
            isCollapsed={collapsedSections.ongoing}
            onToggleCollapse={() => toggleSection('ongoing')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            actionLoading={actionLoading}
            liveMatches={liveMatches}
            sectionType="ongoing"
          />

          {/* Scheduled Matches Section */}
          <CollapsibleMatchSection
            title={`Scheduled Matches (${filteredMatches.scheduled.length})`}
            matches={filteredMatches.scheduled}
            isCollapsed={collapsedSections.scheduled}
            onToggleCollapse={() => toggleSection('scheduled')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            actionLoading={actionLoading}
            liveMatches={liveMatches}
            sectionType="scheduled"
          />

          {/* Completed Matches Section */}
          <CollapsibleMatchSection
            title={`Completed Matches (${filteredMatches.completed.length})`}
            matches={filteredMatches.completed}
            isCollapsed={collapsedSections.completed}
            onToggleCollapse={() => toggleSection('completed')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            actionLoading={actionLoading}
            liveMatches={liveMatches}
            sectionType="completed"
          />

          {/* Empty State - when no matches in any section */}
          {filteredMatches.ongoing.length === 0 && 
           filteredMatches.scheduled.length === 0 && 
           filteredMatches.completed.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <h3>No matches found</h3>
              <p>{selectedSeasonId ? 'No matches found for the selected season' : 'Create your first match to get started'}</p>
              <button onClick={handleCreateNew} className="create-button">
                Create New Match
              </button>
            </div>
          )}
        </div>

        {/* Match Form Modal */}
        <MatchFormModal
          isOpen={showForm}
          onClose={handleCloseForm}
          mode={formMode}
          initialData={editMatch}
          onSubmit={handleFormSubmit}
          loading={actionLoading !== null}
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