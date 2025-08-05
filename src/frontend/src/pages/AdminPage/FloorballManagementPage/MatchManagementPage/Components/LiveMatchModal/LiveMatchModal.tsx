import { useEffect, useMemo, useCallback, useState } from 'react';
import { floorballMatchEventService, type RecordSaveEventRequest } from '../../../../../../api/floorball/floorballMatchEventService';
import type { TimerUpdate } from '../../../../../../api/common/timerService';

// Import extracted components
import LiveMatchModalHeader from './components/LiveMatchModalHeader';
import LiveMatchScoreboard from './components/LiveMatchScoreboard';
import LiveMatchTimer from './components/LiveMatchTimer';
import LiveMatchQuickActions from './components/LiveMatchQuickActions';
import GoalRecordingForm from './components/GoalRecordingForm';
import PenaltyRecordingForm from './components/PenaltyRecordingForm';
import LiveMatchEventsHistory from './components/LiveMatchEventsHistory';
import ConfirmationDialog from './components/ConfirmationDialog';
import SaveRecordingSection from './components/SaveRecordingSection';

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

// Import types
import type { LiveMatchModalProps } from './components/types';
import './LiveMatchModal.scss';

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
  // Use custom hooks for business logic
  const matchData = useMatchData({
    match,
    onMatchUpdated,
    onStateUpdate
  });

  const timer = useLocalTimer({
    isOpen,
    onStateUpdate
  });

  const matchControls = useMatchControls({
    currentMatch: matchData.currentMatch,
    setCurrentMatch: matchData.setCurrentMatch,
    setError: matchData.setError,
    setLoading: matchData.setLoading,
    onGoLive,
    onCompleteLive
  });

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
    isOpen,
    onStateUpdate
  });

  const forms = useFormState({
    currentMatch: matchData.currentMatch,
    clock: timer.localClock,
    currentTimerElapsedTime: timer.currentTimerElapsedTime,
    loadMatchEvents: matchEvents.loadMatchEvents,
    setError: matchData.setError
  });
  // Loading state for save events and destructured dependencies
  const [saveLoading, setSaveLoading] = useState<boolean>(false);
  const matchId = matchData.currentMatch.id;
  const homeTeamId = matchData.homeTeam?.id ?? '';
  const awayTeamId = matchData.awayTeam?.id ?? '';
  const matchWentToOvertime = matchData.currentMatch.wentToOvertime;
  const matchWentToShootout = matchData.currentMatch.wentToShootout;
  const { loadMatchEvents } = matchEvents;
  const { setError } = matchData;

  const handleRecordSave = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    try {
      setSaveLoading(true);
      const payload: RecordSaveEventRequest = {
        goalieId,
        matchId,
        teamId: team === 'home' ? homeTeamId : awayTeamId,
        playerId: goalieId,
        periodNumber: timer.localClock.period,
        timeInSeconds: timer.currentTimerElapsedTime,
        wasInOvertime: matchWentToOvertime || timer.localClock.period > 3,
        wasInShootout: matchWentToShootout || timer.localClock.period > 4
      };
      await floorballMatchEventService.recordSave(payload);
      await loadMatchEvents();
      setError(null);
    } catch (error) {
      console.error('Error recording save:', error);
      setError(error instanceof Error ? error.message : 'Failed to record save');
    } finally {
      setSaveLoading(false);
    }
  }, [
    matchId,
    homeTeamId,
    awayTeamId,
    matchWentToOvertime,
    matchWentToShootout,
    loadMatchEvents,
    setError,
    timer.localClock.period,
    timer.currentTimerElapsedTime
  ]);

  const signalR = useSignalR({
    matchId: match.id,
    isOpen,
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
    setGetCurrentTimeFromTimer
  } = timer;
  const { setShowGoalForm, setShowPenaltyForm } = forms;
  const { setShowOvertimeConfirmation, setShowShootoutConfirmation } = periodManagement;

  // Calculate current score
  const currentScore = useMemo(() => {
    const baseScore = { home: matchData.currentMatch.homeScore, away: matchData.currentMatch.awayScore };
    
    if (liveState?.currentScore) {
      return liveState.currentScore;
    }
    
    return baseScore;
  }, [liveState?.currentScore, matchData.currentMatch.homeScore, matchData.currentMatch.awayScore]);

  // Destructure currentMatch and setter to satisfy update effect dependencies
  const { currentMatch: trackedMatch, setCurrentMatch } = matchData;

  // Update currentMatch when match prop changes - OPTIMIZED
  useEffect(() => {
    // Only update if actually different to prevent cascading re-renders
    if (match.id !== trackedMatch.id || match.status !== trackedMatch.status) {
      setCurrentMatch(match);
    }
  }, [match, trackedMatch.id, trackedMatch.status, setCurrentMatch]);

  // Initialize started periods when component loads - OPTIMIZED
  useEffect(() => {
    if (!isOpen) return;
    
    if (matchData.currentMatch.status === 'InProgress') {
      periodManagement.setStartedPeriods(new Set([1]));
      periodManagement.setNextPeriodToStart(2);
    } else {
      periodManagement.setStartedPeriods(new Set());
      periodManagement.setEndedPeriods(new Set());
      periodManagement.setNextPeriodToStart(1);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, matchData.currentMatch.status]); // Intentionally minimal deps for performance

  // Load team data and setup SignalR when modal opens - OPTIMIZED
  useEffect(() => {
    if (!isOpen) return;

    matchData.loadTeamData();
    matchEvents.loadMatchEvents();
    matchData.loadCurrentMatchStatus();
    signalR.setupSignalR();
    
    return () => {
      signalR.cleanupSignalR();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen, match.id]); // Intentionally minimal deps for performance

  // MEMOIZED: Handles the period control button click
  const handlePeriodControlClick = useCallback(() => {
    if (periodManagement.canEndPeriod()) {
      // Determine elapsed time (get from timer if available, else use last known)
      let totalSeconds = 0;
      if (getCurrentTimeFromTimer) {
        const timeParts = getCurrentTimeFromTimer().split(':');
        if (timeParts.length === 2) {
          const [m, s] = timeParts.map(p => parseInt(p, 10) || 0);
          totalSeconds = m * 60 + s;
        } else if (timeParts.length === 3) {
          const [h, m, s] = timeParts.map(p => parseInt(p, 10) || 0);
          totalSeconds = h * 3600 + m * 60 + s;
        }
      } else {
        totalSeconds = elapsedTime;
      }
      const isUnder20Minutes = totalSeconds < 1200;
      setCurrentTimerElapsedTime(totalSeconds);

      if (isUnder20Minutes) {
        periodManagement.setShowEndPeriodConfirmation(true);
        periodManagement.setPendingEndPeriodAction(() => periodManagement.endPeriod);
      } else {
        periodManagement.endPeriod();
      }
    } else {
      periodManagement.startPeriod();
    }
  }, [periodManagement, getCurrentTimeFromTimer, elapsedTime, setCurrentTimerElapsedTime]);

  // MEMOIZED: Timer update handler
  const handleTimerUpdate = useCallback((update: TimerUpdate) => {
    if (update.ElapsedTime) {
      const timeParts = update.ElapsedTime.split(':');
      let totalSeconds = 0;
      if (timeParts.length === 3) {
        const [h, m, s] = timeParts.map(part => parseInt(part, 10) || 0);
        totalSeconds = h * 3600 + m * 60 + s;
      } else if (timeParts.length === 2) {
        const [m, s] = timeParts.map(part => parseInt(part, 10) || 0);
        totalSeconds = m * 60 + s;
      }
      setCurrentTimerElapsedTime(totalSeconds);
    }
  }, [setCurrentTimerElapsedTime]);

  // MEMOIZED: Get current time handler
  const handleGetCurrentTime = useCallback((getTime: () => string) => {
    setGetCurrentTimeFromTimer(() => getTime);
  }, [setGetCurrentTimeFromTimer]);

  // MEMOIZED: Goal form show handler
  const handleShowGoalForm = useCallback(() => {
    setShowGoalForm(true);
  }, [setShowGoalForm]);

  // MEMOIZED: Goal form close handler
  const handleCloseGoalForm = useCallback(() => {
    setShowGoalForm(false);
  }, [setShowGoalForm]);

  // MEMOIZED: Penalty form close handler
  const handleClosePenaltyForm = useCallback(() => {
    setShowPenaltyForm(false);
  }, [setShowPenaltyForm]);

  // MEMOIZED: Error close handler
  const handleCloseError = useCallback(() => {
    setError(null);
  }, [setError]);

  // MEMOIZED: Overtime confirmation cancel handler
  const handleCancelOvertime = useCallback(() => {
    setShowOvertimeConfirmation(false);
  }, [setShowOvertimeConfirmation]);

  // MEMOIZED: Shootout confirmation cancel handler
  const handleCancelShootout = useCallback(() => {
    setShowShootoutConfirmation(false);
  }, [setShowShootoutConfirmation]);

  if (!isOpen) return null;

  return (
    <div className="live-match-modal-overlay">
      <div className="live-match-modal">
        {/* Header */}
        <LiveMatchModalHeader
          homeTeam={matchData.homeTeam}
          awayTeam={matchData.awayTeam}
          currentMatch={matchData.currentMatch}
          onClose={onClose}
          onCompleteLive={matchControls.handleCompleteLive}
        />

        {/* Error Display */}
        {matchData.error && (
          <div className="error-alert">
            <span className="error-icon">⚠️</span>
            <span className="error-text">{matchData.error}</span>
            <button onClick={handleCloseError} className="error-close">×</button>
          </div>
        )}

        {/* Confirmation Dialogs */}
        <ConfirmationDialog
          isOpen={periodManagement.showEndPeriodConfirmation}
          icon="⚠️"
          title="Confirm End Period"
          message={`Are you sure you want to end period ${timer.localClock.period} at ${timer.formatTime(Math.floor(timer.currentTimerElapsedTime / 60), timer.currentTimerElapsedTime % 60)}?`}
          warningMessage="This action cannot be undone."
          confirmText="End Period"
          isLoading={periodManagement.periodLoading[timer.localClock.period]}
          onConfirm={periodManagement.confirmEndPeriod}
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
          onConfirm={periodManagement.recordOvertime}
          onCancel={handleCancelOvertime}
        />

        <ConfirmationDialog
          isOpen={periodManagement.showShootoutConfirmation}
          icon="🎯"
          title="Start Shootout"
          message="Are you sure you want to start a shootout for this match?"
          warningMessage="Shootout does not use time keeping. Goals will be recorded without time."
          confirmText="Start Shootout"
          isLoading={matchData.loading}
          onConfirm={periodManagement.recordShootout}
          onCancel={handleCancelShootout}
        />

        <div className="modal-content">
          <div className="left-section">
            {/* Timer and Period Management Section */}
            <LiveMatchTimer
              currentMatch={matchData.currentMatch}
              clock={timer.localClock}
              isOpen={isOpen}
              loading={matchData.loading}
              startedPeriods={periodManagement.startedPeriods}
              endedPeriods={periodManagement.endedPeriods}
              nextPeriodToStart={periodManagement.nextPeriodToStart}
              periodLoading={periodManagement.periodLoading}
              currentTimerElapsedTime={timer.currentTimerElapsedTime}
              onStartMatch={matchControls.handleStartMatch}
              onPeriodControlClick={handlePeriodControlClick}
              onTimerUpdate={handleTimerUpdate}
              onGetCurrentTime={handleGetCurrentTime}
              canEndPeriod={periodManagement.canEndPeriod}
              getPeriodStatus={periodManagement.getPeriodStatus}
              getPeriodControlButtonText={periodManagement.getPeriodControlButtonText}
              isInOvertime={periodManagement.isInOvertime}
              isInShootout={periodManagement.isInShootout}
              formatTime={timer.formatTime}
            />
          <SaveRecordingSection
            currentMatch={matchData.currentMatch}
            homePlayers={matchData.homePlayers}
            awayPlayers={matchData.awayPlayers}
            onRecordSave={handleRecordSave}
            loading={saveLoading}
          />
            {/* Quick Actions */}
            <LiveMatchQuickActions
              loading={forms.loading}
              currentMatch={matchData.currentMatch}
              onShowGoalForm={handleShowGoalForm}
              onShowPenaltyForm={forms.openPenaltyForm}
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
              currentTimerElapsedTime={timer.currentTimerElapsedTime}
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
              currentTimerElapsedTime={timer.currentTimerElapsedTime}
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
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default LiveMatchModal;