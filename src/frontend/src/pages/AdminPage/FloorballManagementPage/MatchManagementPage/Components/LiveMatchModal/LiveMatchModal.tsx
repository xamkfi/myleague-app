import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { signalRService, type MatchEvent } from '../../../../../../services/signalRService';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest,
  type FloorballDomainEventDto
} from '../../../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../../../api/floorball/floorballMatchService';
import { floorballTeamService } from '../../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../../api/floorball/floorballPlayerService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../../types/floorball/floorballTypes';
import type { LiveMatchState } from '../../hooks/useLiveMatchState';

import { Timer } from '../../../../../../components/Timer/Timer';
import './LiveMatchModal.scss';

// Create a memoized Timer component to prevent unnecessary re-renders
const MemoizedTimer = React.memo(Timer);

interface LiveMatchModalProps {
  match: FloorballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onGoLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
  onMatchUpdated?: (updatedMatch: FloorballMatchDto) => void;
}

interface PeriodEventData {
  matchId: string;
  periodNumber: number;
  homeTeamScore: number;
  awayTeamScore: number;
  isLastRegularPeriod: boolean;
  occurredOn: string;
}



interface GoalEventData {
  MatchId: string;
  TeamId: string;
  PlayerId: string;
  PeriodNumber: number;
  EventTime: string;
  HomeTeam: { Id: string; Name: string };
  AwayTeam: { Id: string; Name: string };
}

interface PenaltyEventData {
  MatchId: string;
  EventTime: string;
  PenaltyType: string;
  TeamId: string;
  PlayerId: string;
  HomeTeam: { Id: string; Name: string };
  AwayTeam: { Id: string; Name: string };
}

