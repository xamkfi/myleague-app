import { useState, useEffect, useCallback, useMemo } from 'react';
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
import './LiveMatchModal.scss';

interface LiveMatchModalProps {
  match: FloorballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onGoLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
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
  onStateUpdate
}: LiveMatchModalProps) => {
  // State management
  const [homeTeam, setHomeTeam] = useState<FloorballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FloorballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FloorballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [matchEvents, setMatchEvents] = useState<FloorballDomainEventDto[]>([]);
  
  // Period state management
  const [periodStates, setPeriodStates] = useState<Record<number, 'not_started' | 'started' | 'ended'>>({});
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});
  
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
  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState(false);
  const [pendingEndPeriodAction, setPendingEndPeriodAction] = useState<(() => void) | null>(null);
  const [showOvertimeConfirmation, setShowOvertimeConfirmation] = useState(false);
  const [showShootoutConfirmation, setShowShootoutConfirmation] = useState(false);

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

  /**
   * Loads the current match status from the backend
   * This ensures we have the most up-to-date match information
   */
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

  /**
   * Loads all match events (goals and penalties) from the backend
   * The backend returns domain events in a flat array structure
   * This function fetches the events and stores them for processing
   */
  const loadMatchEvents = async () => {
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
  };

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
        console.log('SignalR connected, subscribing to events...');
        await signalRService.subscribeToEventType('FloorballGoalScored');
        await signalRService.subscribeToEventType('FloorballPenaltyAssigned');
        
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
        await signalRService.unsubscribeFromEventType('FloorballGoalScored');
        await signalRService.unsubscribeFromEventType('FloorballPenaltyAssigned');
      }
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
      // Don't throw - cleanup errors are not critical
    }
  };

  /**
   * Handles real-time SignalR events for this match
   * Filters events to only process those relevant to this match
   * Updates the UI immediately when events are received
   */
  const handleSignalREvent = useCallback((event: MatchEvent) => {
    console.log('Received match event:', event);
    
    const eventData = event.data as { MatchId?: string };
    if (eventData?.MatchId !== match.id) {
      return; // Event is not for this match
    }
    
    if (event.eventType === 'FloorballGoalScored') {
      handleGoalScored(event.data as GoalEventData);
    } else if (event.eventType === 'FloorballPenaltyAssigned') {
      handlePenaltyAssigned(event.data as PenaltyEventData);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [match.id]);

  /**
   * Handles real-time goal events from SignalR
   * Updates the score immediately and refreshes the events list
   * This provides instant feedback when goals are recorded
   */
  const handleGoalScored = useCallback((eventData: GoalEventData) => {
    console.log('handleGoalScored called with eventData:', eventData);
    console.log('onStateUpdate available:', !!onStateUpdate);
    console.log('currentScore:', currentScore);
    console.log('match.homeTeamId:', match.homeTeamId);
    console.log('match.awayTeamId:', match.awayTeamId);
    
    if (!onStateUpdate) return;
    
    // Update score
    const newScore = {
      home: eventData.TeamId === match.homeTeamId ? currentScore.home + 1 : currentScore.home,
      away: eventData.TeamId === match.awayTeamId ? currentScore.away + 1 : currentScore.away
    };
    
    console.log('Calculated newScore:', newScore);
    
    onStateUpdate({
      currentScore: newScore
    });
    
    // Refresh events from backend
    loadMatchEvents();
  }, [onStateUpdate, match.homeTeamId, match.awayTeamId, currentScore]);

  /**
   * Handles real-time penalty events from SignalR
   * Refreshes the events list to show the new penalty
   */
  const handlePenaltyAssigned = useCallback((_eventData: PenaltyEventData) => {
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

  /**
   * Combined function that handles starting the match, period, and clock
   * This replaces the separate Go Live, Start Period, and Start Clock buttons
   */
  const handleStartMatchAndPeriod = async () => {
    if (!onStateUpdate) return;
    
    try {
      setLoading(true);
      setError(null);
      
      // Step 1: Start the match if not already live
      if (currentMatch.status !== 'InProgress') {
        console.log('Starting match...');
        const response = await floorballMatchService.start(currentMatch.id);
        
        if (response.success && response.data) {
          setCurrentMatch(response.data);
          if (onGoLive) {
            onGoLive(currentMatch.id, response.data);
          }
        } else {
          throw new Error('Failed to start match');
        }
      }
      
      // Step 2: Start the current period if not already started
      const currentPeriodState = periodStates[clock.period];
      if (currentPeriodState !== 'started' && currentPeriodState !== 'ended') {
        console.log(`Starting period ${clock.period}...`);
        setPeriodLoading(prev => ({ ...prev, [clock.period]: true }));
        
        await floorballMatchEventService.startPeriod(currentMatch.id, clock.period);
        console.log(`Started period ${clock.period} for match ${currentMatch.id}`);
        
        // Update period state
        setPeriodStates(prev => ({ ...prev, [clock.period]: 'started' }));
      }
      
      // Step 3: Start the clock
      console.log('Starting clock...');
      onStateUpdate({
        clock: { ...clock, isRunning: true }
      });
      
      setError(null);
    } catch (error) {
      console.error('Error starting match and period:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match and period');
    } finally {
      setLoading(false);
      setPeriodLoading(prev => ({ ...prev, [clock.period]: false }));
    }
  };

  /**
   * Pause the clock (only when clock is running)
   */
  const handlePauseClock = () => {
    if (!onStateUpdate) return;
    onStateUpdate({
      clock: { ...clock, isRunning: false }
    });
  };

  /**
   * Determines if the combined start button should be enabled
   * @returns true if the button can be used
   */
  const canStartMatchAndPeriod = () => {
    // Can start if:
    // 1. Match is not completed
    // 2. Not currently loading
    // 3. Clock is not running (if clock is running, we show pause button)
    // 4. Current period is not ended
    const currentPeriodState = periodStates[clock.period];
    return currentMatch.status !== 'Completed' && 
           !loading && 
           !clock.isRunning && 
           currentPeriodState !== 'ended';
  };

  /**
   * Determines if the pause button should be enabled
   * @returns true if the pause button can be used
   */
  const canPauseClock = () => {
    // Can pause if:
    // 1. Match is not completed
    // 2. Not currently loading
    // 3. Clock is running
    return currentMatch.status !== 'Completed' && 
           !loading && 
           clock.isRunning;
  };

  /**
   * Determines if the main clock button should be enabled
   * @returns true if the button can be used
   */
  const canUseMainClockButton = () => {
    if (clock.isRunning) {
      return canPauseClock();
    } else {
      return canStartMatchAndPeriod();
    }
  };

  /**
   * Gets the button text based on current state
   * @returns The appropriate button text
   */
  const getStartButtonText = () => {
    if (clock.isRunning) {
      return '⏸️ Pause';
    }
    
    if (currentMatch.status !== 'InProgress') {
      return '🟢 Start Match & 1st Period';
    }
    
    // Check if we're in overtime
    if (isInOvertime()) {
      const currentPeriodState = periodStates[clock.period];
      if (currentPeriodState === 'started') {
        return '▶️ Start Clock';
      }
      return '⏰ Start Overtime';
    }
    
    // Check if we're in shootout
    if (isInShootout()) {
      const currentPeriodState = periodStates[clock.period];
      if (currentPeriodState === 'started') {
        return '▶️ Start Clock';
      }
      return '🎯 Start Shootout';
    }
    
    const currentPeriodState = periodStates[clock.period];
    if (currentPeriodState === 'started') {
      return '▶️ Start Clock';
    }
    
    return '🟢 Start Period & Clock';
  };

  /**
   * Handles the main start/pause button click
   */
  const handleMainClockButton = () => {
    if (clock.isRunning) {
      // If clock is running, pause it
      handlePauseClock();
    } else {
      // If clock is not running, start match/period/clock
      handleStartMatchAndPeriod();
    }
  };

  const resetClock = () => {
    if (!onStateUpdate) return;
    onStateUpdate({
      clock: { ...clock, minutes: 0, seconds: 0, isRunning: false }
    });
  };

  /**
   * Ends the current period by sending API call
   * This is separate from clock control
   */
  const endPeriod = async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [clock.period]: true }));
      await floorballMatchEventService.endPeriod(currentMatch.id, clock.period);
      console.log(`Ended period ${clock.period} for match ${currentMatch.id}`);
      
      // Update period state
      setPeriodStates(prev => ({ ...prev, [clock.period]: 'ended' }));
      
      // If we're ending overtime (period 4), show shootout button
      if (clock.period === 4 && currentMatch.wentToOvertime) {
        // The shootout button will be available after ending overtime
        console.log('Overtime ended, shootout button will be available');
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
   * Handles the end period button click with validation
   */
  const handleEndPeriodClick = () => {
    const totalSeconds = clock.minutes * 60 + clock.seconds;
    const isUnder20Minutes = totalSeconds < 1200; // 20 minutes = 1200 seconds
    
    if (isUnder20Minutes) {
      // Show confirmation dialog for periods under 20 minutes
      setShowEndPeriodConfirmation(true);
      setPendingEndPeriodAction(() => endPeriod);
    } else {
      // End period immediately if 20 minutes or more
      endPeriod();
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
      if (onStateUpdate) {
        onStateUpdate({
          clock: { 
            period: 4, 
            minutes: 0, 
            seconds: 0, 
            isRunning: false 
          }
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
      if (onStateUpdate) {
        onStateUpdate({
          clock: { 
            period: 5, 
            minutes: 0, 
            seconds: 0, 
            isRunning: false 
          }
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
   * Determines if we should show overtime button instead of next period
   */
  const shouldShowOvertimeButton = () => {
    return currentMatch.status === 'InProgress' && 
           clock.period >= 3 && 
           !currentMatch.wentToOvertime;
  };

  /**
   * Determines if we should show shootout button instead of next period
   */
  const shouldShowShootoutButton = () => {
    return currentMatch.status === 'InProgress' && 
           currentMatch.wentToOvertime && 
           !currentMatch.wentToShootout;
  };

  /**
   * Determines if we're currently in overtime
   */
  const isInOvertime = () => {
    return currentMatch.wentToOvertime && clock.period > 3;
  };

  /**
   * Determines if we're currently in shootout
   */
  const isInShootout = () => {
    return currentMatch.wentToShootout && clock.period > 4;
  };

  /**
   * Gets the text for the end period button
   */
  const getEndPeriodButtonText = () => {
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
  };

  /**
   * Gets the text for the next period button
   */
  const getNextPeriodButtonText = () => {
    if (shouldShowOvertimeButton()) {
      return '⏰ Overtime';
    }
    
    if (shouldShowShootoutButton()) {
      return '🎯 Shootout';
    }
    
    return '➡️ Next Period';
  };

  /**
   * Handles the next period/overtime/shootout button click
   */
  const handleNextPeriodClick = () => {
    if (shouldShowOvertimeButton()) {
      setShowOvertimeConfirmation(true);
    } else if (shouldShowShootoutButton()) {
      setShowShootoutConfirmation(true);
    } else {
      nextPeriod();
    }
  };

  const previousPeriod = async () => {
    if (!onStateUpdate || clock.period <= 1) return;
    
    try {
      // End current period if it's running
      await floorballMatchEventService.endPeriod(currentMatch.id, clock.period);
      console.log(`Ended period ${clock.period} for match ${currentMatch.id}`);
      
      const newPeriod = clock.period - 1;
      
      // Start the previous period
      await floorballMatchEventService.startPeriod(currentMatch.id, newPeriod);
      console.log(`Started period ${newPeriod} for match ${currentMatch.id}`);
      
      onStateUpdate({
        clock: { 
          period: newPeriod, 
          minutes: 0, 
          seconds: 0, 
          isRunning: false 
        }
      });
    } catch (error) {
      console.error('Error going to previous period:', error);
      setError(error instanceof Error ? error.message : 'Failed to go to previous period');
    }
  };

  const nextPeriod = async () => {
    if (!onStateUpdate) return;
    
    try {
      // End current period if it's running
      await floorballMatchEventService.endPeriod(currentMatch.id, clock.period);
      console.log(`Ended period ${clock.period} for match ${currentMatch.id}`);
      
      const newPeriod = clock.period + 1;
      
      // Start the next period
      await floorballMatchEventService.startPeriod(currentMatch.id, newPeriod);
      console.log(`Started period ${newPeriod} for match ${currentMatch.id}`);
      
      onStateUpdate({
        clock: { 
          period: newPeriod, 
          minutes: 0, 
          seconds: 0, 
          isRunning: false 
        }
      });
    } catch (error) {
      console.error('Error going to next period:', error);
      setError(error instanceof Error ? error.message : 'Failed to go to next period');
    }
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

  /**
   * Determines if the End Period button should be enabled
   * @returns true if the period can be ended
   */
  const canEndPeriod = () => {
    const currentPeriodState = periodStates[clock.period];
    return currentMatch.status === 'InProgress' && 
           !periodLoading[clock.period] && 
           currentPeriodState === 'started';
  };

  /**
   * Gets the current period status for display
   * @returns A string describing the current period status
   */
  const getPeriodStatus = () => {
    const currentPeriodState = periodStates[clock.period];
    if (periodLoading[clock.period]) {
      return 'Processing...';
    }
    switch (currentPeriodState) {
      case 'started':
        return '🟢 Started';
      case 'ended':
        return '🔴 Ended';
      default:
        return '⏸️ Not Started';
    }
  };

  const formatEventTime = (timeInSeconds: number) => {
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
  const getPlayerNameById = (playerId: string): string => {
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
          const goalData = event.data as any;
          console.log('Goal data structure:', goalData);
          console.log('Goal data keys:', Object.keys(goalData));
          
          // Extract player IDs with fallback property names (handles both camelCase and PascalCase)
          const playerId = goalData.PlayerId || goalData.playerId;
          const assisterId = goalData.AssisterId || goalData.assisterId;
          
          return {
            id: `goal-${goalData.TeamId || goalData.teamId || 'unknown'}-${playerId || 'unknown'}-${goalData.PeriodNumber || goalData.periodNumber || 1}-${goalData.TimeInSeconds || goalData.timeInSeconds || 0}`,
            type: 'goal' as const,
            teamId: goalData.TeamId || goalData.teamId,
            teamName: (goalData.TeamId || goalData.teamId) === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            playerId: playerId || 'Unknown Player',
            playerName: getPlayerNameById(playerId), // Look up player name from loaded data
            assisterId: assisterId,
            assisterName: assisterId ? getPlayerNameById(assisterId) : undefined, // Look up assister name
            periodNumber: goalData.PeriodNumber || goalData.periodNumber || 1,
            timeInSeconds: goalData.TimeInSeconds || goalData.timeInSeconds || 0,
            timestamp: new Date(event.occurredOn),
            wasInOvertime: goalData.WasInOvertime || goalData.wasInOvertime || false,
            wasInShootout: goalData.WasInShootout || goalData.wasInShootout || false
          };
        } 
        // Handle penalty events
        else if (event.eventType === 'FloorballPenaltyAssignedEvent') {
          const penaltyData = event.data as any;
          console.log('Penalty data structure:', penaltyData);
          console.log('Penalty data keys:', Object.keys(penaltyData));
          
          const playerId = penaltyData.PlayerId || penaltyData.playerId;
          
          return {
            id: `penalty-${penaltyData.TeamId || penaltyData.teamId || 'unknown'}-${playerId || 'team'}-${penaltyData.PeriodNumber || penaltyData.periodNumber || 1}-${penaltyData.TimeInSeconds || penaltyData.timeInSeconds || 0}`,
            type: 'penalty' as const,
            teamId: penaltyData.TeamId || penaltyData.teamId,
            teamName: (penaltyData.TeamId || penaltyData.teamId) === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            playerId: playerId,
            playerName: playerId ? getPlayerNameById(playerId) : 'Team Penalty', // Handle team penalties
            periodNumber: penaltyData.PeriodNumber || penaltyData.periodNumber || 1,
            timeInSeconds: penaltyData.TimeInSeconds || penaltyData.timeInSeconds || 0,
            timestamp: new Date(event.occurredOn),
            penaltyType: penaltyData.PenaltyType || penaltyData.penaltyType || 'Unknown',
            penaltyMinutes: penaltyData.Minutes || penaltyData.minutes || 2,
            description: penaltyData.Description || penaltyData.description || ''
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
                  Are you sure you want to end period {clock.period} at {formatTime(clock.minutes, clock.seconds)}?
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
          <div className={`clock-score-section ${isInOvertime() ? 'overtime' : ''}`}>
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
            <div className="match-clock">
              <div className="period-management">
                <div className="period-status">
                  Period {clock.period}: {getPeriodStatus()}
                </div>
              </div>
              <div className="previous-next-period">
                <button 
                  onClick={previousPeriod} 
                  className="period-control-btn" 
                  title="Go to previous period"
                  disabled={currentMatch.status !== 'InProgress' || clock.period <= 1}
                >
                  ⬅️ Previous Period
                </button>
                <button 
                  onClick={handleNextPeriodClick} 
                  className="period-control-btn"
                  title="Go to next period or overtime/shootout"
                  disabled={currentMatch.status !== 'InProgress'}
                >
                  {getNextPeriodButtonText()}
                </button>
              </div>
              <div className="period">Period {clock.period}</div>
              <div className={`time-display ${isTimeOverLimit(clock.minutes, clock.seconds) ? 'time-over-limit' : ''} ${isInOvertime() ? 'overtime' : ''}`}>
                {formatTime(clock.minutes, clock.seconds)}
              </div>
              <div className="clock-start-reset">
                <button 
                  onClick={handleMainClockButton} 
                  className={`start-pause-btn ${clock.isRunning ? 'pause' : ''}`}
                  disabled={!canUseMainClockButton()}
                >
                  {getStartButtonText()}
                </button>
                <button 
                  onClick={handleEndPeriodClick} 
                  className="end-period-btn"
                  title="End the current period"
                  disabled={!canEndPeriod()}
                >
                  {getEndPeriodButtonText()}
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
                <div className="time-inputs-container">
                  <div className="time-input-group">
                    <label>Period:</label>
                    <input 
                      type="number" 
                      value={penaltyForm.periodNumber}
                      onChange={(e) => setPenaltyForm(prev => ({ ...prev, periodNumber: parseInt(e.target.value) || 1 }))}
                      min="1"
                      max="10"
                      className="time-input period-input"
                    />
                  </div>
                  
                  <div className="time-input-group">
                    <label>Minutes:</label>
                    <input 
                      type="number" 
                      value={penaltyForm.timeMinutes}
                      onChange={(e) => setPenaltyForm(prev => ({ ...prev, timeMinutes: parseInt(e.target.value) || 0 }))}
                      min="0"
                      max="20"
                      placeholder={`${clock.minutes}`}
                      className="time-input minutes-input"
                    />
                  </div>
                  
                  <div className="time-input-group">
                    <label>Seconds:</label>
                    <input 
                      type="number" 
                      value={penaltyForm.timeSeconds}
                      onChange={(e) => setPenaltyForm(prev => ({ ...prev, timeSeconds: parseInt(e.target.value) || 0 }))}
                      min="0"
                      max="59"
                      placeholder={`${clock.seconds}`}
                      className="time-input seconds-input"
                    />
                  </div>
                </div>
                
                <div className="time-hint">
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