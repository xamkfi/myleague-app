import { useEffect, useMemo, useCallback, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { floorballMatchEventService, type RecordSaveEventRequest } from '../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { timerService } from '../../../../api/common/timerService';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import type { TimerUpdate } from '../../../../api/common/timerService';
import Navbar from '../../../../components/Navigation/Navbar';
import BackButton from '../../../../components/BackButton/BackButton';

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
  const [pendingGoalieChange, setPendingGoalieChange] = useState<{ team: 'home' | 'away'; goalieId: string; goalieName: string } | null>(null);

  // This effect ensures that if the match prop is updated from the server,
  // the local goalie state is synchronized. This is useful if the user
  // reloads the page for an already in-progress match.
  useEffect(() => {
    setHomeGoalieId(match.homeActiveGoalieId || '');
    setAwayGoalieId(match.awayActiveGoalieId || '');
  }, [match.homeActiveGoalieId, match.awayActiveGoalieId]);

  const handleConfirmGoalieChange = async () => {
    if (!pendingGoalieChange || !match.id) return;

    const { team, goalieId } = pendingGoalieChange;
    const teamId = team === 'home' ? matchData.homeTeam?.id : matchData.awayTeam?.id;

    if (!teamId) return;

    try {
      setError(null);
      const response = await floorballMatchService.changeGoalie(match.id, teamId, goalieId);

      if (response.success && response.data) {
        setMatch(response.data); // This is the critical line that was missing
      } else {
        throw new Error(response.errors?.join(', ') || 'Failed to change goalie');
      }
    } catch (error) {
      console.error(`Error setting ${team} goalie:`, error);
      setError(error instanceof Error ? error.message : `Failed to set ${team} goalie`);
    } finally {
      setPendingGoalieChange(null);
    }
  };

  const handleStateUpdate = useCallback(() => {
    // This is a placeholder for now.
    // If we need to manage more complex state between hooks, we can implement a reducer here.
  }, []);

  // Use custom hooks for business logic
  const matchData = useMatchData({
    match,
    onMatchUpdated: setMatch,
    onStateUpdate: handleStateUpdate,
  });

  // Destructure properties from matchData to use as stable dependencies in effects
  const { homeTeam, awayTeam, homePlayers, awayPlayers, setError } = matchData;

  useEffect(() => {
    const changeGoalie = async () => {
      if (homeGoalieId && match.id && homeTeam?.id) {
        if (match.status === 'InProgress' && match.homeActiveGoalieId !== homeGoalieId) {
          const newGoalie = homePlayers.find(p => p.id === homeGoalieId);
          setPendingGoalieChange({ 
            team: 'home', 
            goalieId: homeGoalieId, 
            goalieName: newGoalie ? `${newGoalie.person.firstName} ${newGoalie.person.lastName}` : 'Unknown Player'
          });
        } else {
          try {
            setError(null);
            await floorballMatchService.changeGoalie(match.id, homeTeam.id, homeGoalieId);
          } catch (error) {
            console.error('Error setting home goalie:', error);
            setError(error instanceof Error ? error.message : 'Failed to set home goalie');
          }
        }
      }
    };
    changeGoalie();
  }, [homeGoalieId, match.id, homeTeam, homePlayers, match.status, match.homeActiveGoalieId, setError]);

  useEffect(() => {
    const changeGoalie = async () => {
      if (awayGoalieId && match.id && awayTeam?.id) {
        if (match.status === 'InProgress' && match.awayActiveGoalieId !== awayGoalieId) {
          const newGoalie = awayPlayers.find(p => p.id === awayGoalieId);
          setPendingGoalieChange({ 
            team: 'away', 
            goalieId: awayGoalieId,
            goalieName: newGoalie ? `${newGoalie.person.firstName} ${newGoalie.person.lastName}` : 'Unknown Player'
          });
        } else {
          try {
            setError(null);
            await floorballMatchService.changeGoalie(match.id, awayTeam.id, awayGoalieId);
          } catch (error) {
            console.error('Error setting away goalie:', error);
            setError(error instanceof Error ? error.message : 'Failed to set away goalie');
          }
        }
      }
    };
    changeGoalie();
  }, [awayGoalieId, match.id, awayTeam, awayPlayers, match.status, match.awayActiveGoalieId, setError]);

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
  const homeTeamId = matchData.homeTeam?.id ?? '';
  const awayTeamId = matchData.awayTeam?.id ?? '';
  const matchWentToOvertime = matchData.currentMatch.wentToOvertime;
  const matchWentToShootout = matchData.currentMatch.wentToShootout;
  const { loadMatchEvents } = matchEvents;

  const handleRecordSave = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    if (!match.id) return;
    try {
      setSaveLoading(true);
      const payload: RecordSaveEventRequest = {
        goalieId,
        matchId: match.id,
        teamId: team === 'home' ? homeTeamId : awayTeamId,
        playerId: goalieId,
        periodNumber: timer.localClock.period,
        timeInSeconds: timer.currentTimerElapsedTime,
        wasInOvertime: matchWentToOvertime || timer.localClock.period > 3,
        wasInShootout: matchWentToShootout || timer.localClock.period > 4
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
  const { setShowOvertimeConfirmation, setShowShootoutConfirmation } = periodManagement;

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
          
          for (let i = 1; i < currentPeriod; i++) {
            startedPeriods.add(i);
            endedPeriods.add(i);
          }
          
          startedPeriods.add(currentPeriod);
          
          const nextPeriod = currentPeriod + 1;
          
          periodManagement.setStartedPeriods(startedPeriods);
          periodManagement.setEndedPeriods(endedPeriods);
          periodManagement.setNextPeriodToStart(nextPeriod <= 5 ? nextPeriod : 0);

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

  // Keybinds enabled when match live, and no forms are open
  const keybindsEnabled = matchData.currentMatch.status === 'InProgress' &&
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
    setGetCurrentTimeFromTimer(() => getTime);
  }, [setGetCurrentTimeFromTimer]);

  // MEMOIZED: Get toggle function handler
  const handleGetToggleFunction = useCallback((toggleFunction: () => Promise<void>) => {
    setGetToggleFromTimer(() => toggleFunction);
  }, [setGetToggleFromTimer]);

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
    matchData.setError(null);
  }, [matchData]);


  return (
    <>
      {/* Header */}
      <LiveMatchModalHeader
        homeTeam={matchData.homeTeam}
        awayTeam={matchData.awayTeam}
        currentMatch={matchData.currentMatch}
        onClose={() => navigate('/admin/floorball/matches')}
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
        isOpen={!!pendingGoalieChange}
        icon="🔄"
        title="Confirm Goalie Change"
        message={`Are you sure you want to change the ${pendingGoalieChange?.team === 'home' ? matchData.homeTeam?.name : matchData.awayTeam?.name} goalkeeper to ${pendingGoalieChange?.goalieName}?`}
        confirmText="Confirm Change"
        isLoading={matchData.loading}
        onConfirm={handleConfirmGoalieChange}
        onCancel={() => {
          // Revert the selection in the dropdown
          if (pendingGoalieChange?.team === 'home') {
            setHomeGoalieId(match.homeActiveGoalieId || '');
          } else {
            setAwayGoalieId(match.awayActiveGoalieId || '');
          }
          setPendingGoalieChange(null);
        }}
      />

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
        onConfirm={periodManagement.recordShootout}
        onCancel={() => setShowShootoutConfirmation(false)}
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
            onStartMatch={matchControls.handleStartMatch}
            onPeriodControlClick={handlePeriodControlClick}
            onTimerUpdate={handleTimerUpdate}
            onGetCurrentTime={handleGetCurrentTime}
            onGetToggleFunction={handleGetToggleFunction}
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
            onDeleteEvent={async (event) => {
              if (!event.eventId) {
                matchData.setError('Cannot delete: missing event id');
                return;
              }
              try {
                matchData.setError(null);
                console.log('Deleting event', { type: event.type, eventId: event.eventId, matchId: match.id });
                // Refresh match before delete to ensure scores/periods are up to date
                await matchData.loadCurrentMatchStatus();
                if (event.type === 'goal') {
                  await floorballMatchService.deleteGoal(match.id, event.eventId);
                } else if (event.type === 'penalty') {
                  await floorballMatchService.deletePenalty(match.id, event.eventId);
                } else if (event.type === 'save') {
                  await floorballMatchService.deleteSave(match.id, event.eventId);
                } else {
                  return;
                }
                await matchData.loadCurrentMatchStatus();
                await matchEvents.loadMatchEvents();
              } catch (err) {
                console.error('Failed to delete event', err);
                matchData.setError(err instanceof Error ? err.message : 'Failed to delete event');
              }
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
    return <div>Error: {error}</div>;
  }

  if (!match) {
    return <div>Match not found.</div>;
  }

  return (
    <div className="manage-match-page">
      <Navbar />
      <div className="manage-match-content">
        <div className="page-header">
          <div className="page-header__top">
            <BackButton to="/admin/floorball/matches" />
            <h1 className="page-title-compact font-title">MATCH MANAGEMENT</h1>
          </div>
        </div>
        <ManageMatchPageContent match={match} setMatch={setMatch} />
      </div>
    </div>
  );
};

export default ManageMatchPage;