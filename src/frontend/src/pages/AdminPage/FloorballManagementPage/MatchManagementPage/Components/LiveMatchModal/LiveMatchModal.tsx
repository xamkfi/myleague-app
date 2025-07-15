import { useState, useEffect, useCallback, useMemo } from 'react';
import { signalRService, type MatchEvent } from '../../../../../../services/signalRService';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest,
  type FloorballGoalEventDto,
  type FloorballPenaltyEventDto
} from '../../../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../../../api/floorball/floorballMatchService';
import { floorballTeamService } from '../../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../../types/floorball/floorballTypes';
import type { LiveMatchState } from '../../hooks/useLiveMatchState';
import './LiveMatchModal.scss';

interface LiveMatchModalProps {
  match: FloorballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onGoLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
  isLive?: boolean;
  isFinished?: boolean;
}

interface GoalEventData {
  TeamId: string;
  PlayerId: string;
  AssisterId?: string;
  PeriodNumber: number;
  TimeInSeconds: number;
}

interface PenaltyEventData {
  TeamId: string;
  PlayerId: string;
  PenaltyType: string;
  Minutes: number;
  PeriodNumber: number;
  TimeInSeconds: number;
  Description: string;
}

const LiveMatchModal = ({ 
  match, 
  isOpen, 
  onClose, 
  onCompleteLive,
  onGoLive,
  liveState,
  onStateUpdate,
  isLive = false,
  isFinished = false
}: LiveMatchModalProps) => {
  // State management
  const [homeTeam, setHomeTeam] = useState<FloorballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FloorballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FloorballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [matchEvents, setMatchEvents] = useState<{
    goals: FloorballGoalEventDto[];
    penalties: FloorballPenaltyEventDto[];
  }>({ goals: [], penalties: [] });
  
  // Real match status from backend
  const [currentMatch, setCurrentMatch] = useState<FloorballMatchDto>(match);
  
  // Use state from parent or default values
  const currentScore = useMemo(() => 
    liveState?.currentScore || { home: currentMatch.homeScore, away: currentMatch.awayScore }, 
    [liveState?.currentScore, currentMatch.homeScore, currentMatch.awayScore]
  );
  const clock = liveState?.clock || {
    period: 1,
    minutes: 0,
    seconds: 0,
    isRunning: false
  };
  
  // Event recording state
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showPenaltyForm, setShowPenaltyForm] = useState(false);

  // Update penalty form with current time when form opens
  const openPenaltyForm = () => {
    setPenaltyForm(prev => ({
      ...prev,
      periodNumber: clock.period,
      timeMinutes: clock.minutes,
      timeSeconds: clock.seconds
    }));
    setShowPenaltyForm(true);
  };

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
    periodNumber: 1,
    timeMinutes: 0,
    timeSeconds: 0,
  });

  // Load team and player data
  useEffect(() => {
    if (isOpen) {
      loadTeamData();
      loadMatchEvents();
      loadCurrentMatchStatus();
      setupSignalR();
    }
    
    return () => {
      cleanupSignalR();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
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

  const loadCurrentMatchStatus = async () => {
    try {
      const response = await floorballMatchService.getById(match.id);
      
      if (response.success && response.data) {
        setCurrentMatch(response.data);
      }
    } catch (error) {
      console.error('Error loading current match status:', error);
      // Don't set error for status loading - it's not critical
    }
  };

  const loadMatchEvents = async () => {
    try {
      const response = await floorballMatchEventService.getMatchEvents(match.id);
      
      if (response.success && response.data) {
        setMatchEvents(response.data);
      }
    } catch (error) {
      console.error('Error loading match events:', error);
      // Don't set error for events loading - it's not critical
    }
  };

  const setupSignalR = async () => {
    try {
      // Connect to SignalR
      await signalRService.connect();
      
      // Wait a bit to ensure connection is stable
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Only subscribe if connection is established
      if (signalRService.isConnected) {
        await signalRService.subscribeToEventType('FloorballGoalScoredEvent');
        await signalRService.subscribeToEventType('FloorballPenaltyAssignedEvent');
        
        const unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        return unsubscribe;
      } else {
        console.warn('SignalR connection not established, skipping event subscriptions');
      }
    } catch (error) {
      console.error('Error setting up SignalR:', error);
      // Don't throw - SignalR is not critical for basic functionality
    }
  };

  const cleanupSignalR = async () => {
    try {
      if (signalRService.isConnected) {
        await signalRService.unsubscribeFromEventType('FloorballGoalScoredEvent');
        await signalRService.unsubscribeFromEventType('FloorballPenaltyAssignedEvent');
      }
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
      // Don't throw - cleanup errors are not critical
    }
  };

  const handleSignalREvent = useCallback((event: MatchEvent) => {
    console.log('Received match event:', event);
    
    const eventData = event.data as { MatchId?: string };
    if (eventData?.MatchId !== match.id) {
      return; // Event is not for this match
    }
    
    if (event.eventType === 'FloorballGoalScoredEvent') {
      handleGoalScored(event.data as GoalEventData);
    } else if (event.eventType === 'FloorballPenaltyAssignedEvent') {
      handlePenaltyAssigned(event.data as PenaltyEventData);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [match.id]);

  const handleGoalScored = useCallback((eventData: GoalEventData) => {
    if (!onStateUpdate) return;
    
    // Update score
    const newScore = {
      home: eventData.TeamId === match.homeTeamId ? currentScore.home + 1 : currentScore.home,
      away: eventData.TeamId === match.awayTeamId ? currentScore.away + 1 : currentScore.away
    };
    
    onStateUpdate({
      currentScore: newScore
    });
    
    // Refresh events from backend
    loadMatchEvents();
  }, [onStateUpdate, match.homeTeamId, match.awayTeamId, currentScore]);

  const handlePenaltyAssigned = useCallback((eventData: PenaltyEventData) => {
    if (!onStateUpdate) return;
    
    // Refresh events from backend
    loadMatchEvents();
  }, [onStateUpdate]);

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

  const goBackOneSecond = () => {
    if (!onStateUpdate) return;
    const totalSeconds = clock.minutes * 60 + clock.seconds;
    const newTotalSeconds = Math.max(0, totalSeconds - 1); // Don't go below 0
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

  const goAheadOneSecond = () => {
    if (!onStateUpdate) return;
    const totalSeconds = clock.minutes * 60 + clock.seconds;
    const newTotalSeconds = Math.min(1200, totalSeconds + 1); // Cap at 20 minutes (1200 seconds)
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
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: clock.period,
        timeInSeconds: clock.minutes * 60 + clock.seconds,
        wasInOvertime: clock.period > 3,
        wasInShootout: false, // TODO: Add shootout support
      };
      
      await floorballMatchEventService.recordGoal(goalData);
      
      // Refresh events from backend
      await loadMatchEvents();
      
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
        matchId: currentMatch.id,
        teamId: penaltyForm.teamId,
        playerId: penaltyForm.playerId || undefined,
        penaltyType: penaltyForm.penaltyType,
        durationMinutes: penaltyForm.minutes,
        periodNumber: penaltyForm.periodNumber,
        timeInSeconds: penaltyForm.timeMinutes * 60 + penaltyForm.timeSeconds,
        description: penaltyForm.description,
      };
      
      await floorballMatchEventService.recordPenalty(penaltyData);
      
      // Refresh events from backend
      await loadMatchEvents();
      
      // Reset form
      setPenaltyForm({ teamId: '', playerId: '', penaltyType: '', minutes: 2, description: '', periodNumber: 1, timeMinutes: 0, timeSeconds: 0 });
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
    return teamId === currentMatch.homeTeamId ? homePlayers : awayPlayers;
  };

  // Combine and sort all events by time
  const allEvents = useMemo(() => {
    if (!matchEvents) {
      return [];
    }
    
    const events = [
      ...(matchEvents.goals || []).map(goal => ({
        id: `goal-${goal.teamId}-${goal.playerId}-${goal.periodNumber}-${goal.timeInSeconds}`,
        type: 'goal' as const,
        teamId: goal.teamId,
        teamName: goal.teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
        playerId: goal.playerId,
        assisterId: goal.assisterId,
        periodNumber: goal.periodNumber,
        timeInSeconds: goal.timeInSeconds,
        timestamp: new Date(), // We don't have actual timestamp from backend
        wasInOvertime: goal.wasInOvertime,
        wasInShootout: goal.wasInShootout
      })),
      ...(matchEvents.penalties || []).map(penalty => ({
        id: `penalty-${penalty.teamId}-${penalty.playerId || 'team'}-${penalty.periodNumber}-${penalty.timeInSeconds}`,
        type: 'penalty' as const,
        teamId: penalty.teamId,
        teamName: penalty.teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
        playerId: penalty.playerId,
        periodNumber: penalty.periodNumber,
        timeInSeconds: penalty.timeInSeconds,
        timestamp: new Date(), // We don't have actual timestamp from backend
        penaltyType: penalty.penaltyType,
        penaltyMinutes: penalty.minutes,
        description: penalty.description
      }))
    ];

    // Sort by period number, then by time in seconds (most recent first)
    return events.sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return b.periodNumber - a.periodNumber; // Most recent period first
      }
      return b.timeInSeconds - a.timeInSeconds; // Most recent time first
    });
  }, [matchEvents, currentMatch.homeTeamId, homeTeam?.name, awayTeam?.name]);

  const handleGoLive = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Use the event sourced endpoint to start the match
      const response = await floorballMatchService.start(currentMatch.id);
      
      if (response.success && response.data) {
        // Update the current match with the response from the backend
        setCurrentMatch(response.data);
        
        // Update the match with the response from the backend
        // This will include the updated status from the event sourced system
        if (onGoLive) {
          onGoLive(currentMatch.id, response.data);
        }
      } else {
        setError('Failed to start match');
      }
    } catch (error) {
      console.error('Error starting match:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match');
    } finally {
      setLoading(false);
    }
  };

  const handleCompleteLive = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Use the event sourced endpoint to complete the match
      const response = await floorballMatchService.complete(currentMatch.id);
      
      if (response.success && response.data) {
        // Update the current match with the response from the backend
        setCurrentMatch(response.data);
        
        // Update the match with the response from the backend
        // This will include the updated status from the event sourced system
        if (onCompleteLive) {
          onCompleteLive(currentMatch.id, response.data);
        }
      } else {
        setError('Failed to complete match');
      }
    } catch (error) {
      console.error('Error completing match:', error);
      setError(error instanceof Error ? error.message : 'Failed to complete match');
    } finally {
      setLoading(false);
    }
    // Don't close the modal - let it stay open with "Match Finished" status
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
              {currentMatch.status === 'Completed' ? (
                <>
                  <span className="match-status">🏁 FINISHED</span>
                  <button onClick={onClose} className="close-modal-button" title="Close the match modal">
                    ✕ Close
                  </button>
                </>
              ) : currentMatch.status === 'InProgress' ? (
                <>
                  <span className="match-status">🔴 LIVE</span>
                  <button onClick={handleCompleteLive} className="cancel-live-button" title="Stop live tracking and mark match as finished">
                    ⏹️ Finish Match
                  </button>
                </>
              ) : (
                <>
                  <span className="match-status">⏸️ READY</span>
                  <button onClick={handleGoLive} className="go-live-button" title="Start live tracking for this match">
                    🟢 Go Live
                  </button>
                </>
              )}
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
          <div className="left-section">
          {/* Clock and Score Section */}
          <div className="clock-score-section">
            {currentMatch.status === 'Completed' && (
              <div className="match-finished-notice">
                <span className="notice-icon">🏁</span>
                <span className="notice-text">Match has been finished. Live tracking has been stopped.</span>
              </div>
            )}
            {currentMatch.status !== 'InProgress' && currentMatch.status !== 'Completed' && (
              <div className="not-live-notice">
                <span className="notice-icon">⏸️</span>
                <span className="notice-text">Match is not live yet. Click "Go Live" to start tracking.</span>
              </div>
            )}
            <div className="match-clock">
              <div className="period">Period {clock.period}</div>
              <div className={`time-display ${isTimeOverLimit(clock.minutes, clock.seconds) ? 'time-over-limit' : ''}`}>
                {formatTime(clock.minutes, clock.seconds)}
              </div>
              <div className="clock-controls">
                <button 
                  onClick={toggleClock} 
                  className={clock.isRunning ? "pause-btn" : "start-btn"}
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  {clock.isRunning ? '⏸️ Pause' : '▶️ Start'}
                </button>
                <button 
                  onClick={resetClock} 
                  className="reset-btn"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  🔄 Reset
                </button>
                <button 
                  onClick={nextPeriod} 
                  className="next-period-btn"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  ⏭️ Next Period
                </button>
              </div>
              <div className="time-controls">
                <button 
                  onClick={goBackOneSecond} 
                  className="time-control-btn back-time-btn" 
                  title="Go back 1 second"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  ⏪ 1s
                </button>
                <button 
                  onClick={goBackTime} 
                  className="time-control-btn back-time-btn" 
                  title="Go back 5 seconds"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  ⏪ 5s
                </button>
                <button 
                  onClick={goAheadOneSecond} 
                  className="time-control-btn ahead-time-btn" 
                  title="Go ahead 1 second"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  ⏩ 1s
                </button>
                <button 
                  onClick={goAheadTime} 
                  className="time-control-btn ahead-time-btn" 
                  title="Go ahead 30 seconds (Debug)"
                  disabled={currentMatch.status !== 'InProgress'}
                >
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
              disabled={loading || currentMatch.status !== 'InProgress'}
            >
              ⚽ Record Goal
            </button>
            <button 
              onClick={openPenaltyForm} 
              className="action-btn penalty-btn"
              disabled={loading || currentMatch.status !== 'InProgress'}
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
                  <option value={currentMatch.homeTeamId}>{homeTeam?.name || 'Home'}</option>
                  <option value={currentMatch.awayTeamId}>{awayTeam?.name || 'Away'}</option>
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
                  <option value={currentMatch.homeTeamId}>{homeTeam?.name || 'Home'}</option>
                  <option value={currentMatch.awayTeamId}>{awayTeam?.name || 'Away'}</option>
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
                  <option value="Minor">Minor</option>
                  <option value="Major">Major</option>
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
              
              <div className="form-row compact-time-row">
                <div className="compact-time-group">
                  <label>P:</label>
                  <input 
                    type="number" 
                    value={penaltyForm.periodNumber}
                    onChange={(e) => setPenaltyForm(prev => ({ ...prev, periodNumber: parseInt(e.target.value) || 1 }))}
                    min="1"
                    max="10"
                    className="compact-time-input"
                  />
                </div>
                
                <div className="compact-time-group">
                  <label>M:</label>
                  <input 
                    type="number" 
                    value={penaltyForm.timeMinutes}
                    onChange={(e) => setPenaltyForm(prev => ({ ...prev, timeMinutes: parseInt(e.target.value) || 0 }))}
                    min="0"
                    max="20"
                    placeholder={`${clock.minutes}`}
                    className="compact-time-input"
                  />
                </div>
                
                <div className="compact-time-group">
                  <label>S:</label>
                  <input 
                    type="number" 
                    value={penaltyForm.timeSeconds}
                    onChange={(e) => setPenaltyForm(prev => ({ ...prev, timeSeconds: parseInt(e.target.value) || 0 }))}
                    min="0"
                    max="59"
                    placeholder={`${clock.seconds}`}
                    className="compact-time-input"
                  />
                </div>
                
                <div className="time-hint-compact">
                  Current: {formatTime(clock.minutes, clock.seconds)}
                </div>
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
          </div>
          {/* Events History */}
          <div className="right-section">
            <div className="events-history">
              <h3>Match Events</h3>
              {allEvents.length === 0 ? (
                <div className="no-events">No events recorded yet</div>
              ) : (
                <div className="events-list">
                  {allEvents.map(event => (
                    <div key={event.id} className={`event-item ${event.type}`}>
                      <div className="event-time">
                       P{event.periodNumber} - {formatEventTime(event.timeInSeconds)}
                      </div>
                      <div className="event-details">
                        {event.type === 'goal' ? (
                          <div className="goal-event">
                            <span className="event-icon">⚽</span>
                            <span className="event-text">
                              <strong>{event.teamName}</strong> - Goal by {event.playerId}
                              {event.assisterId && ` (Assist: ${event.assisterId})`}
                              {event.wasInOvertime && ` (OT)`}
                              {event.wasInShootout && ` (SO)`}
                            </span>
                          </div>
                        ) : (
                          <div className="penalty-event">
                            <span className="event-icon">🟨</span>
                            <span className="event-text">
                              <strong>{event.teamName}</strong> - {event.penaltyType} ({event.penaltyMinutes}min)
                              {event.playerId && ` - ${event.playerId}`}
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
    </div>
  );
};

export default LiveMatchModal; 