import { useEffect, useMemo, useCallback } from 'react';
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

  const signalR = useSignalR({
    matchId: match.id,
    isOpen,
    onPeriodStarted: periodManagement.handlePeriodStarted,
    onGoalScored: matchEvents.handleGoalScored,
    onPenaltyAssigned: matchEvents.handlePenaltyAssigned
  });

  // Calculate current score
  const currentScore = useMemo(() => {
    const baseScore = { home: matchData.currentMatch.homeScore, away: matchData.currentMatch.awayScore };
    
    if (liveState?.currentScore) {
      return liveState.currentScore;
    }
    
    return baseScore;
  }, [liveState?.currentScore, matchData.currentMatch.homeScore, matchData.currentMatch.awayScore]);

  // Update currentMatch when match prop changes - OPTIMIZED
  useEffect(() => {
    // Only update if actually different to prevent cascading re-renders
    if (match.id !== matchData.currentMatch.id || match.status !== matchData.currentMatch.status) {
      matchData.setCurrentMatch(match);
    }
  }, [match.id, match.status, matchData.currentMatch.id, matchData.currentMatch.status, matchData.setCurrentMatch]);

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
      let currentTime = '00:00';
      let totalSeconds = 0;
      
      if (timer.getCurrentTimeFromTimer) {
        currentTime = timer.getCurrentTimeFromTimer();
        const timeParts = currentTime.split(':');
        if (timeParts.length === 2) {
          const minutes = parseInt(timeParts[0]) || 0;
          const seconds = parseInt(timeParts[1]) || 0;
          totalSeconds = minutes * 60 + seconds;
        } else if (timeParts.length === 3) {
          const hours = parseInt(timeParts[0]) || 0;
          const minutes = parseInt(timeParts[1]) || 0;
          const seconds = parseInt(timeParts[2]) || 0;
          totalSeconds = hours * 3600 + minutes * 60 + seconds;
        }
      } else {
        totalSeconds = timer.currentTimerElapsedTime;
      }
      
      const isUnder20Minutes = totalSeconds < 1200; // 20 minutes = 1200 seconds
      timer.setCurrentTimerElapsedTime(totalSeconds);
      
      if (isUnder20Minutes) {
        periodManagement.setShowEndPeriodConfirmation(true);
        periodManagement.setPendingEndPeriodAction(() => periodManagement.endPeriod);
      } else {
        periodManagement.endPeriod();
      }
    } else {
      periodManagement.startPeriod();
    }
  }, [
    periodManagement.canEndPeriod,
    periodManagement.startPeriod,
    periodManagement.endPeriod,
    periodManagement.setShowEndPeriodConfirmation,
    periodManagement.setPendingEndPeriodAction,
    periodManagement.startedPeriods,
    periodManagement.endedPeriods,
    periodManagement.nextPeriodToStart,
    periodManagement.periodLoading,
    timer.localClock.period,
    timer.getCurrentTimeFromTimer,
    timer.currentTimerElapsedTime,
    timer.setCurrentTimerElapsedTime,
    matchData.currentMatch.status
  ]);

  // MEMOIZED: Timer update handler
  const handleTimerUpdate = useCallback((update: TimerUpdate) => {
    if (update.ElapsedTime) {
      const timeParts = update.ElapsedTime.split(':');
      if (timeParts.length === 3) {
        const hours = parseInt(timeParts[0]) || 0;
        const minutes = parseInt(timeParts[1]) || 0;
        const seconds = parseInt(timeParts[2]) || 0;
        const totalSeconds = hours * 3600 + minutes * 60 + seconds;
        timer.setCurrentTimerElapsedTime(totalSeconds);
      } else if (timeParts.length === 2) {
        const minutes = parseInt(timeParts[0]) || 0;
        const seconds = parseInt(timeParts[1]) || 0;
        const totalSeconds = minutes * 60 + seconds;
        timer.setCurrentTimerElapsedTime(totalSeconds);
      }
    }
  }, [timer.setCurrentTimerElapsedTime]);

  // MEMOIZED: Get current time handler
  const handleGetCurrentTime = useCallback((getTime: () => string) => {
    timer.setGetCurrentTimeFromTimer(() => getTime);
  }, [timer.setGetCurrentTimeFromTimer]);

  // MEMOIZED: Goal form show handler
  const handleShowGoalForm = useCallback(() => {
    forms.setShowGoalForm(true);
  }, [forms.setShowGoalForm]);

  // MEMOIZED: Goal form close handler
  const handleCloseGoalForm = useCallback(() => {
    forms.setShowGoalForm(false);
  }, [forms.setShowGoalForm]);

  // MEMOIZED: Penalty form close handler
  const handleClosePenaltyForm = useCallback(() => {
    forms.setShowPenaltyForm(false);
  }, [forms.setShowPenaltyForm]);

  // MEMOIZED: Error close handler
  const handleCloseError = useCallback(() => {
    matchData.setError(null);
  }, [matchData.setError]);

  // MEMOIZED: Overtime confirmation cancel handler
  const handleCancelOvertime = useCallback(() => {
    periodManagement.setShowOvertimeConfirmation(false);
  }, [periodManagement.setShowOvertimeConfirmation]);

  // MEMOIZED: Shootout confirmation cancel handler
  const handleCancelShootout = useCallback(() => {
    periodManagement.setShowShootoutConfirmation(false);
  }, [periodManagement.setShowShootoutConfirmation]);

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