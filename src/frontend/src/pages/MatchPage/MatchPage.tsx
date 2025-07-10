import { useParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './MatchPage.scss';
import { signalRService } from '../../services/signalRService';
import type { MatchEvent } from '../../services/signalRService';
import PageTemplate from '../../components/PageTemplate/PageTemplate';

// SignalR event names used by the backend. Consider centralising these to a constants file if reused elsewhere.
const GOAL_SCORED_EVENT = 'FloorballGoalScored';
const PENALTY_ASSIGNED_EVENT = 'FloorballPenaltyAssigned';

type TabType = 'summary' | 'stats' | 'h2h' | 'table';

function formatDate(dateString: string) {
  const date = new Date(dateString);
  const formattedDate = date.toLocaleDateString('fi-FI', {
    day: 'numeric',
    month: 'numeric',
    year: 'numeric'
  });
  const formattedTime = date.toLocaleTimeString('fi-FI', {
    hour: '2-digit',
    minute: '2-digit'
  });
  return `${formattedDate} ${formattedTime}`;
}

function getTeamInitials(name: string): string {
  return name
    .split(' ')
    .map(word => word.charAt(0))
    .join('')
    .substring(0, 3)
    .toUpperCase();
}

export default function MatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('summary');

  useEffect(() => {
    // Helper that loads the latest state of the match from the API.
    const loadMatch = async () => {
      if (!id) return;
      try {
        const response = await floorballMatchService.getById(id);
        setMatch(response.data);
      } catch (err) {
        console.error(err);
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    };

    const fetchMatch = async () => {
      if (!id) return;
      try {
        setLoading(true);
        await loadMatch();
      } catch (err) {
        console.error(err);
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    };

    fetchMatch();

    // -------------------- SignalR Live Updates --------------------
    let unsubscribe: () => void;

    const setupSignalR = async () => {
      try {
        await signalRService.connect();

        // Subscribe only to goal / penalty events.
        await signalRService.subscribeToEventType(GOAL_SCORED_EVENT);
        await signalRService.subscribeToEventType(PENALTY_ASSIGNED_EVENT);

        // Register callback for incoming events.
        unsubscribe = signalRService.onMatchEvent((evt: MatchEvent) => {
          // Narrow down to the events we care about
          if (evt.eventType !== GOAL_SCORED_EVENT && evt.eventType !== PENALTY_ASSIGNED_EVENT) return;

          // The backend payload structure is known: it contains matchId.
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const payload = evt.data as any;
          if (!payload || payload.matchId !== id) return; // Not our match

          // Simply reload the match so UI stays consistent.
          // This avoids duplicating score-update logic client-side.
          loadMatch();
        });
      } catch (err) {
        console.error('Error setting up SignalR on MatchPage:', err);
      }
    };

    setupSignalR();

    // Cleanup on unmount.
    return () => {
      if (unsubscribe) unsubscribe();
      signalRService.unsubscribeFromEventType(GOAL_SCORED_EVENT);
      signalRService.unsubscribeFromEventType(PENALTY_ASSIGNED_EVENT);
    };
  }, [id]);

  if (loading) {
    return (
      <div className="match-page">
        <div className="loading">Loading match...</div>
      </div>
    );
  }

  if (error || !match) {
    return (
      <div className="match-page">
        <div className="error">{error || 'Match not found'}</div>
      </div>
    );
  }

  const renderBreadcrumb = () => (
    <div className="breadcrumb">
      <span>🏆 JALKAPALLO</span>
      <span>🇪🇺 EUROOPPA</span>
      <span>EUROOPPA-LIIGA · KARSINTA · NELJÄNNESVÄLIERÄT</span>
      <a href="#" className="settings-link">⚙️ Uusi ikkuna</a>
    </div>
  );

  const renderMatchHeader = () => (
    <div className="match-header">
      <div className="match-date-time">{formatDate(match.scheduledDateTime)}</div>
      
      <div className="teams-container">
        <div className="team-section home">
          <div className="team-crest">
            {getTeamInitials(match.homeTeamName)}
          </div>
          <div className="team-info">
            <div className="team-name">{match.homeTeamName}</div>
          </div>
        </div>

        <div className="score-container">
          {match.status === FloorballMatchStatus.Scheduled ? (
            <div className="vs-separator">—</div>
          ) : (
            <div className="match-score">
              <span className="home-score">{match.homeScore}</span>
              <span className="score-separator">—</span>
              <span className="away-score">{match.awayScore}</span>
            </div>
          )}
        </div>

        <div className="team-section away">
          <div className="team-crest">
            {getTeamInitials(match.awayTeamName)}
          </div>
          <div className="team-info">
            <div className="team-name">{match.awayTeamName}</div>
          </div>
        </div>
      </div>

      {match.status === FloorballMatchStatus.Scheduled && (
        <div className="match-status">
          <span className="status-indicator">⏰</span>
          <span>1. osapuolli.</span>
        </div>
      )}
    </div>
  );

  const renderNavigation = () => (
    <div className="navigation-tabs">
      <button 
        className={`nav-tab ${activeTab === 'summary' ? 'active' : ''}`}
        onClick={() => setActiveTab('summary')}
      >
        YHTEENVETO
      </button>
      <button 
        className={`nav-tab ${activeTab === 'stats' ? 'active' : ''}`}
        onClick={() => setActiveTab('stats')}
      >
        KERTOIMET
      </button>
      <button 
        className={`nav-tab ${activeTab === 'h2h' ? 'active' : ''}`}
        onClick={() => setActiveTab('h2h')}
      >
        H2H
      </button>
      <button 
        className={`nav-tab ${activeTab === 'table' ? 'active' : ''}`}
        onClick={() => setActiveTab('table')}
      >
        KAAVIO
      </button>
    </div>
  );

  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              <div className="match-info">
                {match.venue && (
                  <p>📍 Venue: {match.venue}</p>
                )}
                <p>Status: {match.status}</p>
                {match.wentToOvertime && <p>⏱️ Went to overtime</p>}
                {match.wentToShootout && <p>🥅 Went to shootout</p>}
              </div>
              
              {Object.keys(match.periodScores).length > 0 && (
                <div className="period-scores">
                  <h3>Period Scores</h3>
                  {Object.entries(match.periodScores).map(([period, scores]) => (
                    <div key={period} className="period-score">
                      <span>Period {period}:</span>
                      <span>{scores.homeScore} - {scores.awayScore}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        );
      
      case 'stats':
        return (
          <div className="tab-content">
            <div className="stats-placeholder">
              <h3>Kertoimet</h3>
              <p>Betting odds and statistics coming soon...</p>
            </div>
          </div>
        );
      
      case 'h2h':
        return (
          <div className="tab-content">
            <div className="h2h-placeholder">
              <h3>Head to Head</h3>
              <p>Historical match data between these teams coming soon...</p>
            </div>
          </div>
        );
      
      case 'table':
        return (
          <div className="tab-content">
            <div className="table-placeholder">
              <h3>Kaavio</h3>
              <p>League table and standings coming soon...</p>
            </div>
          </div>
        );
      
      default:
        return null;
    }
  };

  return (<>
    <PageTemplate title="Match Page">
    <div className="match-page">
      {renderBreadcrumb()}
      {renderMatchHeader()}
      {renderNavigation()}
      {renderTabContent()}
    </div>
    </PageTemplate>
    </>
  );
}