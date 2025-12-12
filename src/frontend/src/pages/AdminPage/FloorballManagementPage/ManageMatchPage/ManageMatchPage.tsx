import { useEffect, useMemo, useCallback, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { floorballMatchEventService, type RecordSaveEventRequest } from '../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { timerService } from '../../../../api/common/timerService';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import type { TimerUpdate } from '../../../../api/common/timerService';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

// Import extracted components
import LiveMatchModalHeader from './components/LiveMatchModalHeader';
import LiveMatchScoreboard from './components/LiveMatchScoreboard';
import LiveMatchTimer from './components/LiveMatchTimer';
import LiveMatchQuickActions from './components/LiveMatchQuickActions';
import GoalRecordingForm from './components/GoalRecordingForm';
import PenaltyRecordingForm from './components/PenaltyRecordingForm';
import LiveMatchEventsHistory from './components/LiveMatchEventsHistory';
import ConfirmationDialog from './components/ConfirmationDialog';
import ActivePlayersSelector from './components/ActivePlayersSelector';
import OfficialsSelectorSection from './components/OfficialsSelectorSection';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import type { ProcessedEvent } from './components/types';
import { floorballRefereeService } from '../../../../api/floorball/floorballRefereeService';

// Import custom hooks
import {
  useMatchData,
  useSignalR,
  usePeriodManagement,
  useMatchEvents,
  useFormState,
  useLocalTimer,
  useMatchControls
} from './hooks';

import './ManageMatchPage.scss';

interface ManageMatchPageContentProps {
  match: FloorballMatchDto;
  setMatch: (match: FloorballMatchDto) => void;
}

const ManageMatchPageContent = ({ match, setMatch }: ManageMatchPageContentProps) => {
  const navigate = useNavigate();
  // State for selected goalies (lifted up), initialized from match prop
  const [homeGoalieId, setHomeGoalieId] = useState<string>(match.homeActiveGoalieId || '');
  const [awayGoalieId, setAwayGoalieId] = useState<string>(match.awayActiveGoalieId || '');
  const [selectedOfficials, setSelectedOfficials] = useState<string[]>(match.officials || []);
  const [officialOptions, setOfficialOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [officialsSaving, setOfficialsSaving] = useState<boolean>(false);
  const [showEndMatchConfirmation, setShowEndMatchConfirmation] = useState(false);
  const lastSaveRef = useRef<Record<string, number>>({});

  // This effect ensures that if the match prop is updated from the server,
  // the local goalie state is synchronized. This is useful if the user
  // reloads the page for an already in-progress match.
  useEffect(() => {
    setHomeGoalieId(match.homeActiveGoalieId || '');
    setAwayGoalieId(match.awayActiveGoalieId || '');
    setSelectedOfficials(match.officials || []);
  }, [match.homeActiveGoalieId, match.awayActiveGoalieId, match.officials]);


  const handleStateUpdate = useCallback(() => {
    // This is a placeholder for now.
    // If we need to manage more complex state between hooks, we can implement a reducer here.
  }, []);

  const promoteGuestOfficial = useCallback((options: Array<{ id: string; name: string }>) => {
    const sorted = [...options].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
    const guestIndex = sorted.findIndex(option => option.name.toUpperCase() === 'GUEST REFEREE');
    if (guestIndex <= 0) {
      return sorted;
    }
    const guest = sorted[guestIndex];
    const remaining = sorted.filter((_, index) => index !== guestIndex);
    return [guest, ...remaining];
  }, []);

  // Use custom hooks for business logic
  const matchData = useMatchData({
    match,
    onMatchUpdated: setMatch,
    onStateUpdate: handleStateUpdate,
  });
  const { setError: setMatchError } = matchData;

  useEffect(() => {
    let isCancelled = false;
    const loadOfficials = async () => {
      if (!match.id) return;
      try {
        const response = await floorballRefereeService.getAll({ pageSize: 50 });
        if (!isCancelled && response.success && response.data) {
          const mapped = response.data.map(ref => ({ id: ref.id, name: ref.person.fullName }));
          setOfficialOptions(promoteGuestOfficial(mapped));
        }
      } catch (error) {
        if (!isCancelled) {
          console.error('Error loading officials:', error);
          setMatchError(error instanceof Error ? error.message : 'Failed to load officials');
        }
      }
    };

    loadOfficials();
    return () => {
      isCancelled = true;
    };
  }, [promoteGuestOfficial, match.id, setMatchError]);


  const timer = useLocalTimer({
    isOpen: true,
    matchId: match.id,
    onStateUpdate: handleStateUpdate,
  });

  const matchControls = useMatchControls({
    currentMatch: matchData.currentMatch,
    setCurrentMatch: matchData.setCurrentMatch,
    setError: matchData.setError,
    setLoading: matchData.setLoading,
    onGoLive: (_matchId: string, updatedMatch?: FloorballMatchDto) => {
      if (updatedMatch) setMatch(updatedMatch);
    },
    onCompleteLive: (_matchId: string, updatedMatch?: FloorballMatchDto) => {
      if (updatedMatch) setMatch(updatedMatch);
    },
  });
  const { handleStartMatch } = matchControls;

  const matchEvents = useMatchEvents({
    match,
    currentMatch: matchData.currentMatch,
    homeTeam: matchData.homeTeam,
    awayTeam: matchData.awayTeam,
    getPlayerNameById: matchData.getPlayerNameById,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus
  });

  const periodManagement = usePeriodManagement({
    currentMatch: matchData.currentMatch,
    clock: timer.localClock,
    setLocalClock: timer.setLocalClock,
    currentTimerElapsedTime: timer.currentTimerElapsedTime,
    isOpen: true,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus,
    onStateUpdate: handleStateUpdate,
  });

  const forms = useFormState({
    currentMatch: matchData.currentMatch,
    clock: timer.localClock,
    currentTimerElapsedTime: timer.currentTimerElapsedTime,
    loadMatchEvents: matchEvents.loadMatchEvents,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus,
    setError: matchData.setError
  });
  // Loading state for save events and destructured dependencies
  const [saveLoading, setSaveLoading] = useState<boolean>(false);
  // Delete confirmation state
  const [eventToDelete, setEventToDelete] = useState<ProcessedEvent | null>(null);
  const [deleteEventLoading, setDeleteEventLoading] = useState<boolean>(false);
  // Start timer right after starting match
  const [shouldStartTimer, setShouldStartTimer] = useState<boolean>(false);
  const homeTeamId = matchData.homeTeam?.id ?? '';
  const awayTeamId = matchData.awayTeam?.id ?? '';
  const matchWentToOvertime = matchData.currentMatch.wentToOvertime;
  const matchWentToShootout = matchData.currentMatch.wentToShootout;
  const { loadMatchEvents } = matchEvents;

  const handleRecordSave = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    if (!match.id) return;

    const key = `${match.id}:${team}:${goalieId}`;
    const now = Date.now();
    if (lastSaveRef.current[key] && now - lastSaveRef.current[key] < 1000) {
      return;
    }
    lastSaveRef.current[key] = now;

    try {
      setSaveLoading(true);
      const payload: RecordSaveEventRequest = {
        goalieId,
        matchId: match.id,
        teamId: team === 'home' ? homeTeamId : awayTeamId,
        playerId: goalieId,
        periodNumber: timer.localClock.period,
        timeInSeconds: timer.currentTimerElapsedTime,
        wasInOvertime: matchWentToOvertime || timer.localClock.period > 2,
        wasInShootout: matchWentToShootout || timer.localClock.period > 3
      };
      await floorballMatchEventService.recordSave(payload);
      await loadMatchEvents();
      matchData.setError(null);
    } catch (error) {
      console.error('Error recording save:', error);
      matchData.setError(error instanceof Error ? error.message : 'Failed to record save');
    } finally {
      setSaveLoading(false);
    }
  }, [
    match.id,
    homeTeamId,
    awayTeamId,
    matchWentToOvertime,
    matchWentToShootout,
    loadMatchEvents,
    timer.localClock.period,
    timer.currentTimerElapsedTime,
    matchData,
  ]);

  const signalR = useSignalR({
    matchId: match?.id,
    isOpen: true,
    onPeriodStarted: periodManagement.handlePeriodStarted,
    onGoalScored: matchEvents.handleGoalScored,
    onPenaltyAssigned: matchEvents.handlePenaltyAssigned,
    onSaveRecorded: matchEvents.handleSaveRecorded
  });

  // Destructure utilities to satisfy hook deps without pulling full objects
  const {
    currentTimerElapsedTime: elapsedTime,
    getCurrentTimeFromTimer,
    setCurrentTimerElapsedTime,
    setGetCurrentTimeFromTimer,
    getToggleFromTimer,
    setGetToggleFromTimer
  } = timer;
  const { setShowGoalForm, setShowPenaltyForm } = forms;
  const { setShowOvertimeConfirmation } = periodManagement;

  // Local refs to timer controls (start/reset)
  const [startTimerFn, setStartTimerFn] = useState<(() => Promise<void>) | null>(null);
  const [resetTimerFn, setResetTimerFn] = useState<(() => void) | null>(null);

  const handleGetStartFunction = useCallback((fn: () => Promise<void>) => {
    setStartTimerFn(() => fn);
  }, []);

  const handleGetResetFunction = useCallback((fn: () => void) => {
    setResetTimerFn(() => fn);
  }, []);

  // Calculate current score
  const currentScore = useMemo(() => {
    if (!match) return { home: 0, away: 0 };
    return { home: match.homeScore, away: match.awayScore };
  }, [match]);

  // Destructure currentMatch and setter to satisfy update effect dependencies
  const { currentMatch: trackedMatch, setCurrentMatch } = matchData;

  // Update internal currentMatch state when match prop changes
  useEffect(() => {
    if (match && (match.id !== trackedMatch.id || match.status !== trackedMatch.status)) {
      setCurrentMatch(match);
    }
  }, [match, trackedMatch.id, trackedMatch.status, setCurrentMatch]);

  // Initialize started periods when component loads
  useEffect(() => {
    const initializePeriodState = async () => {
      try {
        if (matchData.currentMatch.status === 'InProgress') {
          const timerStatus = await timerService.getTimerStatus(match.id);
          const currentPeriod = timerStatus.exists && timerStatus.periodNumber ? timerStatus.periodNumber : 1;
          
          const startedPeriods = new Set<number>();
          const endedPeriods = new Set<number>();
          
          // Use the periodScores from match data to determine which periods have ended
          const periodScores = matchData.currentMatch.periodScores || {};
          
          // Only mark periods as started if they've been completed or are the current period
          Object.entries(periodScores).forEach(([periodNum, scoreData]) => {
            const period = parseInt(periodNum);
            
            // A period is started if it's completed OR if it's the current period
            if (scoreData.isCompleted) {
              startedPeriods.add(period);
              endedPeriods.add(period);
            } else if (period === currentPeriod) {
              // Only mark as started if it's the current period from the timer
              startedPeriods.add(period);
            }
          });
          
          // Ensure current period is marked as started (safety check)
          startedPeriods.add(currentPeriod);
          
          // Next period is the first period that hasn't been started yet
          let nextPeriod = 1;
          for (let i = 1; i <= 4; i++) {
            if (!startedPeriods.has(i)) {
              nextPeriod = i;
              break;
            }
          }
          if (nextPeriod > 4 || (startedPeriods.has(4))) {
            nextPeriod = 0; // No more periods to start
          }
          
          console.log('Initialized period state:', { 
            startedPeriods: Array.from(startedPeriods), 
            endedPeriods: Array.from(endedPeriods), 
            nextPeriod,
            currentPeriod 
          });
          
          periodManagement.setStartedPeriods(startedPeriods);
          periodManagement.setEndedPeriods(endedPeriods);
          periodManagement.setNextPeriodToStart(nextPeriod);

        } else {
          periodManagement.setStartedPeriods(new Set());
          periodManagement.setEndedPeriods(new Set());
          periodManagement.setNextPeriodToStart(1);
        }
      } catch (error) {
        console.warn('Failed to initialize period state, using defaults:', error);
        if (matchData.currentMatch.status === 'InProgress') {
          periodManagement.setStartedPeriods(new Set([1]));
          periodManagement.setNextPeriodToStart(2);
        } else {
          periodManagement.setStartedPeriods(new Set());
          periodManagement.setEndedPeriods(new Set());
          periodManagement.setNextPeriodToStart(1);
        }
      }
    };
    
    initializePeriodState();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [matchData.currentMatch.status, match.id]);

  // Destructure the data-loading functions from hooks to use as stable dependencies
  const { loadTeamData, loadCurrentMatchStatus } = matchData;
  const { setupSignalR, cleanupSignalR } = signalR;

  // Load initial match data and set up SignalR connection
  useEffect(() => {
    loadTeamData();
    loadMatchEvents();
    loadCurrentMatchStatus();
    setupSignalR();

    return () => {
      cleanupSignalR();
    };
  }, [loadTeamData, loadMatchEvents, loadCurrentMatchStatus, setupSignalR, cleanupSignalR]);

  // Keybinds enabled only when match live, no forms open, and current period is active
  const isPeriodActive = periodManagement.startedPeriods.has(timer.localClock.period) &&
    !periodManagement.endedPeriods.has(timer.localClock.period);
  const keybindsEnabled = matchData.currentMatch.status === 'InProgress' &&
    isPeriodActive &&
    !forms.showGoalForm &&
    !forms.showPenaltyForm;

  // Handle Q/P/Space keybinds
  useEffect(() => {
    if (!keybindsEnabled) return;
    const handler = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement;
      if (!target) return;
      // Ignore typing in inputs or contenteditable
      if (['INPUT', 'TEXTAREA'].includes(target.tagName) || target.isContentEditable) {
        return;
      }
      const key = e.key.toLowerCase();
      if (key === 'q' && homeGoalieId) {
        handleRecordSave('home', homeGoalieId);
        e.preventDefault();
      }
      if (key === 'p' && awayGoalieId) {
        handleRecordSave('away', awayGoalieId);
        e.preventDefault();
      }
      if (key === ' ' && getToggleFromTimer) {
        getToggleFromTimer();
        e.preventDefault();
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [keybindsEnabled, homeGoalieId, awayGoalieId, handleRecordSave, getToggleFromTimer]);

  // MEMOIZED: Handles the period control button click
  const handlePeriodControlClick = useCallback(() => {
    if (periodManagement.canEndPeriod()) {
      // Special handling for shootout (period 4) - show confirmation before ending match
      if (timer.localClock.period === 4) {
        // Show confirmation dialog for ending the match
        setShowEndMatchConfirmation(true);
        return;
      }
      
      // Determine elapsed time (get from timer if available, else use last known)
      let totalSeconds = 0;
      if (getCurrentTimeFromTimer) {
        const timeParts = getCurrentTimeFromTimer().split(':');
        if (timeParts.length === 2) {
          const [m, s] = timeParts.map((p: string) => parseInt(p, 10) || 0);
          totalSeconds = m * 60 + s;
        } else if (timeParts.length === 3) {
          const [h, m, s] = timeParts.map((p: string) => parseInt(p, 10) || 0);
          totalSeconds = h * 3600 + m * 60 + s;
        }
      } else {
        totalSeconds = elapsedTime;
      }
      // 15-minute confirmation threshold
      const isUnder15Minutes = totalSeconds < 900;
      setCurrentTimerElapsedTime(totalSeconds);

      if (isUnder15Minutes) {
        periodManagement.setShowEndPeriodConfirmation(true);
      } else {
        // End immediately, then reset timer
        (async () => {
          await periodManagement.endPeriod();
          if (resetTimerFn) {
            resetTimerFn();
          }
        })();
      }
    } else {
      // Start the next period and immediately start the timer (unless it's shootout)
      (async () => {
        await periodManagement.startPeriod();
        // Don't start timer for shootout (period 4)
        if (periodManagement.nextPeriodToStart !== 4) {
          if (startTimerFn) {
            await startTimerFn();
          } else if (getToggleFromTimer) {
            await getToggleFromTimer();
          }
        }
      })();
    }
  }, [periodManagement, getCurrentTimeFromTimer, elapsedTime, setCurrentTimerElapsedTime, resetTimerFn, startTimerFn, getToggleFromTimer, timer.localClock.period, matchControls]);

  // MEMOIZED: Timer update handler
  const handleTimerUpdate = useCallback((update: TimerUpdate) => {
    if (update.ElapsedTime) {
      const timeParts = update.ElapsedTime.split(':');
      let totalSeconds = 0;
      if (timeParts.length === 3) {
        const [h, m, s] = timeParts.map((part: string) => parseInt(part, 10) || 0);
        totalSeconds = h * 3600 + m * 60 + s;
      } else if (timeParts.length === 2) {
        const [m, s] = timeParts.map((part: string) => parseInt(part, 10) || 0);
        totalSeconds = m * 60 + s;
      }
      setCurrentTimerElapsedTime(totalSeconds);
    }
  }, [setCurrentTimerElapsedTime]);

  // MEMOIZED: Get current time handler
  const handleGetCurrentTime = useCallback((getTime: () => string) => {
    setGetCurrentTimeFromTimer(prev => (prev !== getTime ? getTime : prev));
  }, [setGetCurrentTimeFromTimer]);

  // MEMOIZED: Get toggle function handler
  const handleGetToggleFunction = useCallback((toggleFunction: () => Promise<void>) => {
    setGetToggleFromTimer(prev => (prev !== toggleFunction ? toggleFunction : prev));
  }, [setGetToggleFromTimer]);


  // MEMOIZED: Start match and then start timer once timer is mounted
  const handleStartMatchAndTimer = useCallback(async () => {
    await handleStartMatch();
    // When match status flips to InProgress, Timer mounts and provides toggle
    setShouldStartTimer(true);
  }, [handleStartMatch]);

  // EFFECT: Trigger timer start exactly once after match starts, when toggle is ready
  useEffect(() => {
    if (!shouldStartTimer) return;
    if (getToggleFromTimer) {
      getToggleFromTimer();
      setShouldStartTimer(false);
    }
  }, [shouldStartTimer, getToggleFromTimer]);

  // MEMOIZED: Goal form close handler
  const handleCloseGoalForm = useCallback(() => {
    setShowGoalForm(false);
  }, [setShowGoalForm]);

  // MEMOIZED: Penalty form close handler
  const handleClosePenaltyForm = useCallback(() => {
    setShowPenaltyForm(false);
  }, [setShowPenaltyForm]);

  // MEMOIZED: Show confirmation when user wants to complete the match
  const handleCompleteMatchRequest = useCallback(() => {
    setShowEndMatchConfirmation(true);
  }, []);

  return (
    <>
      {/* Header */}
      <LiveMatchModalHeader
        homeTeam={matchData.homeTeam}
        awayTeam={matchData.awayTeam}
        currentMatch={matchData.currentMatch}
        onClose={() => navigate('/admin/floorball/matches')}
        onCompleteLive={handleCompleteMatchRequest}
      />

      {/* Error Display */}
      <ErrorPopup message={matchData.error} />

      {/* Confirmation Dialogs */}
      {/* Goalie change confirmation moved to GoalieSelectorSection */}

      <ConfirmationDialog
        isOpen={periodManagement.showEndPeriodConfirmation}
        icon="⚠️"
        title="Confirm End Period"
        message={`Are you sure you want to end period ${timer.localClock.period} at ${timer.formatTime(Math.floor(timer.currentTimerElapsedTime / 60), timer.currentTimerElapsedTime % 60)}?`}
        warningMessage="This action cannot be undone."
        confirmText="End Period"
        isLoading={periodManagement.periodLoading[timer.localClock.period]}
        onConfirm={async () => {
          await periodManagement.endPeriod();
          if (resetTimerFn) {
            resetTimerFn();
          }
          periodManagement.setShowEndPeriodConfirmation(false);
        }}
        onCancel={periodManagement.cancelEndPeriod}
      />

      <ConfirmationDialog
        isOpen={periodManagement.showOvertimeConfirmation}
        icon="⏰"
        title="Start Overtime"
        message="Are you sure you want to start overtime for this match?"
        warningMessage="This will begin the overtime period. The clock will be reset to 0:00."
        confirmText="Start Overtime"
        isLoading={matchData.loading}
        onConfirm={async () => {
          await periodManagement.recordOvertime();
          // Start timer for overtime
          if (startTimerFn) {
            await startTimerFn();
          } else if (getToggleFromTimer) {
            await getToggleFromTimer();
          }
        }}
        onCancel={() => setShowOvertimeConfirmation(false)}
      />

      <ConfirmationDialog
        isOpen={periodManagement.showShootoutConfirmation}
        icon="🎯"
        title="Start Shootout"
        message="Are you sure you want to start a shootout for this match?"
        warningMessage="Shootout does not use time keeping. Goals will be recorded without time."
        confirmText="Start Shootout"
        isLoading={matchData.loading}
        onConfirm={async () => {
          await periodManagement.recordShootout();
          if (startTimerFn) {
            await startTimerFn();
          } else if (getToggleFromTimer) {
            await getToggleFromTimer();
          }
        }}
        onCancel={() => periodManagement.setShowShootoutConfirmation(false)}
      />

      <ConfirmationDialog
        isOpen={showEndMatchConfirmation}
        icon="🏁"
        title="Confirm End Match"
        message={`Are you sure you want to complete this match?${timer.localClock.period === 4 ? ' This will end the shootout.' : ''}`}
        warningMessage="This will finalize the match results. This action cannot be undone."
        confirmText="Complete Match"
        isLoading={matchData.loading}
        onConfirm={async () => {
          await matchControls.handleCompleteLive();
          setShowEndMatchConfirmation(false);
        }}
        onCancel={() => setShowEndMatchConfirmation(false)}
      />

      {/* Delete Event Confirmation */}
      <ConfirmationDialog
        isOpen={!!eventToDelete}
        icon="🗑️"
        title="Delete Event"
        message={
          eventToDelete
            ? `Delete ${eventToDelete.type} for ${eventToDelete.teamName} at P${eventToDelete.periodNumber} ${timer.formatEventTime(eventToDelete.timeInSeconds)}?`
            : ''
        }
        warningMessage="This action cannot be undone."
        confirmText="Delete"
        isLoading={deleteEventLoading}
        onConfirm={async () => {
          if (!eventToDelete) return;
          if (!eventToDelete.eventId) {
            matchData.setError('Cannot delete: missing event id');
            setEventToDelete(null);
            return;
          }
          try {
            setDeleteEventLoading(true);
            matchData.setError(null);
            // Ensure match state is up-to-date before deletion
            await matchData.loadCurrentMatchStatus();
            if (eventToDelete.type === 'goal') {
              await floorballMatchService.deleteGoal(match.id, eventToDelete.eventId);
            } else if (eventToDelete.type === 'penalty') {
              await floorballMatchService.deletePenalty(match.id, eventToDelete.eventId);
            } else if (eventToDelete.type === 'save') {
              await floorballMatchService.deleteSave(match.id, eventToDelete.eventId);
            }
            await matchData.loadCurrentMatchStatus();
            await matchEvents.loadMatchEvents();
            setEventToDelete(null);
          } catch (err) {
            console.error('Failed to delete event', err);
            matchData.setError(err instanceof Error ? err.message : 'Failed to delete event');
            setEventToDelete(null);
          } finally {
            setDeleteEventLoading(false);
          }
        }}
        onCancel={() => setEventToDelete(null)}
      />

      <div className="modal-content">
        <div className="left-section">
          {/* Timer and Period Management Section */}
          <LiveMatchTimer
            currentMatch={matchData.currentMatch}
            clock={timer.localClock}
            isOpen={true} // Timer is always open in this view
            loading={matchData.loading}
            startedPeriods={periodManagement.startedPeriods}
            endedPeriods={periodManagement.endedPeriods}
            nextPeriodToStart={periodManagement.nextPeriodToStart}
            periodLoading={periodManagement.periodLoading}
            currentTimerElapsedTime={timer.currentTimerElapsedTime}
            onStartMatch={handleStartMatchAndTimer}
            onPeriodControlClick={handlePeriodControlClick}
            onTimerUpdate={handleTimerUpdate}
            onGetCurrentTime={handleGetCurrentTime}
            onGetToggleFunction={handleGetToggleFunction}
            onGetResetFunction={handleGetResetFunction}
            onGetStartFunction={handleGetStartFunction}
            canEndPeriod={periodManagement.canEndPeriod}
            getPeriodStatus={periodManagement.getPeriodStatus}
            getPeriodControlButtonText={periodManagement.getPeriodControlButtonText}
            isInOvertime={periodManagement.isInOvertime}
            isInShootout={periodManagement.isInShootout}
            formatTime={timer.formatTime}
            keybindsEnabled={keybindsEnabled}
            isStartMatchDisabled={!homeGoalieId || !awayGoalieId}
          />
          {/* Quick Actions */}
          <LiveMatchQuickActions
            loading={forms.loading}
            currentMatch={matchData.currentMatch}
            homeTeamId={matchData.homeTeam?.id}
            awayTeamId={matchData.awayTeam?.id}
            homeTeamName={matchData.homeTeam?.name}
            awayTeamName={matchData.awayTeam?.name}
            onShowGoalForm={forms.openGoalFormForTeam}
            onShowPenaltyForm={forms.openPenaltyFormForTeam}
            homeGoalieId={homeGoalieId}
            awayGoalieId={awayGoalieId}
            onRecordSave={handleRecordSave}
            keybindsEnabled={keybindsEnabled}
            saveLoading={saveLoading}
          />

          <ActivePlayersSelector
            homePlayers={matchData.homePlayers}
            awayPlayers={matchData.awayPlayers}
            homeTeamName={matchData.homeTeam?.name}
            awayTeamName={matchData.awayTeam?.name}
            homeGoalieId={homeGoalieId}
            awayGoalieId={awayGoalieId}
            setHomeGoalieId={setHomeGoalieId}
            setAwayGoalieId={setAwayGoalieId}
            currentMatch={matchData.currentMatch}
            onMatchUpdated={setMatch}
            setError={matchData.setError}
          />

          <OfficialsSelectorSection
            selectedOfficials={selectedOfficials}
            options={officialOptions}
            saving={officialsSaving}
            onAddRow={() => setSelectedOfficials(prev => [...prev, ''])}
            onSelect={async (index, refereeId) => {
              if (!match.id) return;
              if (!refereeId) return;

              const isDuplicate = selectedOfficials.some((id, idx) => id === refereeId && idx !== index);
              if (isDuplicate) {
                matchData.setError('Referee already selected in another slot');
                return;
              }

              const next = [...selectedOfficials];
              const wasEmpty = next[index] === '';
              next[index] = refereeId;

              try {
                setOfficialsSaving(true);
                matchData.setError(null);
                if (wasEmpty) {
                  const resp = await floorballMatchService.addOfficial(match.id, refereeId);
                  if (resp.success && resp.data) {
                    setSelectedOfficials(resp.data.officials);
                    setCurrentMatch(resp.data);
                    setMatch(resp.data);
                  }
                } else {
                  const resp = await floorballMatchService.updateOfficials(match.id, next);
                  if (resp.success && resp.data) {
                    setSelectedOfficials(resp.data.officials);
                    setCurrentMatch(resp.data);
                    setMatch(resp.data);
                  }
                }
              } catch (error) {
                console.error('Error selecting official:', error);
                matchData.setError(error instanceof Error ? error.message : 'Failed to set official');
              } finally {
                setOfficialsSaving(false);
              }
            }}
            onRemove={async (index, refereeId) => {
              if (!match.id) {
                return;
              }
              if (!refereeId) {
                setSelectedOfficials(prev => prev.filter((_, idx) => idx !== index));
                return;
              }
              try {
                setOfficialsSaving(true);
                matchData.setError(null);
                const resp = await floorballMatchService.deleteOfficial(match.id, refereeId);
                if (resp.success && resp.data) {
                  setSelectedOfficials(resp.data.officials);
                  setCurrentMatch(resp.data);
                  setMatch(resp.data);
                }
              } catch (error) {
                console.error('Error removing official:', error);
                matchData.setError(error instanceof Error ? error.message : 'Failed to remove official');
              } finally {
                setOfficialsSaving(false);
              }
            }}
          />

          {/* Forms */}
          <GoalRecordingForm
            showGoalForm={forms.showGoalForm}
            goalForm={forms.goalForm}
            setGoalForm={forms.setGoalForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            clock={timer.localClock}
            loading={forms.loading}
            getPlayersForTeam={matchData.getPlayersForTeam}
            onRecordGoal={forms.recordGoal}
            onClose={handleCloseGoalForm}
            formatTime={timer.formatTime}
          />

          <PenaltyRecordingForm
            showPenaltyForm={forms.showPenaltyForm}
            penaltyForm={forms.penaltyForm}
            setPenaltyForm={forms.setPenaltyForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            clock={timer.localClock}
            loading={forms.loading}
            getPlayersForTeam={matchData.getPlayersForTeam}
            onRecordPenalty={forms.recordPenalty}
            onClose={handleClosePenaltyForm}
            formatTime={timer.formatTime}
          />
        </div>
        
        {/* Right Section */}
        <div className="right-section">
          <LiveMatchScoreboard
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            currentScore={currentScore}
          />

          <LiveMatchEventsHistory
            allEvents={matchEvents.allEvents}
            formatEventTime={timer.formatEventTime}
            onDeleteEvent={(event) => {
              if (!event.eventId) {
                matchData.setError('Cannot delete: missing event id');
                return;
              }
              setEventToDelete(event);
            }}
          />
        </div>
      </div>
    </>
  )
}

const ManageMatchPage = () => {
  const { matchId } = useParams<{ matchId: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!matchId) {
      setError('Match ID is missing');
      setLoading(false);
      return;
    }

    const fetchMatch = async () => {
      try {
        const response = await floorballMatchService.getById(matchId);
        if (response.success && response.data) {
          setMatch(response.data);
        } else {
          setError('Failed to fetch match data');
        }
      } catch (err) {
        setError('An error occurred while fetching match data.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchMatch();
  }, [matchId]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return (
      <div className="manage-match-page">
        <ErrorPopup message={error} />
      </div>
    );
  }

  if (!match) {
    return <div>Match not found.</div>;
  }

  return (
    <PageTemplate title={'Manage match page'}>
    <div className="manage-match-page">
        <div className="page-header">
          <div className="page-header__top">
            <h1 className="page-title-compact font-title">MATCH MANAGEMENT</h1>
          </div>
        </div>
        <ManageMatchPageContent match={match} setMatch={setMatch} />
    </div>
    </PageTemplate>
  );
};

export default ManageMatchPage;