import React, { useState, useEffect, useCallback } from 'react';
import { signalRService, type MatchEvent } from '../../../../../services/signalRService';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest 
} from '../../../../../api/floorball/floorballMatchEventService';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { LiveMatchState } from '../hooks/useLiveMatchState';
import './LiveMatchModal.scss';

interface LiveMatchModalProps {
  match: FloorballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onMatchUpdate: (updatedMatch: FloorballMatchDto) => void;
  onCancelLive?: (matchId: string) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
}



const LiveMatchModal: React.FC<LiveMatchModalProps> = ({ 
  match, 
  isOpen, 
  onClose, 
  onMatchUpdate: _onMatchUpdate,
  onCancelLive,
  liveState,
  onStateUpdate
}) => {
  // State management
  const [homeTeam, setHomeTeam] = useState<FloorballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FloorballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FloorballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Use state from parent or default values
  const currentScore = liveState?.currentScore || { home: match.homeScore, away: match.awayScore };
  const events = liveState?.events || [];
  const clock = liveState?.clock || {
    period: 1,
    minutes: 0,
    seconds: 0,
    isRunning: false
  };
  
  // Event recording state
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showPenaltyForm, setShowPenaltyForm] = useState(false);

  // Form states
  const [goalForm, setGoalForm] = useState({
    teamId: '',
    playerId: '',
    assisterId: '',
  });
  
  const [penaltyForm, setPenaltyForm] = useState({
    teamId: '',
    playerId: '',
    penaltyType: '',
    minutes: 2,
    description: '',
  });

  // Load team and player data
  useEffect(() => {
    if (isOpen) {
      loadTeamData();
      setupSignalR();
    }
    
    return () => {
      cleanupSignalR();
    };
  }, [isOpen, match.id]);

  const loadTeamData = async () => {
    try {
      setLoading(true);
      
      const [homeTeamData, awayTeamData] = await Promise.all([
        floorballTeamService.getById(match.homeTeamId),
        floorballTeamService.getById(match.awayTeamId)
      ]);
      
      setHomeTeam(homeTeamData);
      setAwayTeam(awayTeamData);
      
      // Load players for both teams
      const [homePlayersData, awayPlayersData] = await Promise.all([
        floorballPlayerService.getByTeamId(match.homeTeamId),
        floorballPlayerService.getByTeamId(match.awayTeamId)
      ]);
      
      setHomePlayers(homePlayersData);
      setAwayPlayers(awayPlayersData);
      
    } catch (error) {
      console.error('Error loading team data:', error);
      setError('Failed to load team data');
    } finally {
      setLoading(false);
    }
  };

  const setupSignalR = async () => {
    try {
      await signalRService.connect();
      await signalRService.subscribeToEventType('FloorballGoalScoredEvent');
      await signalRService.subscribeToEventType('FloorballPenaltyAssignedEvent');
      
      const unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
      
      return unsubscribe;
    } catch (error) {
      console.error('Error setting up SignalR:', error);
    }
  };

  const cleanupSignalR = async () => {
    try {
      await signalRService.unsubscribeFromEventType('FloorballGoalScoredEvent');
      await signalRService.unsubscribeFromEventType('FloorballPenaltyAssignedEvent');
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
    }
  };

  const handleSignalREvent = useCallback((event: MatchEvent) => {
    console.log('Received match event:', event);
    
    if (event.data.MatchId !== match.id) {
      return; // Event is not for this match
    }
    
    if (event.eventType === 'FloorballGoalScoredEvent') {
      handleGoalScored(event.data);
    } else if (event.eventType === 'FloorballPenaltyAssignedEvent') {
      handlePenaltyAssigned(event.data);
    }
  }, [match.id]);

  const handleGoalScored = (eventData: any) => {
    if (!onStateUpdate) return;
    
    // Update score
    const newScore = {
      home: eventData.TeamId === match.homeTeamId ? currentScore.home + 1 : currentScore.home,
      away: eventData.TeamId === match.awayTeamId ? currentScore.away + 1 : currentScore.away
    };
    
    // Add to events history
    const goalEvent = {
      id: `goal-${Date.now()}`,
      type: 'goal' as const,
      teamId: eventData.TeamId,
      teamName: eventData.TeamId === match.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
      playerId: eventData.PlayerId,
      assisterId: eventData.AssisterId,
      periodNumber: eventData.PeriodNumber,
      timeInSeconds: eventData.TimeInSeconds,
      timestamp: new Date(),
    };
    
    onStateUpdate({
      currentScore: newScore,
      events: [goalEvent, ...events]
    });
  };

  const handlePenaltyAssigned = (eventData: any) => {
    if (!onStateUpdate) return;
    
    const penaltyEvent = {
      id: `penalty-${Date.now()}`,
      type: 'penalty' as const,
      teamId: eventData.TeamId,
      teamName: eventData.TeamId === match.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
      playerId: eventData.PlayerId,
      periodNumber: eventData.PeriodNumber,
      timeInSeconds: eventData.TimeInSeconds,
      timestamp: new Date(),
      penaltyType: eventData.PenaltyType,
      penaltyMinutes: eventData.Minutes,
      description: eventData.Description,
    };
    
    onStateUpdate({
      events: [penaltyEvent, ...events]
    });
  };

  // Clock management
  const toggleClock = () => {
    if (!onStateUpdate) return;
    onStateUpdate({
      clock: { ...clock, isRunning: !clock.isRunning }
    });
  };

  const resetClock = () => {
    if (!onStateUpdate) return;
    onStateUpdate({
      clock: { ...clock, minutes: 0, seconds: 0, isRunning: false }
    });
  };

  const nextPeriod = () => {
    if (!onStateUpdate) return;
    onStateUpdate({
      clock: { 
        period: clock.period + 1, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      }
    });
  };

  const goBackTime = () => {
    if (!onStateUpdate) return;
    const totalSeconds = clock.minutes * 60 + clock.seconds;
    const newTotalSeconds = Math.max(0, totalSeconds - 5); // Don't go below 0
    const newMinutes = Math.floor(newTotalSeconds / 60);
    const newSeconds = newTotalSeconds % 60;
    
    onStateUpdate({
      clock: { 
        ...clock, 
        minutes: newMinutes, 
        seconds: newSeconds 
      }
    });
  };

  const goAheadTime = () => {
    if (!onStateUpdate) return;
    const totalSeconds = clock.minutes * 60 + clock.seconds;
    const newTotalSeconds = Math.min(1200, totalSeconds + 30); // Cap at 20 minutes (1200 seconds)
    const newMinutes = Math.floor(newTotalSeconds / 60);
    const newSeconds = newTotalSeconds % 60;
    
    onStateUpdate({
      clock: { 
        ...clock, 
        minutes: newMinutes, 
        seconds: newSeconds 
      }
    });
  };

  // Clock is now managed by parent component with persistent background timer

  // Event recording functions
  const recordGoal = async () => {
    if (!goalForm.teamId || !goalForm.playerId) {
      setError('Please select team and player');
      return;
    }
    
    try {
      setLoading(true);
      
      const goalData: RecordGoalEventRequest = {
        matchId: match.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: clock.period,
        timeInSeconds: clock.minutes * 60 + clock.seconds,
        wasInOvertime: clock.period > 3,
        wasInShootout: false, // TODO: Add shootout support
      };
      
      await floorballMatchEventService.recordGoal(goalData);
      
      // Reset form
      setGoalForm({ teamId: '', playerId: '', assisterId: '' });
      setShowGoalForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording goal:', error);
      setError(error instanceof Error ? error.message : 'Failed to record goal');
    } finally {
      setLoading(false);
    }
  };

  const recordPenalty = async () => {
    if (!penaltyForm.teamId || !penaltyForm.penaltyType) {
      setError('Please select team and penalty type');
      return;
    }
    
    try {
      setLoading(true);
      
      const penaltyData: RecordPenaltyEventRequest = {
        matchId: match.id,
        teamId: penaltyForm.teamId,
        playerId: penaltyForm.playerId || undefined,
        penaltyType: penaltyForm.penaltyType,
        durationMinutes: penaltyForm.minutes,
        periodNumber: clock.period,
        timeInSeconds: clock.minutes * 60 + clock.seconds,
        description: penaltyForm.description,
      };
      
      await floorballMatchEventService.recordPenalty(penaltyData);
      
      // Reset form
      setPenaltyForm({ teamId: '', playerId: '', penaltyType: '', minutes: 2, description: '' });
      setShowPenaltyForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording penalty:', error);
      setError(error instanceof Error ? error.message : 'Failed to record penalty');
    } finally {
      setLoading(false);
    }
  };

  const formatTime = (minutes: number, seconds: number) => {
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  const isTimeOverLimit = (minutes: number, seconds: number) => {
    const totalSeconds = minutes * 60 + seconds;
    return totalSeconds >= 1200; // 20 minutes = 1200 seconds
  };

  const formatEventTime = (timeInSeconds: number) => {
    const mins = Math.floor(timeInSeconds / 60);
    const secs = timeInSeconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const getPlayersForTeam = (teamId: string) => {
    return teamId === match.homeTeamId ? homePlayers : awayPlayers;
  };

  const handleCancelLive = () => {
    if (onCancelLive) {
      onCancelLive(match.id);
    }
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="live-match-modal-overlay">
      <div className="live-match-modal">
        {/* Header */}
        <div className="modal-header">
          <div className="match-info">
            <h2>{homeTeam?.name || 'Home'} vs {awayTeam?.name || 'Away'}</h2>
            <div className="status-controls">
              <span className="match-status">🔴 LIVE</span>
              <button onClick={handleCancelLive} className="cancel-live-button" title="Stop live tracking and return to scheduled state">
                ⏹️ Stop Live
              </button>
            </div>
          </div>
          <button onClick={onClose} className="close-button">×</button>
        </div>

        {/* Error Display */}
        {error && (
          <div className="error-alert">
            <span className="error-icon">⚠️</span>
            <span className="error-text">{error}</span>
            <button onClick={() => setError(null)} className="error-close">×</button>
          </div>
        )}

        <div className="modal-content">
          {/* Clock and Score Section */}
          <div className="clock-score-section">
            <div className="match-clock">
              <div className="period">Period {clock.period}</div>
              <div className={`time-display ${isTimeOverLimit(clock.minutes, clock.seconds) ? 'time-over-limit' : ''}`}>
                {formatTime(clock.minutes, clock.seconds)}
              </div>
              <div className="clock-controls">
                <button onClick={toggleClock} className={clock.isRunning ? "pause-btn" : "start-btn"}>
                  {clock.isRunning ? '⏸️ Pause' : '▶️ Start'}
                </button>
                <button onClick={resetClock} className="reset-btn">🔄 Reset</button>
                <button onClick={nextPeriod} className="next-period-btn">⏭️ Next Period</button>
              </div>
              <div className="time-controls">
                <button onClick={goBackTime} className="time-control-btn back-time-btn" title="Go back 5 seconds">
                  ⏪ 5s
                </button>
                <button onClick={goAheadTime} className="time-control-btn ahead-time-btn" title="Go ahead 30 seconds (Debug)">
                  ⏩ 30s
                </button>
              </div>
            </div>
            
            <div className="scoreboard">
              <div className="team-score">
                <div className="team-name">{homeTeam?.name || 'Home'}</div>
                <div className="score">{currentScore.home}</div>
              </div>
              <div className="score-separator">-</div>
              <div className="team-score">
                <div className="team-name">{awayTeam?.name || 'Away'}</div>
                <div className="score">{currentScore.away}</div>
              </div>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="quick-actions">
            <button 
              onClick={() => setShowGoalForm(true)} 
              className="action-btn goal-btn"
              disabled={loading}
            >
              ⚽ Record Goal
            </button>
            <button 
              onClick={() => setShowPenaltyForm(true)} 
              className="action-btn penalty-btn"
              disabled={loading}
            >
              🟨 Record Penalty
            </button>
          </div>

          {/* Goal Recording Form */}
          {showGoalForm && (
            <div className="event-form goal-form">
              <h3>Record Goal</h3>
              <div className="form-row">
                <select 
                  value={goalForm.teamId} 
                  onChange={(e) => setGoalForm(prev => ({ ...prev, teamId: e.target.value, playerId: '' }))}
                >
                  <option value="">Select Team</option>
                  <option value={match.homeTeamId}>{homeTeam?.name || 'Home'}</option>
                  <option value={match.awayTeamId}>{awayTeam?.name || 'Away'}</option>
                </select>
                
                {goalForm.teamId && (
                  <select 
                    value={goalForm.playerId} 
                    onChange={(e) => setGoalForm(prev => ({ ...prev, playerId: e.target.value }))}
                  >
                    <option value="">Select Player</option>
                    {getPlayersForTeam(goalForm.teamId).map(player => (
                      <option key={player.id} value={player.id}>
                        {player.person.firstName} {player.person.lastName}
                      </option>
                    ))}
                  </select>
                )}
                
                {goalForm.teamId && (
                  <select 
                    value={goalForm.assisterId} 
                    onChange={(e) => setGoalForm(prev => ({ ...prev, assisterId: e.target.value }))}
                  >
                    <option value="">Select Assist (Optional)</option>
                    {getPlayersForTeam(goalForm.teamId)
                      .filter(player => player.id !== goalForm.playerId)
                      .map(player => (
                        <option key={player.id} value={player.id}>
                          {player.person.firstName} {player.person.lastName}
                        </option>
                      ))}
                  </select>
                )}
              </div>
              
              <div className="form-actions">
                <button onClick={recordGoal} disabled={loading} className="submit-btn">
                  {loading ? 'Recording...' : 'Record Goal'}
                </button>
                <button onClick={() => setShowGoalForm(false)} className="cancel-btn">Cancel</button>
              </div>
            </div>
          )}

          {/* Penalty Recording Form */}
          {showPenaltyForm && (
            <div className="event-form penalty-form">
              <h3>Record Penalty</h3>
              <div className="form-row">
                <select 
                  value={penaltyForm.teamId} 
                  onChange={(e) => setPenaltyForm(prev => ({ ...prev, teamId: e.target.value, playerId: '' }))}
                >
                  <option value="">Select Team</option>
                  <option value={match.homeTeamId}>{homeTeam?.name || 'Home'}</option>
                  <option value={match.awayTeamId}>{awayTeam?.name || 'Away'}</option>
                </select>
                
                {penaltyForm.teamId && (
                  <select 
                    value={penaltyForm.playerId} 
                    onChange={(e) => setPenaltyForm(prev => ({ ...prev, playerId: e.target.value }))}
                  >
                    <option value="">Select Player (Optional)</option>
                    {getPlayersForTeam(penaltyForm.teamId).map(player => (
                      <option key={player.id} value={player.id}>
                        {player.person.firstName} {player.person.lastName}
                      </option>
                    ))}
                  </select>
                )}
                
                <select 
                  value={penaltyForm.penaltyType} 
                  onChange={(e) => setPenaltyForm(prev => ({ ...prev, penaltyType: e.target.value }))}
                >
                  <option value="">Select Penalty Type</option>
                  <option value="Tripping">Tripping</option>
                  <option value="Slashing">Slashing</option>
                  <option value="HighSticking">High Sticking</option>
                  <option value="Roughing">Roughing</option>
                  <option value="Boarding">Boarding</option>
                  <option value="Interference">Interference</option>
                  <option value="Unsportsmanlike">Unsportsmanlike Conduct</option>
                </select>
                
                <select 
                  value={penaltyForm.minutes} 
                  onChange={(e) => setPenaltyForm(prev => ({ ...prev, minutes: parseInt(e.target.value) }))}
                >
                  <option value={2}>2 minutes</option>
                  <option value={5}>5 minutes</option>
                  <option value={10}>10 minutes</option>
                  <option value={20}>20 minutes</option>
                </select>
              </div>
              
              <textarea 
                value={penaltyForm.description}
                onChange={(e) => setPenaltyForm(prev => ({ ...prev, description: e.target.value }))}
                placeholder="Description (optional)"
                className="description-input"
              />
              
              <div className="form-actions">
                <button onClick={recordPenalty} disabled={loading} className="submit-btn">
                  {loading ? 'Recording...' : 'Record Penalty'}
                </button>
                <button onClick={() => setShowPenaltyForm(false)} className="cancel-btn">Cancel</button>
              </div>
            </div>
          )}

          {/* Events History */}
          <div className="events-history">
            <h3>Match Events</h3>
            {events.length === 0 ? (
              <div className="no-events">No events recorded yet</div>
            ) : (
              <div className="events-list">
                {events.map(event => (
                  <div key={event.id} className={`event-item ${event.type}`}>
                    <div className="event-time">
                      P{event.periodNumber} - {formatEventTime(event.timeInSeconds)}
                    </div>
                    <div className="event-details">
                      {event.type === 'goal' ? (
                        <div className="goal-event">
                          <span className="event-icon">⚽</span>
                          <span className="event-text">
                            <strong>{event.teamName}</strong> - Goal by {event.playerName}
                            {event.assisterName && ` (Assist: ${event.assisterName})`}
                          </span>
                        </div>
                      ) : (
                        <div className="penalty-event">
                          <span className="event-icon">🟨</span>
                          <span className="event-text">
                            <strong>{event.teamName}</strong> - {event.penaltyType} ({event.penaltyMinutes}min)
                            {event.playerName && ` - ${event.playerName}`}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default LiveMatchModal; 