const LiveMatchModal = ({ 
  match, 
  isOpen, 
  onClose, 
  onCompleteLive,
  onGoLive,
  liveState,
  onStateUpdate,
  onMatchUpdated
}: LiveMatchModalProps) => {
  // Debug logging for modal lifecycle
  useEffect(() => {
    console.log('🔄 LiveMatchModal RENDER:', { 
      matchId: match.id, 
      isOpen, 
      status: match.status,
      hasLiveState: !!liveState 
    });
  }, [match.id, isOpen, match.status, liveState]); // Only log when these specific values change
  // State management
  const [homeTeam, setHomeTeam] = useState<FloorballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FloorballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FloorballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [matchEvents, setMatchEvents] = useState<FloorballDomainEventDto[]>([]);
  
  // Period state management
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});
  
  // Real match status from backend
  const [currentMatch, setCurrentMatch] = useState<FloorballMatchDto>(match);
  
  // Timer state tracking for accurate time calculations
  const [currentTimerElapsedTime, setCurrentTimerElapsedTime] = useState<number>(0);
  const [getCurrentTimeFromTimer, setGetCurrentTimeFromTimer] = useState<(() => string) | null>(null);
  
  // LOCAL TIMER STATE - Only runs when modal is open
  const [localClock, setLocalClock] = useState({
    period: 1,
    minutes: 0,
    seconds: 0,
    isRunning: false
  });
  
  // Timer interval ref for cleanup
  const timerIntervalRef = useRef<number | null>(null);
  
  // Use state from parent or default values
  const currentScore = useMemo(() => {
    // Always use the current match data as the source of truth
    const baseScore = { home: currentMatch.homeScore, away: currentMatch.awayScore };
    
    // If we have liveState.currentScore, use it as an override for real-time updates
    if (liveState?.currentScore) {
      return liveState.currentScore;
    }
    
    return baseScore;
  }, [liveState?.currentScore, currentMatch.homeScore, currentMatch.awayScore]);
  
  // Use local clock state instead of liveState clock
  const clock = localClock;
  
  // Event recording state
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showPenaltyForm, setShowPenaltyForm] = useState(false);
  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState(false);
  const [pendingEndPeriodAction, setPendingEndPeriodAction] = useState<(() => void) | null>(null);
  

  const [showOvertimeConfirmation, setShowOvertimeConfirmation] = useState(false);
  const [showShootoutConfirmation, setShowShootoutConfirmation] = useState(false);

  // Open penalty form
  const openPenaltyForm = () => {
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

  // LOCAL TIMER MANAGEMENT - Only runs when modal is open
  useEffect(() => {
    if (!isOpen) {
      // Clean up timer when modal closes
      if (timerIntervalRef.current) {
        console.log('LiveMatchModal: Cleaning up timer interval');
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
      return;
    }

    // Only start timer if modal is open and clock is running
    if (isOpen && localClock.isRunning) {
      console.log('LiveMatchModal: Starting local timer interval');
      timerIntervalRef.current = setInterval(() => {
        setLocalClock(prev => {
          const newSeconds = prev.seconds + 1;
          if (newSeconds >= 60) {
            return {
              ...prev,
              minutes: prev.minutes + 1,
              seconds: 0
            };
          } else {
            return {
              ...prev,
              seconds: newSeconds
            };
          }
        });
      }, 1000);
    }

    return () => {
      if (timerIntervalRef.current) {
        console.log('LiveMatchModal: Cleaning up timer interval on unmount');
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
    };
  }, [isOpen, localClock.isRunning]);

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

  // Update currentMatch when match prop changes
  useEffect(() => {
    setCurrentMatch(match);
  }, [match]);

  // Initialize clock and period states
  useEffect(() => {
    if (isOpen && onStateUpdate && !liveState?.clock) {
      // Only initialize if we don't already have a clock state
      const initialClock = {
        period: 1,
        minutes: 0,
        seconds: 0,
        isRunning: false
      };
      setLocalClock(initialClock);
      onStateUpdate({
        clock: initialClock
      });
    }
  }, [isOpen, onStateUpdate, liveState?.clock]);



  // Initialize started periods when component loads
  useEffect(() => {
    if (isOpen && currentMatch.status === 'InProgress') {
      // If match is in progress, assume period 1 has been started
      setStartedPeriods(new Set([1]));
      // Set next period to start as period 2
      setNextPeriodToStart(2);
    } else if (isOpen) {
      // Reset started periods for new matches
      setStartedPeriods(new Set());
      setEndedPeriods(new Set());
      setNextPeriodToStart(1);
    }
  }, [isOpen, currentMatch.status]);



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



  /**
   * Loads the current match status from the backend
   * This ensures we have the most up-to-date match information
   */
  const loadCurrentMatchStatus = useCallback(async () => {
    try {
      console.log('Loading current match status for match:', match.id);
      const response = await floorballMatchService.getById(match.id);
      
      console.log('Match status response:', response);
      
      if (response.success && response.data) {
        const updatedMatch = response.data;
        console.log('Updated match data:', updatedMatch);
        console.log('Current scores - Home:', updatedMatch.homeScore, 'Away:', updatedMatch.awayScore);
        
        setCurrentMatch(updatedMatch);
        
        // Update the liveState with the new score from backend
        if (onStateUpdate) {
          const newScore = {
            home: updatedMatch.homeScore,
            away: updatedMatch.awayScore
          };
          console.log('Updating liveState with new score:', newScore);
          onStateUpdate({
            currentScore: newScore
          });
        }
        
        // Notify parent component about the updated match data
        if (onMatchUpdated) {
          console.log('Notifying parent component about updated match data');
          onMatchUpdated(updatedMatch);
        }
      } else {
        console.warn('Failed to load match status:', response.message || 'Unknown error');
      }
    } catch (error) {
      console.error('Error loading current match status:', error);
      // Don't set error for status loading - it's not critical
    }
  }, [match.id, onStateUpdate, onMatchUpdated]);

  /**
   * Loads all match events (goals and penalties) from the backend
   * The backend returns domain events in a flat array structure
   * This function fetches the events and stores them for processing
   */
  const loadMatchEvents = useCallback(async () => {
    try {
      console.log('loadMatchEvents called for match:', match.id);
      const response = await floorballMatchEventService.getMatchEvents(match.id);
      
      console.log('loadMatchEvents response:', response);
      
      if (response.success && response.data) {
        console.log('Setting match events:', response.data);
        setMatchEvents(response.data as FloorballDomainEventDto[]);
      }
    } catch (error) {
      console.error('Error loading match events:', error);
      // Don't set error for events loading - it's not critical
    }
  }, [match.id]);

  /**
   * Sets up SignalR connection for real-time updates
   * Subscribes to goal and penalty events for this specific match
   * This enables live updates when events are recorded
   */
  const setupSignalR = async () => {
    try {
      console.log('Setting up SignalR connection...');
      
      // Test backend accessibility first
      const isBackendAccessible = await signalRService.testBackendAccessibility();
      if (!isBackendAccessible) {
        console.warn('Backend is not accessible, skipping SignalR setup');
        return;
      }
      
      // Connect to SignalR
      await signalRService.connect();
      
      // Wait a bit to ensure connection is stable
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Only subscribe if connection is established
      if (signalRService.isConnected) {
        console.log('SignalR connected, subscribing to match events...');
        
        // Subscribe to this specific match for all match-related events
        await signalRService.subscribeToMatch(match.id);
        
        // Also subscribe to specific event types for broader coverage
        await signalRService.subscribeToEventType('FloorballGoalScored');
        await signalRService.subscribeToEventType('FloorballPenaltyAssigned');
        await signalRService.subscribeToEventType('FloorballPeriodStartedEvent');
        
        const unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        console.log('SignalR setup completed successfully');
        return unsubscribe;
      } else {
        console.warn('SignalR connection not established, skipping event subscriptions');
      }
    } catch (error) {
      console.error('Error setting up SignalR:', error);
      // Don't throw - SignalR is not critical for basic functionality
    }
  };

  /**
   * Cleans up SignalR subscriptions when the modal is closed
   * This prevents memory leaks and unnecessary network traffic
   */
  const cleanupSignalR = async () => {
    try {
      if (signalRService.isConnected) {
        // Unsubscribe from match-specific events
        await signalRService.unsubscribeFromMatch(match.id);
        
        // Unsubscribe from event types
        await signalRService.unsubscribeFromEventType('FloorballGoalScored');
        await signalRService.unsubscribeFromEventType('FloorballPenaltyAssigned');
        await signalRService.unsubscribeFromEventType('FloorballPeriodStartedEvent');
      }
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
      // Don't throw - cleanup errors are not critical
    }
  };



  /**
   * Handles real-time period started events from SignalR
   * Updates the reconstructed match state based on the new event
   */
  const handlePeriodStarted = useCallback((eventData: PeriodEventData) => {
    console.log('handlePeriodStarted called with eventData:', eventData);
    
    if (eventData.matchId !== match.id) {
      console.log('Period started event is for different match, ignoring');
      return;
    }
    
    // Mark this period as started in our local state
    setStartedPeriods(prev => new Set([...prev, eventData.periodNumber]));
    
    // Event handled - no need to reconstruct state since we're using backend timer
    console.log('Period started event handled');
  }, [match.id]);

  /**
   * Handles real-time goal events from SignalR
   * Refreshes the match data from backend to get accurate score
   */
  const handleGoalScored = useCallback((eventData: GoalEventData) => {
    console.log('handleGoalScored called with eventData:', eventData);
    
    if (!onStateUpdate) return;
    
    // Verify this event is for our match
    if (eventData.MatchId !== match.id) {
      console.log('Goal event is for different match, ignoring');
      return;
    }
    
    console.log('Goal scored for team:', eventData.TeamId);
    console.log('Player ID:', eventData.PlayerId);
    console.log('Period number:', eventData.PeriodNumber);
    console.log('Event time:', eventData.EventTime);
    console.log('Home team:', eventData.HomeTeam);
    console.log('Away team:', eventData.AwayTeam);
    
    // Refresh match data from backend to get accurate score
    loadCurrentMatchStatus();
    
    // Refresh events list
    loadMatchEvents();
  }, [onStateUpdate, match.id, loadCurrentMatchStatus, loadMatchEvents]);

  /**
   * Handles real-time penalty events from SignalR
   * Refreshes the events list to show the new penalty
   */
  const handlePenaltyAssigned = useCallback((eventData: PenaltyEventData) => {
    console.log('handlePenaltyAssigned called with eventData:', eventData);
    
    if (!onStateUpdate) return;
    
    // Verify this event is for our match
    if (eventData.MatchId !== match.id) {
      console.log('Penalty event is for different match, ignoring');
      return;
    }
    
    console.log('Penalty assigned for team:', eventData.TeamId);
    console.log('Penalty type:', eventData.PenaltyType);
    console.log('Player ID:', eventData.PlayerId);
    
    // Refresh events list
    loadMatchEvents();
  }, [onStateUpdate, match.id, loadMatchEvents]);

  /**
   * Handles real-time SignalR events for this match
   * Filters events to only process those relevant to this match
   * Updates the UI immediately when events are received
   */
  const handleSignalREvent = useCallback((event: MatchEvent) => {
    console.log('Received match event:', event);
    console.log('Event type:', event.eventType);
    console.log('Event data:', event.data);
    
    const eventData = event.data as { MatchId?: string };
    console.log('Extracted MatchId from event data:', eventData?.MatchId);
    console.log('Current match ID:', match.id);
    
    if (eventData?.MatchId !== match.id) {
      console.log('Event is not for this match, ignoring');
      return; // Event is not for this match
    }
    
    console.log('Processing event for this match');
    
    // IGNORE timer events - let the Timer component handle them
    if (event.eventType === 'TimerUpdateEvent') {
      console.log('Ignoring timer event - Timer component will handle it');
      return;
    }
    
    if (event.eventType === 'FloorballGoalScored') {
      console.log('Handling goal scored event');
      handleGoalScored(event.data as GoalEventData);
    } else if (event.eventType === 'FloorballPenaltyAssigned') {
      console.log('Handling penalty assigned event');
      handlePenaltyAssigned(event.data as PenaltyEventData);
    } else if (event.eventType === 'FloorballPeriodStartedEvent') {
      console.log('Handling period started event');
      handlePeriodStarted(event.data as PeriodEventData);
    } else {
      console.log('Unknown event type:', event.eventType);
    }
  }, [match.id, handleGoalScored, handlePenaltyAssigned, handlePeriodStarted]);

  // State for tracking which periods have been started and ended
  const [startedPeriods, setStartedPeriods] = useState<Set<number>>(new Set());
  const [endedPeriods, setEndedPeriods] = useState<Set<number>>(new Set());
  
  // State for tracking the next period to start
  const [nextPeriodToStart, setNextPeriodToStart] = useState<number>(1);
 
  /**
   * Simple function that only starts the match
   */
  const handleStartMatch = async () => {
    try {
      setLoading(true);
      setError(null);
      
        console.log('Starting match...');
        const response = await floorballMatchService.start(currentMatch.id);
        
        if (response.success && response.data) {
        console.log('Match started successfully, updating state...');
          setCurrentMatch(response.data);
          if (onGoLive) {
            onGoLive(currentMatch.id, response.data);
          }
        } else {
          throw new Error('Failed to start match');
        }
      
      setError(null);
    } catch (error) {
      console.error('Error starting match:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Ends the current period by sending API call
   * Now passes the actual elapsed time to the backend
   */
  const endPeriod = async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [clock.period]: true }));
      
      // Pass the current elapsed time to the backend for accurate period ending
      const elapsedTimeInSeconds = currentTimerElapsedTime;
      console.log(`Ending period ${clock.period} at ${elapsedTimeInSeconds} seconds for match ${currentMatch.id}`);
      
      await floorballMatchEventService.endPeriod(currentMatch.id, clock.period);
      console.log(`Ended period ${clock.period} for match ${currentMatch.id}`);
      
      // Mark this period as ended
      setEndedPeriods(prev => new Set([...prev, clock.period]));
      
      // Calculate the next period to start
      let nextPeriod = clock.period + 1;
      
      // Allow progression to overtime and shootout regardless of score
      if (clock.period === 3) {
        nextPeriod = 4; // Overtime
        console.log('Regular periods ended, transitioning to overtime');
      } else if (clock.period === 4) {
        nextPeriod = 5; // Shootout
        console.log('Overtime ended, transitioning to shootout');
      } else if (clock.period === 5) {
        // After shootout, no more periods
        nextPeriod = 0;
        console.log('Shootout ended, match is complete');
      }
      
      // Set the next period to start
      setNextPeriodToStart(nextPeriod);
      
      // Update the clock to the next period
      if (nextPeriod > 0) {
        const newClock = { 
          period: nextPeriod, 
          minutes: 0, 
          seconds: 0, 
          isRunning: false 
        };
        setLocalClock(newClock);
        if (onStateUpdate) {
          onStateUpdate({
            clock: newClock
          });
        }
      }
      
      setError(null);
    } catch (error) {
      console.error('Error ending period:', error);
      setError(error instanceof Error ? error.message : 'Failed to end period');
    } finally {
      setPeriodLoading(prev => ({ ...prev, [clock.period]: false }));
    }
  };

  /**
   * Starts a new period
   */
  const startPeriod = async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: true }));
      
      console.log(`Starting period ${nextPeriodToStart} for match ${currentMatch.id}`);
      
      // Start the period via API
      await floorballMatchEventService.startPeriod(currentMatch.id, nextPeriodToStart);
      console.log(`Started period ${nextPeriodToStart} for match ${currentMatch.id}`);
      
      // Mark this period as started
      setStartedPeriods(prev => new Set([...prev, nextPeriodToStart]));
      
      // Update the clock to the new period
      const newClock = { 
        period: nextPeriodToStart, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      // Don't increment nextPeriodToStart yet - wait until this period ends
      // The nextPeriodToStart will be calculated when endPeriod() is called
      
      setError(null);
    } catch (error) {
      console.error('Error starting period:', error);
      setError(error instanceof Error ? error.message : 'Failed to start period');
    } finally {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: false }));
    }
  };

  /**
   * Handles the period control button click (End Period or Start Period)
   * Gets current time directly from the timer component
   */
  const handlePeriodControlClick = () => {
    // If we have a period that can be ended, handle end period logic
    if (canEndPeriod()) {
      // Get current time directly from the timer component
      let currentTime = '00:00';
      let totalSeconds = 0;
      
      if (getCurrentTimeFromTimer) {
        currentTime = getCurrentTimeFromTimer();
        console.log(`Got current time from timer: ${currentTime}`);
        
        // Parse the time string (format: "MM:SS" or "HH:MM:SS")
        const timeParts = currentTime.split(':');
        if (timeParts.length === 2) {
          // Format: "MM:SS"
          const minutes = parseInt(timeParts[0]) || 0;
          const seconds = parseInt(timeParts[1]) || 0;
          totalSeconds = minutes * 60 + seconds;
        } else if (timeParts.length === 3) {
          // Format: "HH:MM:SS"
          const hours = parseInt(timeParts[0]) || 0;
          const minutes = parseInt(timeParts[1]) || 0;
          const seconds = parseInt(timeParts[2]) || 0;
          totalSeconds = hours * 3600 + minutes * 60 + seconds;
        }
      } else {
        // Fallback to stored elapsed time if timer function not available
        totalSeconds = currentTimerElapsedTime;
        console.log(`Using fallback elapsed time: ${totalSeconds}s`);
      }
      
      const isUnder20Minutes = totalSeconds < 1200; // 20 minutes = 1200 seconds
      
      console.log(`End period validation - Current time: ${currentTime}, Total seconds: ${totalSeconds}s, Under 20min: ${isUnder20Minutes}`);
      
      // Store the current time for use in confirmation dialog
      setCurrentTimerElapsedTime(totalSeconds);
      
      if (isUnder20Minutes) {
        // Show confirmation dialog for periods under 20 minutes
        setShowEndPeriodConfirmation(true);
        setPendingEndPeriodAction(() => endPeriod);
      } else {
        // End period immediately if 20 minutes or more
        endPeriod();
      }
    } else {
      // Start a new period
      startPeriod();
    }
  };

  /**
   * Confirms the end period action
   */
  const confirmEndPeriod = () => {
    if (pendingEndPeriodAction) {
      pendingEndPeriodAction();
      setPendingEndPeriodAction(null);
    }
    setShowEndPeriodConfirmation(false);
  };

  /**
   * Cancels the end period action
   */
  const cancelEndPeriod = () => {
    setPendingEndPeriodAction(null);
    setShowEndPeriodConfirmation(false);
  };

  /**
   * Records overtime for the current match
   */
  const recordOvertime = async () => {
    try {
      setLoading(true);
      setError(null);
      
      await floorballMatchEventService.recordOvertime(currentMatch.id);
      
      // Start the overtime period (period 4)
      await floorballMatchEventService.startPeriod(currentMatch.id, 4);
      
      // Update the clock to period 4
      const newClock = { 
        period: 4, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      // Refresh match status from backend
      await loadCurrentMatchStatus();
      setError(null);
      setShowOvertimeConfirmation(false);
      
    } catch (error) {
      console.error('Error recording overtime:', error);
      setError(error instanceof Error ? error.message : 'Failed to record overtime');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Records shootout for the current match
   */
  const recordShootout = async () => {
    try {
      setLoading(true);
      setError(null);
      
      await floorballMatchEventService.recordShootout(currentMatch.id);
      
      // Start the shootout period (period 5)
      await floorballMatchEventService.startPeriod(currentMatch.id, 5);
      
      // Update the clock to period 5
      const newClock = { 
        period: 5, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      // Refresh match status from backend
      await loadCurrentMatchStatus();
      setError(null);
      setShowShootoutConfirmation(false);
      
    } catch (error) {
      console.error('Error recording shootout:', error);
      setError(error instanceof Error ? error.message : 'Failed to record shootout');
    } finally {
      setLoading(false);
    }
  };



  /**
   * Determines if we're currently in overtime
   */
  const isInOvertime = () => {
    return clock.period === 4;
  };

  /**
   * Determines if we're currently in shootout
   */
  const isInShootout = () => {
    return clock.period === 5;
  };

  /**
   * Gets the text for the period control button
   */
  const getPeriodControlButtonText = () => {
    // If we can end a period, show end period text
    if (canEndPeriod()) {
      if (periodLoading[clock.period]) {
        return 'Ending...';
      }
      
      if (isInOvertime()) {
        return '🔴 End Overtime';
      }
      
      if (isInShootout()) {
        return '🔴 End Shootout';
      }
      
      return '🔴 End Period';
    } else {
      // Show start period text
      if (periodLoading[nextPeriodToStart]) {
        return 'Starting...';
      }
      
      if (nextPeriodToStart === 4) {
        return '⏰ Start Overtime';
      }
      
      if (nextPeriodToStart === 5) {
        return '🎯 Start Shootout';
      }
      
      return `🟢 Start Period ${nextPeriodToStart}`;
    }
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
        timeInSeconds: currentTimerElapsedTime,
        wasInOvertime: currentMatch.wentToOvertime || clock.period > 3,
        wasInShootout: currentMatch.wentToShootout || clock.period > 4,
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
        periodNumber: clock.period,
        timeInSeconds: currentTimerElapsedTime,
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



  /**
   * Determines if the End Period button should be enabled
   * @returns true if the period can be ended
   */
  const canEndPeriod = () => {
    return currentMatch.status === 'InProgress' && 
           !periodLoading[clock.period] &&
           startedPeriods.has(clock.period) &&
           !endedPeriods.has(clock.period) &&
           nextPeriodToStart > 0; // Only allow ending if there's a next period to start
  };

  /**
   * Gets the current period status for display
   * @returns A string describing the current period status
   */
  const getPeriodStatus = () => {
    if (periodLoading[clock.period]) {
      return 'Processing...';
    }
    
    if (currentMatch.status === 'Completed') {
      return '🔴 Completed';
    }
    
    if (currentMatch.status === 'InProgress') {
      if (endedPeriods.has(clock.period)) {
        if (clock.period === 4) {
          return '🔴 Overtime Ended';
        } else if (clock.period === 5) {
          return '🔴 Shootout Ended';
        } else {
          return '🔴 Ended';
        }
      } else if (startedPeriods.has(clock.period)) {
        if (clock.period === 4) {
          return '🟢 Overtime Started';
        } else if (clock.period === 5) {
          return '🟢 Shootout Started';
        } else {
          return '🟢 Started';
        }
      } else {
        if (clock.period === 4) {
          return '⏸️ Overtime Not Started';
        } else if (clock.period === 5) {
          return '⏸️ Shootout Not Started';
        } else {
          return '⏸️ Not Started';
        }
      }
    }
    
    return '⏸️ Not Started';
  };

  const formatEventTime = (timeInSeconds: number) => {
    // Handle invalid inputs
    if (timeInSeconds === undefined || timeInSeconds === null || isNaN(timeInSeconds)) {
      console.warn('formatEventTime received invalid timeInSeconds:', timeInSeconds);
      return '00:00';
    }
    
    const mins = Math.floor(timeInSeconds / 60);
    const secs = timeInSeconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  /**
   * Gets all players for a specific team (home or away)
   * @param teamId - The team ID to get players for
   * @returns Array of players for the specified team
   */
  const getPlayersForTeam = (teamId: string) => {
    return teamId === currentMatch.homeTeamId ? homePlayers : awayPlayers;
  };

  /**
   * Looks up a player's full name by their ID using the loaded player data
   * This avoids making additional API calls since we already have the player data
   * @param playerId - The player's unique identifier
   * @returns The player's full name (firstName + lastName) or a fallback if not found
   */
  const getPlayerNameById = (playerId: string | undefined | null): string => {
    if (!playerId) {
      return 'Unknown Player';
    }
    
    const allPlayers = [...homePlayers, ...awayPlayers];
    const player = allPlayers.find(p => p.id === playerId);
    return player ? `${player.person.firstName} ${player.person.lastName}` : `Player ${playerId.slice(0, 8)}...`;
  };

  /**
   * Processes and combines all match events (goals and penalties) from the backend
   * The backend returns domain events, so we need to extract and format the relevant data
   * This includes:
   * - Converting domain events to displayable events
   * - Adding player names using the loaded player data
   * - Sorting events by period and time (most recent first)
   * - Handling missing or malformed data gracefully
   */
  const allEvents = useMemo(() => {
    console.log('allEvents useMemo called with matchEvents:', matchEvents);
    
    if (!matchEvents) {
      console.log('No matchEvents, returning empty array');
      return [];
    }
    
    // Process domain events from backend into displayable events
    const events = matchEvents
      .filter((event): event is FloorballDomainEventDto => event !== null && event !== undefined)
      .map((event: FloorballDomainEventDto) => {
        console.log('Processing event:', event);
        console.log('Event type:', event.eventType);
        console.log('Event data:', event.data);
        
        // Handle goal events
        if (event.eventType === 'FloorballGoalScoredEvent') {
          console.log('Processing FloorballGoalScoredEvent');
          console.log('Raw event data:', event.data);
          console.log('Event data type:', typeof event.data);
          console.log('Event data keys:', Object.keys(event.data || {}));
          
          const goalData = event.data as {
            MatchId?: string;
            TeamId?: string;
            PlayerId?: string;
            PeriodNumber?: number;
            TimeInSeconds?: number;
            EventTime?: string;
            IsOvertime?: boolean;
            IsPenaltyShot?: boolean;
            IsShootout?: boolean;
            AssisterId?: string;
            SecondaryAssisterId?: string;
            // Handle camelCase field names from JSON serialization
            matchId?: string;
            teamId?: string;
            playerId?: string;
            periodNumber?: number;
            timeInSeconds?: number;
            eventTime?: string;
            isOvertime?: boolean;
            isPenaltyShot?: boolean;
            isShootout?: boolean;
            assisterId?: string;
            secondaryAssisterId?: string;
          };
          console.log('Goal data structure:', goalData);
          console.log('Goal data keys:', Object.keys(goalData));
          console.log('TimeInSeconds value:', goalData.TimeInSeconds);
          console.log('TimeInSeconds type:', typeof goalData.TimeInSeconds);
          console.log('EventTime value:', goalData.EventTime);
          console.log('EventTime type:', typeof goalData.EventTime);
          
          // Handle both PascalCase and camelCase field names
          // Try TimeInSeconds first, then parse EventTime if available, fallback to 0
          let timeInSeconds = goalData.TimeInSeconds ?? goalData.timeInSeconds ?? 0;
          
          // If TimeInSeconds is not available, try to parse EventTime
          if (timeInSeconds === 0 && (goalData.EventTime || goalData.eventTime)) {
            const eventTime = goalData.EventTime ?? goalData.eventTime;
            console.log('Parsing EventTime:', eventTime);
            // EventTime might be in format "00:45" (mm:ss) or similar
            if (typeof eventTime === 'string') {
              const timeParts = eventTime.split(':');
              if (timeParts.length === 2) {
                const minutes = parseInt(timeParts[0]) || 0;
                const seconds = parseInt(timeParts[1]) || 0;
                timeInSeconds = minutes * 60 + seconds;
                console.log('Parsed time from EventTime:', timeInSeconds, 'seconds');
              } else if (timeParts.length === 3) {
                // Handle hh:mm:ss format
                const hours = parseInt(timeParts[0]) || 0;
                const minutes = parseInt(timeParts[1]) || 0;
                const seconds = parseInt(timeParts[2]) || 0;
                timeInSeconds = hours * 3600 + minutes * 60 + seconds;
                console.log('Parsed time from EventTime (hh:mm:ss):', timeInSeconds, 'seconds');
              }
            }
          }
          
          // If still no time, try to extract from the event data structure
          if (timeInSeconds === 0 && event.data) {
            const eventData = event.data as any;
            // Look for time-related fields in the nested structure
            if (eventData.TimeInSeconds !== undefined) {
              timeInSeconds = eventData.TimeInSeconds;
              console.log('Found TimeInSeconds in nested structure:', timeInSeconds);
            } else if (eventData.timeInSeconds !== undefined) {
              timeInSeconds = eventData.timeInSeconds;
              console.log('Found timeInSeconds in nested structure:', timeInSeconds);
            } else if (eventData.EventTime) {
              const eventTime = eventData.EventTime;
              console.log('Found EventTime in nested structure:', eventTime);
              if (typeof eventTime === 'string') {
                const timeParts = eventTime.split(':');
                if (timeParts.length === 2) {
                  const minutes = parseInt(timeParts[0]) || 0;
                  const seconds = parseInt(timeParts[1]) || 0;
                  timeInSeconds = minutes * 60 + seconds;
                  console.log('Parsed nested EventTime:', timeInSeconds, 'seconds');
                }
              }
            }
          }
            const periodNumber = goalData.PeriodNumber ?? goalData.periodNumber ?? 1;
            const teamId = goalData.TeamId ?? goalData.teamId ?? '';
            const playerId = goalData.PlayerId ?? goalData.playerId ?? '';
            const isOvertime = goalData.IsOvertime ?? goalData.isOvertime ?? false;
            const isShootout = goalData.IsShootout ?? goalData.isShootout ?? false;
            const assisterId = goalData.AssisterId ?? goalData.assisterId;
            
            return {
              id: `goal-${teamId}-${playerId}-${periodNumber}-${timeInSeconds}`,
              type: 'goal' as const,
              teamId: teamId,
              teamName: teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
              playerId: playerId,
              playerName: getPlayerNameById(playerId), // Look up player name from loaded data
              assisterId: assisterId,
              assisterName: assisterId ? getPlayerNameById(assisterId) : undefined, // Look up assister name
              periodNumber: periodNumber,
              timeInSeconds: timeInSeconds,
              timestamp: new Date(event.occurredOn),
              wasInOvertime: isOvertime,
              wasInShootout: isShootout
            };
        } 
        // Handle penalty events
        else if (event.eventType === 'FloorballPenaltyAssignedEvent') {
          console.log('Processing FloorballPenaltyAssignedEvent');
          console.log('Raw penalty event data:', event.data);
          console.log('Penalty event data type:', typeof event.data);
          console.log('Penalty event data keys:', Object.keys(event.data || {}));
          
          const penaltyData = event.data as {
            MatchId?: string;
            TeamId?: string;
            PlayerId?: string;
            PeriodNumber?: number;
            TimeInSeconds?: number;
            EventTime?: string;
            PenaltyType?: string;
            Minutes?: number;
            Description?: string;
            // Handle camelCase field names from JSON serialization
            matchId?: string;
            teamId?: string;
            playerId?: string;
            periodNumber?: number;
            timeInSeconds?: number;
            eventTime?: string;
            penaltyType?: string;
            minutes?: number;
            description?: string;
          };
          console.log('Penalty data structure:', penaltyData);
          console.log('Penalty data keys:', Object.keys(penaltyData));
          console.log('TimeInSeconds value:', penaltyData.TimeInSeconds);
          console.log('TimeInSeconds type:', typeof penaltyData.TimeInSeconds);
          console.log('EventTime value:', penaltyData.EventTime);
          console.log('EventTime type:', typeof penaltyData.EventTime);
          
          // Handle both PascalCase and camelCase field names for penalty
          // Try TimeInSeconds first, then parse EventTime if available, fallback to 0
          let penaltyTimeInSeconds = penaltyData.TimeInSeconds ?? penaltyData.timeInSeconds ?? 0;
          
          // If TimeInSeconds is not available, try to parse EventTime
          if (penaltyTimeInSeconds === 0 && (penaltyData.EventTime || penaltyData.eventTime)) {
            const eventTime = penaltyData.EventTime ?? penaltyData.eventTime;
            console.log('Parsing penalty EventTime:', eventTime);
            // EventTime might be in format "00:45" (mm:ss) or similar
            if (typeof eventTime === 'string') {
              const timeParts = eventTime.split(':');
              if (timeParts.length === 2) {
                const minutes = parseInt(timeParts[0]) || 0;
                const seconds = parseInt(timeParts[1]) || 0;
                penaltyTimeInSeconds = minutes * 60 + seconds;
                console.log('Parsed penalty time from EventTime:', penaltyTimeInSeconds, 'seconds');
              } else if (timeParts.length === 3) {
                // Handle hh:mm:ss format
                const hours = parseInt(timeParts[0]) || 0;
                const minutes = parseInt(timeParts[1]) || 0;
                const seconds = parseInt(timeParts[2]) || 0;
                penaltyTimeInSeconds = hours * 3600 + minutes * 60 + seconds;
                console.log('Parsed penalty time from EventTime (hh:mm:ss):', penaltyTimeInSeconds, 'seconds');
              }
            }
          }
          
          // If still no time, try to extract from the event data structure
          if (penaltyTimeInSeconds === 0 && event.data) {
            const eventData = event.data as any;
            // Look for time-related fields in the nested structure
            if (eventData.TimeInSeconds !== undefined) {
              penaltyTimeInSeconds = eventData.TimeInSeconds;
              console.log('Found penalty TimeInSeconds in nested structure:', penaltyTimeInSeconds);
            } else if (eventData.timeInSeconds !== undefined) {
              penaltyTimeInSeconds = eventData.timeInSeconds;
              console.log('Found penalty timeInSeconds in nested structure:', penaltyTimeInSeconds);
            } else if (eventData.EventTime) {
              const eventTime = eventData.EventTime;
              console.log('Found penalty EventTime in nested structure:', eventTime);
              if (typeof eventTime === 'string') {
                const timeParts = eventTime.split(':');
                if (timeParts.length === 2) {
                  const minutes = parseInt(timeParts[0]) || 0;
                  const seconds = parseInt(timeParts[1]) || 0;
                  penaltyTimeInSeconds = minutes * 60 + seconds;
                  console.log('Parsed nested penalty EventTime:', penaltyTimeInSeconds, 'seconds');
                }
              }
            }
          }
            const penaltyPeriodNumber = penaltyData.PeriodNumber ?? penaltyData.periodNumber ?? 1;
            const penaltyTeamId = penaltyData.TeamId ?? penaltyData.teamId ?? '';
            const penaltyPlayerId = penaltyData.PlayerId ?? penaltyData.playerId;
            const penaltyType = penaltyData.PenaltyType ?? penaltyData.penaltyType ?? '';
            const penaltyMinutes = penaltyData.Minutes ?? penaltyData.minutes ?? 0;
            const penaltyDescription = penaltyData.Description ?? penaltyData.description ?? '';
            
            return {
              id: `penalty-${penaltyTeamId}-${penaltyPlayerId || 'team'}-${penaltyPeriodNumber}-${penaltyTimeInSeconds}`,
              type: 'penalty' as const,
              teamId: penaltyTeamId,
              teamName: penaltyTeamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
              playerId: penaltyPlayerId,
              playerName: penaltyPlayerId ? getPlayerNameById(penaltyPlayerId) : 'Team Penalty', // Handle team penalties
              periodNumber: penaltyPeriodNumber,
              timeInSeconds: penaltyTimeInSeconds,
              timestamp: new Date(event.occurredOn),
              penaltyType: penaltyType,
              penaltyMinutes: penaltyMinutes,
              description: penaltyDescription
            };
        }
        return null;
      })
      .filter((event): event is NonNullable<typeof event> => event !== null);

    console.log('Processed events array:', events);

    // Sort events by period number (descending), then by time in seconds (descending)
    // This shows the most recent events first
    const sortedEvents = events.sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return b.periodNumber - a.periodNumber; // Most recent period first
      }
      return b.timeInSeconds - a.timeInSeconds; // Most recent time first
    });

    console.log('Final sorted events:', sortedEvents);
    return sortedEvents;
  }, [matchEvents, currentMatch.homeTeamId, homeTeam?.name, awayTeam?.name, homePlayers, awayPlayers]);

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

        {/* End Period Confirmation Dialog */}
        {showEndPeriodConfirmation && (
          <div className="confirmation-dialog-overlay">
            <div className="confirmation-dialog">
              <div className="confirmation-header">
                <span className="confirmation-icon">⚠️</span>
                <h3>Confirm End Period</h3>
              </div>
              <div className="confirmation-content">
                <p>
                  Are you sure you want to end period {clock.period} at {formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}?
                </p>
                <p className="confirmation-warning">
                  This action cannot be undone.
                </p>
              </div>
              <div className="confirmation-actions">
                <button 
                  onClick={confirmEndPeriod} 
                  className="confirm-btn"
                  disabled={periodLoading[clock.period]}
                >
                  {periodLoading[clock.period] ? 'Ending...' : 'End Period'}
                </button>
                <button 
                  onClick={cancelEndPeriod} 
                  className="cancel-btn"
                  disabled={periodLoading[clock.period]}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Overtime Confirmation Dialog */}
        {showOvertimeConfirmation && (
          <div className="confirmation-dialog-overlay">
            <div className="confirmation-dialog">
              <div className="confirmation-header">
                <span className="confirmation-icon">⏰</span>
                <h3>Start Overtime</h3>
              </div>
              <div className="confirmation-content">
                <p>
                  Are you sure you want to start overtime for this match?
                </p>
                <p className="confirmation-warning">
                  This will begin the overtime period. The clock will be reset to 0:00.
                </p>
              </div>
              <div className="confirmation-actions">
                <button 
                  onClick={recordOvertime} 
                  className="confirm-btn"
                  disabled={loading}
                >
                  {loading ? 'Starting...' : 'Start Overtime'}
                </button>
                <button 
                  onClick={() => setShowOvertimeConfirmation(false)} 
                  className="cancel-btn"
                  disabled={loading}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Shootout Confirmation Dialog */}
        {showShootoutConfirmation && (
          <div className="confirmation-dialog-overlay">
            <div className="confirmation-dialog">
              <div className="confirmation-header">
                <span className="confirmation-icon">🎯</span>
                <h3>Start Shootout</h3>
              </div>
              <div className="confirmation-content">
                <p>
                  Are you sure you want to start a shootout for this match?
                </p>
                <p className="confirmation-warning">
                  Shootout does not use time keeping. Goals will be recorded without time.
                </p>
              </div>
              <div className="confirmation-actions">
                <button 
                  onClick={recordShootout} 
                  className="confirm-btn"
                  disabled={loading}
                >
                  {loading ? 'Starting...' : 'Start Shootout'}
                </button>
                <button 
                  onClick={() => setShowShootoutConfirmation(false)} 
                  className="cancel-btn"
                  disabled={loading}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

        <div className="modal-content">
          <div className="left-section">
          {/* Clock and Score Section */}
          <div className={`clock-score-section ${isInOvertime() ? 'overtime' : ''} ${isInShootout() ? 'shootout' : ''}`}>
            {currentMatch.status === 'Completed' && (
              <div className="match-finished-notice">
                <span className="notice-icon">🏁</span>
                <span className="notice-text">Match has been finished. Live tracking has been stopped.</span>
              </div>
            )}
            {currentMatch.status !== 'InProgress' && currentMatch.status !== 'Completed' && (
              <div className="not-live-notice">
                <span className="notice-icon">⏸️</span>
                <span className="notice-text">Match is not live yet. Use the clock button to start the match and first period.</span>
              </div>
            )}
            
            {/* Period Management - Simplified */}
            <div className="period-management">
              <div className="period-status">
                Period {clock.period}: {getPeriodStatus()}
              </div>
            </div>
            
            {/* Timer Component */}
            <div className="timer-container">
              {currentMatch.status === 'Scheduled' ? (
                <div className="start-match-container">
                  <button 
                    onClick={handleStartMatch}
                    disabled={loading}
                    className="start-match-btn"
                  >
                    🏁 Start Match
                  </button>
                  <div className="start-match-hint">
                    Click to start the match. After starting, you can use the timer controls below.
                  </div>
                </div>
              ) : currentMatch.status === 'InProgress' ? (
                <MemoizedTimer 
                  key={`timer-${currentMatch.id}`} // Remove status from key to prevent re-mounting
                  matchId={currentMatch.id} 
                  periodNumber={clock.period}
                  isActive={isOpen} // Only activate timer when modal is open
                  onTimerUpdate={(update) => {
                    console.log('Timer update in LiveMatchModal:', update);
                    // Update our local timer state for accurate time calculations
                    if (update.ElapsedTime) {
                      const timeParts = update.ElapsedTime.split(':');
                      if (timeParts.length === 3) {
                        // Parse hh:mm:ss format
                        const hours = parseInt(timeParts[0]) || 0;
                        const minutes = parseInt(timeParts[1]) || 0;
                        const seconds = parseInt(timeParts[2]) || 0;
                        const totalSeconds = hours * 3600 + minutes * 60 + seconds;
                        setCurrentTimerElapsedTime(totalSeconds);
                      } else if (timeParts.length === 2) {
                        // Parse mm:ss format (fallback)
                        const minutes = parseInt(timeParts[0]) || 0;
                        const seconds = parseInt(timeParts[1]) || 0;
                        const totalSeconds = minutes * 60 + seconds;
                        setCurrentTimerElapsedTime(totalSeconds);
                      }
                    }
                  }}
                  onGetCurrentTime={(getTime) => {
                    setGetCurrentTimeFromTimer(() => getTime);
                  }}
                />
              ) : (
                <div className="timer-loading">
                  <div>00:00</div>
                </div>
              )}
            </div>
            
            {/* Period Control Button - End Period or Start Period */}
            <div className="clock-start-reset">
              <button 
                onClick={handlePeriodControlClick} 
                className="period-control-btn"
                title={canEndPeriod() ? "End the current period" : "Start the next period"}
                disabled={periodLoading[canEndPeriod() ? clock.period : nextPeriodToStart]}
              >
                {getPeriodControlButtonText()}
              </button>
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
              
              <div className="form-row compact-time-row">
                <div className="time-info">
                  <div className="time-display">
                    <label>Current Time:</label>
                    <span className="current-time">
                      Period {clock.period} - {formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}
                    </span>
                  </div>
                </div>
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
                <div className="time-info">
                  <div className="time-display">
                    <label>Current Time:</label>
                    <span className="current-time">
                      Period {clock.period} - {formatTime(Math.floor(currentTimerElapsedTime / 60), currentTimerElapsedTime % 60)}
                    </span>
                  </div>
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
          
          {/* Right Section - Scoreboard, Period Management, and Events History */}
          <div className="right-section">
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

            {/* Events History */}
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
                              <strong>{event.teamName}</strong> - Goal by {event.playerName}
                              {event.assisterName && ` (Assist: ${event.assisterName})`}
                              {event.wasInOvertime && ` (OT)`}
                              {event.wasInShootout && ` (SO)`}
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
    </div>
  );
};

export default LiveMatchModal;