import { useState, useEffect, useCallback, useMemo } from 'react';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { signalRService, type MatchEvent } from '../../../../services/signalRService';
import MatchStatsCards from './Components/MatchStatsCards/MatchStatsCards';
import MatchFilters from './Components/MatchFilters/MatchFilters';
import CollapsibleMatchSection from './Components/CollapsibleMatchSection/CollapsibleMatchSection';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import './MatchOverviewPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { useTranslation } from 'react-i18next';
import { useNavigate, Link } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
  
const MatchOverviewPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  // State management
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFiltering, setIsFiltering] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');

  // Collapsible sections state
  const [collapsedSections, setCollapsedSections] = useState({
    ongoing: false,
    scheduled: false,
    completed: false,
    cancelled: false
  });

  // Real-time connection status
  const [signalRConnected, setSignalRConnected] = useState(false);

  // Fetch all required data
  const fetchData = useCallback(async (isInitialLoad = false) => {
    try {
      // Only show full loading on initial load
      if (isInitialLoad) {
        setLoading(true);
      } else {
        setIsFiltering(true);
      }
      setError(null);

      const [seasonsResponse, matchesResponse] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballMatchService.getAll({ 
          pageSize: 100,
          seasonId: selectedSeasonId || undefined,
          searchQuery: searchQuery.trim() || undefined
        })
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
      setIsFiltering(false);
    }
  }, [selectedSeasonId, searchQuery]);

  // Filter and sort matches by status: ongoing, scheduled (next 7 days), completed (latest 10)
  const filteredMatches = useMemo(() => {
    const now = new Date();
    const oneWeekFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    
    // Backend already filtered by season and search query
    const filtered = matches;
    
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

    const cancelledMatches = filtered
      .filter(match => match.status === 'Cancelled')
      .sort((a, b) => {
        const dateA = new Date(a.scheduledDateTime);
        const dateB = new Date(b.scheduledDateTime);
        return dateB.getTime() - dateA.getTime();
      }).slice(0, 10);

    return {
      ongoing: ongoingMatches,
      scheduled: scheduledMatches,
      completed: completedMatches,
      cancelled: cancelledMatches
    };
  }, [matches]);

  // Initial load
  useEffect(() => {
    fetchData(true);
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Debounced fetch for filters - wait 500ms after user stops typing
  useEffect(() => {
    // Skip initial load
    if (loading) return;

    const timer = setTimeout(() => {
      fetchData(false);
    }, 500);

    return () => clearTimeout(timer);
  }, [selectedSeasonId, searchQuery]); // eslint-disable-line react-hooks/exhaustive-deps

  // SignalR subscription management
  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    // Handle match status changes
          const handleMatchStatusChange = (eventData: MatchEvent) => {
        const { MatchId, NewStatus } = eventData.data as { MatchId: string; NewStatus: string };
      
              setMatches(prev => prev.map(match => {
          if (match.id === MatchId) {
            return { ...match, status: NewStatus as FloorballMatchDto['status'] };
          }
          return match;
        }));
      
      console.log(`Match ${MatchId} status changed to ${NewStatus}`);
    };

    // Handle real-time SignalR events
    const handleSignalREvent = (event: MatchEvent) => {
      console.log('Received SignalR event in MatchOverviewPage:', event);
      
      switch (event.eventType) {
        case 'FloorballMatchStatusChangedEvent':
          handleMatchStatusChange(event);
          break;
        default:
          // Ignore other event types
          break;
      }
    };

    const setupSignalR = async () => {
      try {
        console.log('Setting up SignalR connection for MatchOverviewPage...');
        
        // Test backend accessibility first
        const isBackendAccessible = await signalRService.testBackendAccessibility();
        if (!isBackendAccessible) {
          console.warn('Backend is not accessible, skipping SignalR setup');
          setSignalRConnected(false);
          return;
        }
        
        // Connect to SignalR
        await signalRService.connect();
        
        // Update connection status
        setSignalRConnected(signalRService.isConnected);
        
        if (!signalRService.isConnected) {
          console.warn('SignalR connection failed, skipping subscriptions');
          return;
        }
        
        // Subscribe to match status change events
        await signalRService.subscribeToEventType('FloorballMatchStatusChangedEvent');
        
        // Set up event handler
        unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        
        // Set up connection state monitoring
        const checkConnectionStatus = () => {
          setSignalRConnected(signalRService.isConnected);
        };
        
        // Check connection status every 5 seconds
        const connectionInterval = setInterval(checkConnectionStatus, 5000);
        
        console.log('SignalR subscriptions set up for MatchOverviewPage');
        
        return () => {
          clearInterval(connectionInterval);
        };
      } catch (error) {
        console.error('Error setting up SignalR subscriptions:', error);
        setSignalRConnected(false);
        // Don't set error - SignalR is not critical for basic functionality
      }
    };

    setupSignalR().then(cleanupInterval => {
      return () => {
        // Cleanup SignalR subscriptions
        if (unsubscribe) {
          unsubscribe();
        }
        
        // Cleanup connection interval
        if (cleanupInterval) {
          cleanupInterval();
        }
        
        // Unsubscribe from event types
        signalRService.unsubscribeFromEventType('FloorballMatchStatusChangedEvent');
      };
    });
  }, []);

  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.matches.title', 'Manage matches')}>
      <div className="match-management">
        <div className="match-management__content">
          <div className="loading-spinner">
            <div className="spinner"></div>
            <p>{t('floorball.matches.loading', 'Loading matches...')}</p>
          </div>
        </div>
      </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.matches.title', 'Manage matches')}>
    <div className="match-management">
      <div className="match-management__content">
        {/* Header Section */}
        <div className="page-header">
          <div className="page-header__top">
            <BackButton 
              to="/admin/floorball" 
              text={t('common.back', 'Back to Floorball Management')} 
            />
            {/* Real-time Status Indicator */}
            <div className={`realtime-status ${signalRConnected ? 'connected' : 'disconnected'}`}>
              <span className="status-dot"></span>
              <span className="status-text">
                {signalRConnected ? 'Real-time updates active' : 'Real-time updates offline'}
              </span>
            </div>
          </div>
          <div className="page-header__main">
            <h1 className="page-title">{t('floorball.matches.title', 'Match Management')}</h1>
            <p className="page-subtitle">{t('floorball.matches.subtitle', 'Manage your floorball matches, track live games, and organize your season')}</p>
          </div>
        </div>

        {/* Error Message */}
        <ErrorPopup message={error} />

        {/* Stats and Filter Section */}
        <MatchStatsCards 
          allMatches={matches}
          filteredMatches={filteredMatches}
          selectedSeasonId={selectedSeasonId}
          onCreateNew={() => navigate('/admin/floorball/matches/create')}
          onCompletedClick={() => navigate('/admin/floorball/matches/completed')}
          onScheduledClick={() => navigate('/admin/floorball/matches/scheduled')}
          onInProgressClick={() => navigate('/admin/floorball/matches/in-progress')}
          onCancelledClick={() => navigate('/admin/floorball/matches/cancelled')}
        />

        <MatchFilters 
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
        />

        {/* Filtering indicator */}
        {isFiltering && (
          <div style={{ 
            padding: '12px', 
            textAlign: 'center', 
            background: '#f3f4f6', 
            borderRadius: '8px',
            marginBottom: '16px',
            color: '#6b7280',
            fontSize: '0.875rem'
          }}>
            <span style={{ marginRight: '8px' }}>🔍</span>
            Searching...
          </div>
        )}

        {/* Matches Sections */}
        <div className="matches-section" style={{ opacity: isFiltering ? 0.6 : 1, transition: 'opacity 0.2s' }}>
          {/* Ongoing Matches Section */}
          <CollapsibleMatchSection
            title={`Ongoing Matches (${filteredMatches.ongoing.length})`}
            matches={filteredMatches.ongoing}
            isCollapsed={collapsedSections.ongoing}
            onToggleCollapse={() => toggleSection('ongoing')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
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
            sectionType="completed"
          />

          {/* Cancelled Matches Section */}
          <CollapsibleMatchSection
            title={`Cancelled Matches (${filteredMatches.cancelled.length})`}
            matches={filteredMatches.cancelled}
            isCollapsed={collapsedSections.cancelled}
            onToggleCollapse={() => toggleSection('cancelled')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            sectionType="cancelled"
          />

          {/* Empty State - when no matches in any section */}
          {filteredMatches.ongoing.length === 0 && 
           filteredMatches.scheduled.length === 0 && 
           filteredMatches.completed.length === 0 && 
           filteredMatches.cancelled.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <h3>No matches found</h3>
              <p>{selectedSeasonId ? 'No matches found for the selected season' : 'Create your first match to get started'}</p>
              <Link to="/admin/floorball/matches/create" className="create-button">
                Create New Match
              </Link>
            </div>
          )}
        </div>

      </div>
    </div>
    </PageTemplate>
  );
};

export default MatchOverviewPage; 