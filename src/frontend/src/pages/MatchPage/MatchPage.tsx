import { useParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { FloorballMatchStatus, type FloorballMatchDto, type FloorballGoalEventDto, type FloorballPenaltyEventDto } from '../../types/floorball/floorballTypes';
import './MatchPage.scss';
import { useSignalR } from '../../hooks/useSignalR';
import type { MatchEvent } from '../../services/signalRService';
import PageTemplate from '../../components/PageTemplate/PageTemplate';

// SignalR event names used by the backend. Consider centralising these to a constants file if reused elsewhere.
const GOAL_SCORED_EVENT = 'FloorballGoalScored';
const PENALTY_ASSIGNED_EVENT = 'FloorballPenaltyAssigned';

type TabType = 'summary' | 'stats' | 'h2h' | 'table';

type MatchEventItem = {
  type: 'goal' | 'penalty';
  time: number;
  periodNumber: number;
  event: FloorballGoalEventDto | FloorballPenaltyEventDto;
};

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

function formatTime(timeInSeconds: number): string {
  const minutes = Math.floor(timeInSeconds / 60);
  const seconds = timeInSeconds % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

function getTeamName(teamId: string, match: FloorballMatchDto): string {
  if (teamId === match.homeTeamId) return match.homeTeamName;
  if (teamId === match.awayTeamId) return match.awayTeamName;
  return 'Unknown Team';
}

interface MatchEventsProps {
  match: FloorballMatchDto;
}

function MatchEvents({ match }: MatchEventsProps) {
  // Combine all events and sort by time within each period
  const allEvents: MatchEventItem[] = [
    ...match.goalEvents.map(goal => ({
      type: 'goal' as const,
      time: goal.timeInSeconds,
      periodNumber: goal.periodNumber,
      event: goal
    })),
    ...match.penaltyEvents.map(penalty => ({
      type: 'penalty' as const,
      time: penalty.timeInSeconds,
      periodNumber: penalty.periodNumber,
      event: penalty
    }))
  ];

  // Group events by period
  const eventsByPeriod = allEvents.reduce((acc, event) => {
    if (!acc[event.periodNumber]) {
      acc[event.periodNumber] = [];
    }
    acc[event.periodNumber].push(event);
    return acc;
  }, {} as Record<number, MatchEventItem[]>);

  // Sort events within each period by time
  Object.keys(eventsByPeriod).forEach(period => {
    eventsByPeriod[parseInt(period)].sort((a, b) => a.time - b.time);
  });

  const getPeriodScore = (period: number) => {
    const scores = match.periodScores[period];
    return scores ? `${scores.homeScore} - ${scores.awayScore}` : '0 - 0';
  };

  const getPeriodName = (period: number) => {
    if (period <= 3) return `${period}${period === 1 ? 'ST' : period === 2 ? 'ND' : 'RD'} PERIOD`;
    return `${period === 4 ? 'OVERTIME' : `PERIOD ${period}`}`;
  };

  const isHomeTeam = (teamId: string) => teamId === match.homeTeamId;

  const renderGoalEvent = (event: FloorballGoalEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
            <div className="event-icon goal">⚽</div>
            <span className="event-score">{getPeriodScore(event.periodNumber)}</span>
            <span className="event-player">{event.playerName || 'Unknown Player'}</span>
            {event.assisterName && <span className="event-assist">{event.assisterName}</span>}
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            {event.assisterName && <span className="event-assist">{event.assisterName}</span>}
            <span className="event-player">{event.playerName || 'Unknown Player'}</span>
            <span className="event-score">{getPeriodScore(event.periodNumber)}</span>
            <div className="event-icon goal">⚽</div>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
          </>
        )}
      </div>
    </div>
  );

  const renderPenaltyEvent = (event: FloorballPenaltyEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
            <div className="event-icon penalty">🟨</div>
            <span className="event-player">{event.playerName || 'Team penalty'}</span>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <span className="event-player">{event.playerName || 'Team penalty'}</span>
            <div className="event-icon penalty">🟨</div>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
          </>
        )}
      </div>
    </div>
  );

  return (
    <div className="match-events">
      {Object.keys(eventsByPeriod)
        .map(Number)
        .sort((a, b) => a - b)
        .map(period => (
          <div key={period} className="period-section">
            <div className="period-header">
              <span className="period-name">{getPeriodName(period)}</span>
              <span className="period-score">{getPeriodScore(period)}</span>
            </div>
            <div className="period-events">
              {eventsByPeriod[period].map((eventItem, index) => {
                const isHome = isHomeTeam(eventItem.event.teamId);
                return (
                  <div key={index}>
                    {eventItem.type === 'goal' 
                      ? renderGoalEvent(eventItem.event as FloorballGoalEventDto, isHome)
                      : renderPenaltyEvent(eventItem.event as FloorballPenaltyEventDto, isHome)
                    }
                  </div>
                );
              })}
            </div>
          </div>
        ))}
    </div>
  );
}

export default function MatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('summary');

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

  // SignalR integration
  useSignalR({
    eventTypes: [GOAL_SCORED_EVENT, PENALTY_ASSIGNED_EVENT],
    onEvent: (evt: MatchEvent) => {
      // Narrow down to the events we care about
      if (evt.eventType !== GOAL_SCORED_EVENT && evt.eventType !== PENALTY_ASSIGNED_EVENT) return;

      // The backend payload structure is known: it contains matchId.
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const payload = evt.data as any;
      if (!payload || payload.matchId !== id) return; // Not our match

      // Simply reload the match so UI stays consistent.
      // This avoids duplicating score-update logic client-side.
      loadMatch();
    },
    autoConnect: true
  });

  useEffect(() => {
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
      <span>🏒 FLOORBALL</span>
      <span>🇫🇮 FINLAND</span>
      <span>FLOORBALL LEAGUE · REGULAR SEASON</span>
      <a href="#" className="settings-link">⚙️ Settings</a>
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
            <div className="vs-separator">VS</div>
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

      {match.status === FloorballMatchStatus.InProgress && (
        <div className="match-status">
          <span className="status-indicator">🔴</span>
          <span>LIVE</span>
        </div>
      )}
      
      {match.status === FloorballMatchStatus.Completed && (
        <div className="match-status">
          <span className="status-indicator">✅</span>
          <span>FINAL</span>
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
        SUMMARY
      </button>
      <button 
        className={`nav-tab ${activeTab === 'stats' ? 'active' : ''}`}
        onClick={() => setActiveTab('stats')}
      >
        STATS
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
        TABLE
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
              
              <MatchEvents match={match} />
            </div>
          </div>
        );
      
      case 'stats':
        return (
          <div className="tab-content">
            <div className="stats-placeholder">
              <h3>Match Statistics</h3>
              <p>Detailed match statistics coming soon...</p>
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
              <h3>League Table</h3>
              <p>League table and standings coming soon...</p>
            </div>
          </div>
        );
      
      default:
        return null;
    }
  };

  return (
    <PageTemplate title="Match Details">
      <div className="match-page">
        {renderBreadcrumb()}
        {renderMatchHeader()}
        {renderNavigation()}
        {renderTabContent()}
      </div>
    </PageTemplate>
  );
}