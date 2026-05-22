import { useEffect, useMemo, useCallback, useState, useRef, type ReactElement } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { floorballMatchEventService, type RecordSaveEventRequest } from '../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { timerService } from '../../../../api/common/timerService';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

// Components
import LiveMatchModalHeader from './components/LiveMatchModalHeader';
import LiveMatchScoreboard from './components/LiveMatchScoreboard';
import LiveMatchTimer from './components/LiveMatchTimer';
import LiveMatchQuickActions from './components/LiveMatchQuickActions';
import GoalRecordingForm from './components/GoalRecordingForm';
import PenaltyRecordingForm from './components/PenaltyRecordingForm';
import LiveMatchEventsHistory from './components/LiveMatchEventsHistory';
import ActiveRosterCard from './components/ActiveRosterCard';
import EditActiveRosterDialog from './components/EditActiveRosterDialog';
import OfficialsSelectorSection from './components/OfficialsSelectorSection';
import MatchConfirmationDialogs from './components/MatchConfirmationDialogs';
import BulkSaveDialog, { type BulkSavePayload } from './components/BulkSaveDialog';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import type { EventGroup } from './components/types';
import { floorballRefereeService } from '../../../../api/floorball/floorballRefereeService';

// Context
import { MatchTimerProvider, useMatchTimerContext } from './context';

// Hooks
import {
  useMatchData,
  useSignalR,
  usePeriodManagement,
  useMatchEvents,
  useFormState,
  useMatchControls,
} from './hooks';

import './ManageMatchPage.scss';

interface ManageMatchPageContentProps {
  match: FloorballMatchDto;
  setMatch: (match: FloorballMatchDto) => void;
  /**
   * Where the Close button should navigate. Resolved by the parent so the page can return to
   * the originating tournament edit view (via the `returnTo` query parameter) instead of always
   * dropping the user on the global match list.
   */
  onClose: () => void;
}

/**
 * Main content component that uses the timer context
 */
const ManageMatchPageContent = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => {
  const timerContext = useMatchTimerContext();
  const lastSaveRef = useRef<Record<string, number>>({});
  
  // Goalie and official state
  const [homeGoalieId, setHomeGoalieId] = useState<string>(match.homeActiveGoalieId || '');
  const [awayGoalieId, setAwayGoalieId] = useState<string>(match.awayActiveGoalieId || '');
  const [selectedOfficials, setSelectedOfficials] = useState<string[]>(match.officials || []);
  const [officialOptions, setOfficialOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [officialsSaving, setOfficialsSaving] = useState(false);
  
  // UI state
  const [showEndMatchConfirmation, setShowEndMatchConfirmation] = useState(false);
  const [showReopenConfirmation, setShowReopenConfirmation] = useState(false);
  const [saveLoading, setSaveLoading] = useState(false);
  // Holds the group the user picked for deletion (always non-empty when set). For most events
  // the group has a single underlying event; bulk-recorded saves expand into N entries and
  // the delete handler removes every one of them in sequence.
  const [eventToDelete, setEventToDelete] = useState<EventGroup | null>(null);
  const [deleteEventLoading, setDeleteEventLoading] = useState(false);
  const [shouldStartTimer, setShouldStartTimer] = useState(false);
  const [isSidesSwapped, setIsSidesSwapped] = useState(false);
  const [isLineupDialogOpen, setIsLineupDialogOpen] = useState(false);
  // Bulk save dialog state. `null` means "closed"; an object means the dialog is open for the
  // captured side + goalie. Using a single state object instead of separate `isOpen` + `side`
  // flags avoids inconsistent transitions when the user reopens the dialog after a submit.
  const [bulkSaveTarget, setBulkSaveTarget] = useState<{ team: 'home' | 'away'; goalieId: string } | null>(null);
  const [bulkSaveLoading, setBulkSaveLoading] = useState(false);
  const [bulkSaveError, setBulkSaveError] = useState<string | null>(null);

  // Sync goalie state with match prop
  useEffect(() => {
    setHomeGoalieId(match.homeActiveGoalieId || '');
    setAwayGoalieId(match.awayActiveGoalieId || '');
    setSelectedOfficials(match.officials || []);
  }, [match.homeActiveGoalieId, match.awayActiveGoalieId, match.officials]);

  // Custom hooks
  const matchData = useMatchData({
    match,
    onMatchUpdated: setMatch,
    onStateUpdate: () => {},
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
    onReopen: (_matchId: string, updatedMatch?: FloorballMatchDto) => {
      if (updatedMatch) setMatch(updatedMatch);
    },
  });

  const matchEvents = useMatchEvents({
    match,
    currentMatch: matchData.currentMatch,
    homeTeam: matchData.homeTeam,
    awayTeam: matchData.awayTeam,
    getPlayerNameById: matchData.getPlayerNameById,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus,
  });

  const periodManagement = usePeriodManagement({
    currentMatch: matchData.currentMatch,
    currentPeriod: timerContext.currentPeriod,
    setCurrentPeriod: timerContext.setCurrentPeriod,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus,
  });
  const {
    startedPeriods,
    endedPeriods,
    nextPeriodToStart,
    setStartedPeriods,
    setEndedPeriods,
    setNextPeriodToStart
  } = periodManagement;

  const forms = useFormState({
    currentMatch: matchData.currentMatch,
    clock: { period: timerContext.currentPeriod, minutes: 0, seconds: 0, isRunning: timerContext.isRunning },
    currentTimerElapsedTime: timerContext.elapsedTimeSeconds,
    getCurrentElapsedSeconds: timerContext.callbacks.getCurrentElapsedSeconds,
    loadMatchEvents: matchEvents.loadMatchEvents,
    loadCurrentMatchStatus: matchData.loadCurrentMatchStatus,
    setError: matchData.setError,
  });

  const signalR = useSignalR({
    matchId: match?.id,
    onPeriodStarted: periodManagement.handlePeriodStarted,
    onGoalScored: matchEvents.handleGoalScored,
    onPenaltyAssigned: matchEvents.handlePenaltyAssigned,
    onSaveRecorded: matchEvents.handleSaveRecorded,
  });

  // Derived values
  const homeTeamId = matchData.homeTeam?.id ?? '';
  const awayTeamId = matchData.awayTeam?.id ?? '';
  const matchWentToOvertime = matchData.currentMatch.wentToOvertime;
  const matchWentToShootout = matchData.currentMatch.wentToShootout;
  const toggleTimer = timerContext.callbacks.toggle;
  const timerCurrentPeriod = timerContext.currentPeriod;
  const setTimerCurrentPeriod = timerContext.setCurrentPeriod;

  const currentScore = useMemo(() => ({
    home: match?.homeScore ?? 0,
    away: match?.awayScore ?? 0,
  }), [match]);

  const leftSideTeam: 'home' | 'away' = isSidesSwapped ? 'away' : 'home';
  const rightSideTeam: 'home' | 'away' = isSidesSwapped ? 'home' : 'away';

  const leftSideTeamData = leftSideTeam === 'home' ? matchData.homeTeam : matchData.awayTeam;
  const rightSideTeamData = rightSideTeam === 'home' ? matchData.homeTeam : matchData.awayTeam;
  const leftSideScore = leftSideTeam === 'home' ? currentScore.home : currentScore.away;
  const rightSideScore = rightSideTeam === 'home' ? currentScore.home : currentScore.away;
  const leftSideTeamId = leftSideTeam === 'home' ? homeTeamId : awayTeamId;
  const rightSideTeamId = rightSideTeam === 'home' ? homeTeamId : awayTeamId;
  const leftSidePlayers = leftSideTeam === 'home' ? matchData.homePlayers : matchData.awayPlayers;
  const rightSidePlayers = rightSideTeam === 'home' ? matchData.homePlayers : matchData.awayPlayers;

  const leftSideGoalieId = leftSideTeam === 'home' ? homeGoalieId : awayGoalieId;
  const rightSideGoalieId = rightSideTeam === 'home' ? homeGoalieId : awayGoalieId;

  const isPeriodActive = periodManagement.startedPeriods.has(timerContext.currentPeriod) &&
    !periodManagement.endedPeriods.has(timerContext.currentPeriod);

  const keybindsEnabled = matchData.currentMatch.status === 'InProgress' &&
    isPeriodActive &&
    !forms.showGoalForm &&
    !forms.showPenaltyForm;

  const areNumberSetsEqual = useCallback((a: Set<number>, b: Set<number>) => {
    if (a === b) return true;
    if (a.size !== b.size) return false;
    for (const value of a) {
      if (!b.has(value)) return false;
    }
    return true;
  }, []);

  const startedPeriodsRef = useRef<Set<number>>(startedPeriods);
  const endedPeriodsRef = useRef<Set<number>>(endedPeriods);
  const nextPeriodToStartRef = useRef<number>(nextPeriodToStart);
  const timerCurrentPeriodRef = useRef<number>(timerCurrentPeriod);

  useEffect(() => { startedPeriodsRef.current = startedPeriods; }, [startedPeriods]);
  useEffect(() => { endedPeriodsRef.current = endedPeriods; }, [endedPeriods]);
  useEffect(() => { nextPeriodToStartRef.current = nextPeriodToStart; }, [nextPeriodToStart]);
  useEffect(() => { timerCurrentPeriodRef.current = timerCurrentPeriod; }, [timerCurrentPeriod]);

  // Load officials - only runs once when match.id is available
  useEffect(() => {
    let isCancelled = false;
    const loadOfficials = async () => {
      if (!match.id) return;
      try {
        const response = await floorballRefereeService.getAll({ pageSize: 50 });
        if (!isCancelled && response.success && response.data) {
          const mapped = response.data.map(ref => ({ id: ref.id, name: ref.person.fullName }));
          const sorted = [...mapped].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
          const guestIndex = sorted.findIndex(option => option.name.toUpperCase() === 'GUEST REFEREE');
          if (guestIndex > 0) {
            const guest = sorted[guestIndex];
            sorted.splice(guestIndex, 1);
            sorted.unshift(guest);
          }
          setOfficialOptions(sorted);
        }
      } catch (error) {
        if (!isCancelled) {
          console.error('Failed to load officials:', error);
        }
      }
    };
    loadOfficials();
    return () => { isCancelled = true; };
  }, [match.id]);

  // Initialize period state
  useEffect(() => {
    const initializePeriodState = async () => {
      try {
        if (matchData.currentMatch.status === 'InProgress') {
          const timerStatus = await timerService.getTimerStatus(match.id);
          const currentPeriod = timerStatus.exists && timerStatus.periodNumber ? timerStatus.periodNumber : 1;
          
          const desiredStartedPeriods = new Set<number>();
          const desiredEndedPeriods = new Set<number>();
          const periodScores = matchData.currentMatch.periodScores || {};
          
          Object.entries(periodScores).forEach(([periodNum, scoreData]) => {
            const period = parseInt(periodNum);
            if (scoreData.isCompleted) {
              desiredStartedPeriods.add(period);
              desiredEndedPeriods.add(period);
            } else if (period === currentPeriod) {
              desiredStartedPeriods.add(period);
          }
          });
          
          desiredStartedPeriods.add(currentPeriod);
          
          const maxPeriod = periodManagement.maxPeriodNumber;
          let nextPeriod = 1;
          for (let i = 1; i <= maxPeriod; i++) {
            if (!desiredStartedPeriods.has(i)) {
              nextPeriod = i;
              break;
            }
          }
          if (nextPeriod > maxPeriod || desiredStartedPeriods.has(maxPeriod)) {
            nextPeriod = 0;
          }
          
          if (timerCurrentPeriodRef.current !== currentPeriod) {
            setTimerCurrentPeriod(currentPeriod);
          }
          if (!areNumberSetsEqual(startedPeriodsRef.current, desiredStartedPeriods)) {
            setStartedPeriods(desiredStartedPeriods);
          }
          if (!areNumberSetsEqual(endedPeriodsRef.current, desiredEndedPeriods)) {
            setEndedPeriods(desiredEndedPeriods);
          }
          if (nextPeriodToStartRef.current !== nextPeriod) {
            setNextPeriodToStart(nextPeriod);
          }
        } else {
          const desiredStartedPeriods = new Set<number>();
          const desiredEndedPeriods = new Set<number>();
          if (!areNumberSetsEqual(startedPeriodsRef.current, desiredStartedPeriods)) {
            setStartedPeriods(desiredStartedPeriods);
          }
          if (!areNumberSetsEqual(endedPeriodsRef.current, desiredEndedPeriods)) {
            setEndedPeriods(desiredEndedPeriods);
          }
          if (nextPeriodToStartRef.current !== 1) {
            setNextPeriodToStart(1);
          }
        }
      } catch (error) {
        console.warn('Failed to initialize period state:', error);
        if (matchData.currentMatch.status === 'InProgress') {
          const desiredStartedPeriods = new Set<number>([1]);
          if (!areNumberSetsEqual(startedPeriodsRef.current, desiredStartedPeriods)) {
            setStartedPeriods(desiredStartedPeriods);
          }
          if (nextPeriodToStartRef.current !== 2) {
            setNextPeriodToStart(2);
          }
        }
      }
    };
    initializePeriodState();
  }, [
    match.id,
    matchData.currentMatch.status,
    matchData.currentMatch.periodScores,
    periodManagement.maxPeriodNumber,
    setTimerCurrentPeriod,
    setStartedPeriods,
    setEndedPeriods,
    setNextPeriodToStart,
    areNumberSetsEqual
  ]);

  // Load initial data
  useEffect(() => {
    matchData.loadTeamData();
    matchEvents.loadMatchEvents();
    matchData.loadCurrentMatchStatus();
    signalR.setupSignalR();
    return () => { signalR.cleanupSignalR(); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Update match state when prop changes
  useEffect(() => {
    if (match && (match.id !== matchData.currentMatch.id || match.status !== matchData.currentMatch.status)) {
      matchData.setCurrentMatch(match);
    }
  }, [match, matchData]);

  // Handlers
  const handleRecordSave = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    if (!match.id) return;

    const key = `${match.id}:${team}:${goalieId}`;
    const now = Date.now();
    if (lastSaveRef.current[key] && now - lastSaveRef.current[key] < 250) return;
    lastSaveRef.current[key] = now;

    // Get the LIVE elapsed time from the timer callback, not the stale context state
    const currentElapsedSeconds = timerContext.callbacks.getCurrentElapsedSeconds
      ? timerContext.callbacks.getCurrentElapsedSeconds()
      : timerContext.elapsedTimeSeconds;

    try {
      setSaveLoading(true);
      const payload: RecordSaveEventRequest = {
        goalieId,
        matchId: match.id,
        teamId: team === 'home' ? homeTeamId : awayTeamId,
        playerId: goalieId,
        periodNumber: timerContext.currentPeriod,
        timeInSeconds: currentElapsedSeconds,
        wasInOvertime: matchWentToOvertime || timerContext.currentPeriod > 2,
        wasInShootout: matchWentToShootout || timerContext.currentPeriod > 3,
      };
      await floorballMatchEventService.recordSave(payload);
      await matchEvents.loadMatchEvents();
      matchData.setError(null);
    } catch (error) {
      matchData.setError(error instanceof Error ? error.message : 'Failed to record save');
    } finally {
      setSaveLoading(false);
    }
  }, [match.id, homeTeamId, awayTeamId, matchWentToOvertime, matchWentToShootout, timerContext.currentPeriod, timerContext.elapsedTimeSeconds, timerContext.callbacks, matchEvents, matchData]);

  const handleOpenBulkSave = useCallback((team: 'home' | 'away', goalieId: string) => {
    if (!goalieId) return;
    setBulkSaveError(null);
    setBulkSaveTarget({ team, goalieId });
  }, []);

  const handleCloseBulkSave = useCallback(() => {
    if (bulkSaveLoading) return;
    setBulkSaveTarget(null);
    setBulkSaveError(null);
  }, [bulkSaveLoading]);

  const handleSubmitBulkSave = useCallback(async (payload: BulkSavePayload) => {
    if (!bulkSaveTarget || !match.id) return;
    const { team, goalieId } = bulkSaveTarget;
    const teamId: string = team === 'home' ? homeTeamId : awayTeamId;
    if (!teamId) {
      setBulkSaveError('Cannot determine team — try reopening the match.');
      return;
    }

    // The backend accepts a `count` field on the record-save request and writes all events
    // inside a single transaction (and skips the per-(match, goalie) rate limit while it's
    // doing so). This avoids both the 250ms rate limit that broke the previous client-side
    // loop and the partial-failure window where some saves landed and some didn't.
    setBulkSaveLoading(true);
    setBulkSaveError(null);
    try {
      const request: RecordSaveEventRequest = {
        goalieId,
        matchId: match.id,
        teamId,
        playerId: goalieId,
        periodNumber: payload.periodNumber,
        timeInSeconds: payload.timeInSeconds,
        wasInOvertime: matchWentToOvertime || payload.periodNumber > 2,
        wasInShootout: matchWentToShootout || payload.periodNumber > 3,
        count: payload.count,
      };
      await floorballMatchEventService.recordSave(request);
      await matchEvents.loadMatchEvents();
      matchData.setError(null);
      setBulkSaveTarget(null);
    } catch (error) {
      const baseMessage: string = error instanceof Error ? error.message : 'Failed to record saves';
      setBulkSaveError(baseMessage);
      // Refresh the events list so any saves the backend committed before the failure are
      // still reflected in the UI immediately.
      await matchEvents.loadMatchEvents();
    } finally {
      setBulkSaveLoading(false);
    }
  }, [bulkSaveTarget, match.id, homeTeamId, awayTeamId, matchWentToOvertime, matchWentToShootout, matchEvents, matchData]);

  // Keyboard shortcuts
  useEffect(() => {
    if (!keybindsEnabled) return;
    
    const handler = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement;
      if (['INPUT', 'TEXTAREA'].includes(target?.tagName) || target?.isContentEditable) return;
      
      const key = e.key.toLowerCase();
      if (key === 'q' && leftSideGoalieId) {
        handleRecordSave(leftSideTeam, leftSideGoalieId);
        e.preventDefault();
      }
      if (key === 'r' && rightSideGoalieId) {
        handleRecordSave(rightSideTeam, rightSideGoalieId);
        e.preventDefault();
      }
      if (key === ' ' && toggleTimer) {
        toggleTimer();
        e.preventDefault();
      }
    };
    
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [keybindsEnabled, leftSideGoalieId, rightSideGoalieId, leftSideTeam, rightSideTeam, toggleTimer, handleRecordSave]);

  // Trigger timer start after match starts
  useEffect(() => {
    if (shouldStartTimer && toggleTimer) {
      toggleTimer();
      setShouldStartTimer(false);
    }
  }, [shouldStartTimer, toggleTimer]);

  const handleStartMatchAndTimer = useCallback(async () => {
    await matchControls.handleStartMatch();
    setShouldStartTimer(true);
  }, [matchControls]);

  const handlePeriodControlClick = useCallback(() => {
    if (periodManagement.canEndPeriod()) {
      if (periodManagement.isInShootout() || timerContext.currentPeriod === periodManagement.maxPeriodNumber) {
        setShowEndMatchConfirmation(true);
        return;
      }
      
      // Get live elapsed time from callback, not stale context state
      const currentElapsedSeconds = timerContext.callbacks.getCurrentElapsedSeconds
        ? timerContext.callbacks.getCurrentElapsedSeconds()
        : timerContext.elapsedTimeSeconds;
      
      const isUnder15Minutes = currentElapsedSeconds < 900;
      if (isUnder15Minutes) {
        periodManagement.setShowEndPeriodConfirmation(true);
      } else {
        (async () => {
          await periodManagement.endPeriod();
          if (timerContext.callbacks.reset) timerContext.callbacks.reset();
        })();
      }
    } else {
      (async () => {
        await periodManagement.startPeriod();
        if (periodManagement.nextPeriodToStart !== 4) {
          if (timerContext.callbacks.start) {
            await timerContext.callbacks.start();
          } else if (timerContext.callbacks.toggle) {
            await timerContext.callbacks.toggle();
          }
        }
      })();
    }
  }, [periodManagement, timerContext]);

  const handleDeleteEvent = useCallback(async () => {
    if (!eventToDelete || eventToDelete.events.length === 0) {
      setEventToDelete(null);
      return;
    }
    // Require every underlying event to carry an id. We don't want to partially delete a
    // bulk-save group and leave orphan rows behind, so abort the whole batch if anything
    // is malformed.
    const eventsToDelete = eventToDelete.events.filter(e => !!e.eventId);
    if (eventsToDelete.length !== eventToDelete.events.length || eventsToDelete.length === 0) {
      matchData.setError('Cannot delete: missing event id');
      setEventToDelete(null);
      return;
    }

    try {
      setDeleteEventLoading(true);
      matchData.setError(null);
      await matchData.loadCurrentMatchStatus();

      for (const evt of eventsToDelete) {
        const eventId: string = evt.eventId as string;
        if (evt.type === 'goal') {
          await floorballMatchService.deleteGoal(match.id, eventId);
        } else if (evt.type === 'penalty') {
          await floorballMatchService.deletePenalty(match.id, eventId);
        } else if (evt.type === 'save') {
          await floorballMatchService.deleteSave(match.id, eventId);
        }
      }

      await matchData.loadCurrentMatchStatus();
      await matchEvents.loadMatchEvents();
      setEventToDelete(null);
    } catch (err) {
      matchData.setError(err instanceof Error ? err.message : 'Failed to delete event');
      setEventToDelete(null);
    } finally {
      setDeleteEventLoading(false);
    }
  }, [eventToDelete, match.id, matchData, matchEvents]);

  const handleOfficialSelect = useCallback(async (index: number, refereeId: string) => {
    if (!match.id || !refereeId) return;
    
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
      
      const resp = wasEmpty
        ? await floorballMatchService.addOfficial(match.id, refereeId)
        : await floorballMatchService.updateOfficials(match.id, next);
        
      if (resp.success && resp.data) {
        setSelectedOfficials(resp.data.officials);
        matchData.setCurrentMatch(resp.data);
        setMatch(resp.data);
      }
    } catch (error) {
      matchData.setError(error instanceof Error ? error.message : 'Failed to set official');
    } finally {
      setOfficialsSaving(false);
    }
  }, [match.id, selectedOfficials, matchData, setMatch]);

  const handleOfficialRemove = useCallback(async (index: number, refereeId: string) => {
    if (!match.id) return;
    
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
        matchData.setCurrentMatch(resp.data);
        setMatch(resp.data);
      }
    } catch (error) {
      matchData.setError(error instanceof Error ? error.message : 'Failed to remove official');
    } finally {
      setOfficialsSaving(false);
    }
  }, [match.id, matchData, setMatch]);

  // Format time helpers
  const currentTimeFormatted = timerContext.formatTime(
    Math.floor(timerContext.elapsedTimeSeconds / 60),
    timerContext.elapsedTimeSeconds % 60
  );

  return (
    <>
      <LiveMatchModalHeader
        homeTeam={matchData.homeTeam}
        awayTeam={matchData.awayTeam}
        currentMatch={matchData.currentMatch}
        isSidesSwapped={isSidesSwapped}
        onToggleSides={() => setIsSidesSwapped(prev => !prev)}
        onClose={onClose}
        onCompleteLive={() => setShowEndMatchConfirmation(true)}
        onReopen={() => setShowReopenConfirmation(true)}
      />

      <ErrorPopup message={matchData.error} />

      <MatchConfirmationDialogs
        showEndPeriodConfirmation={periodManagement.showEndPeriodConfirmation}
        currentPeriod={timerContext.currentPeriod}
        currentTimeFormatted={currentTimeFormatted}
        periodLoading={periodManagement.periodLoading}
        onEndPeriodConfirm={async () => {
          await periodManagement.endPeriod();
          if (timerContext.callbacks.reset) timerContext.callbacks.reset();
          periodManagement.setShowEndPeriodConfirmation(false);
        }}
        onEndPeriodCancel={periodManagement.cancelEndPeriod}
        
        showOvertimeConfirmation={periodManagement.showOvertimeConfirmation}
        onOvertimeConfirm={async () => {
          await periodManagement.recordOvertime();
          if (timerContext.callbacks.start) {
            await timerContext.callbacks.start();
          } else if (timerContext.callbacks.toggle) {
            await timerContext.callbacks.toggle();
          }
        }}
        onOvertimeCancel={() => periodManagement.setShowOvertimeConfirmation(false)}
        
        showShootoutConfirmation={periodManagement.showShootoutConfirmation}
        onShootoutConfirm={async () => {
          await periodManagement.recordShootout();
          if (timerContext.callbacks.start) {
            await timerContext.callbacks.start();
          } else if (timerContext.callbacks.toggle) {
            await timerContext.callbacks.toggle();
          }
        }}
        onShootoutCancel={() => periodManagement.setShowShootoutConfirmation(false)}
        
        showEndMatchConfirmation={showEndMatchConfirmation}
        isShootout={periodManagement.isInShootout()}
        onEndMatchConfirm={async () => {
          await matchControls.handleCompleteLive();
          setShowEndMatchConfirmation(false);
        }}
        onEndMatchCancel={() => setShowEndMatchConfirmation(false)}

        showReopenConfirmation={showReopenConfirmation}
        onReopenConfirm={async () => {
          await matchControls.handleReopenMatch();
          setShowReopenConfirmation(false);
        }}
        onReopenCancel={() => setShowReopenConfirmation(false)}

        eventToDelete={eventToDelete}
        deleteEventLoading={deleteEventLoading}
        onDeleteEventConfirm={handleDeleteEvent}
        onDeleteEventCancel={() => setEventToDelete(null)}
        
        matchLoading={matchData.loading}
      />

      <div className="modal-content">
        <div className="left-section">
          <LiveMatchTimer
            currentMatch={matchData.currentMatch}
            isOpen={true}
            loading={matchData.loading}
            startedPeriods={periodManagement.startedPeriods}
            endedPeriods={periodManagement.endedPeriods}
            nextPeriodToStart={periodManagement.nextPeriodToStart}
            periodLoading={periodManagement.periodLoading}
            onStartMatch={handleStartMatchAndTimer}
            onPeriodControlClick={handlePeriodControlClick}
            canEndPeriod={periodManagement.canEndPeriod}
            getPeriodControlButtonText={periodManagement.getPeriodControlButtonText}
            keybindsEnabled={keybindsEnabled}
            isStartMatchDisabled={!homeGoalieId || !awayGoalieId}
            overtimePeriodNumber={periodManagement.overtimePeriodNumber}
            shootoutPeriodNumber={periodManagement.shootoutPeriodNumber}
          />

          <LiveMatchQuickActions
            loading={forms.loading}
            currentMatch={matchData.currentMatch}
            leftTeamId={leftSideTeamId}
            rightTeamId={rightSideTeamId}
            leftTeamName={leftSideTeamData?.name}
            rightTeamName={rightSideTeamData?.name}
            leftTeamSide={leftSideTeam}
            rightTeamSide={rightSideTeam}
            onShowGoalForm={forms.openGoalFormForTeam}
            onShowPenaltyForm={forms.openPenaltyFormForTeam}
            leftGoalieId={leftSideGoalieId}
            rightGoalieId={rightSideGoalieId}
            onRecordSave={handleRecordSave}
            onShowBulkSave={handleOpenBulkSave}
            keybindsEnabled={keybindsEnabled}
            saveLoading={saveLoading}
          />

          <ActiveRosterCard
            leftTeamName={leftSideTeamData?.name}
            rightTeamName={rightSideTeamData?.name}
            leftPlayers={leftSidePlayers}
            rightPlayers={rightSidePlayers}
            leftLineup={leftSideTeam === 'home' ? matchData.currentMatch.homeActivePlayers : matchData.currentMatch.awayActivePlayers}
            rightLineup={rightSideTeam === 'home' ? matchData.currentMatch.homeActivePlayers : matchData.currentMatch.awayActivePlayers}
            leftGoalieId={leftSideGoalieId}
            rightGoalieId={rightSideGoalieId}
            onEditLineup={() => setIsLineupDialogOpen(true)}
            disabled={matchData.currentMatch.status === 'Completed' || matchData.currentMatch.status === 'Cancelled'}
          />

          <EditActiveRosterDialog
            isOpen={isLineupDialogOpen}
            matchId={matchData.currentMatch.id}
            homeTeamId={homeTeamId}
            awayTeamId={awayTeamId}
            homeTeamName={matchData.homeTeam?.name ?? ''}
            awayTeamName={matchData.awayTeam?.name ?? ''}
            homePlayers={matchData.homePlayers}
            awayPlayers={matchData.awayPlayers}
            initialHomeLineup={matchData.currentMatch.homeActivePlayers ?? []}
            initialAwayLineup={matchData.currentMatch.awayActivePlayers ?? []}
            initialHomeGoalieId={homeGoalieId}
            initialAwayGoalieId={awayGoalieId}
            onClose={() => setIsLineupDialogOpen(false)}
            onSaved={(updated) => {
              matchData.setCurrentMatch(updated);
              setMatch(updated);
              setHomeGoalieId(updated.homeActiveGoalieId ?? '');
              setAwayGoalieId(updated.awayActiveGoalieId ?? '');
            }}
            onError={matchData.setError}
          />

          <OfficialsSelectorSection
            selectedOfficials={selectedOfficials}
            options={officialOptions}
            saving={officialsSaving}
            onAddRow={() => setSelectedOfficials(prev => [...prev, ''])}
            onSelect={handleOfficialSelect}
            onRemove={handleOfficialRemove}
            disabled={matchData.currentMatch.status === 'Completed' || matchData.currentMatch.status === 'Cancelled'}
          />

          <GoalRecordingForm
            showGoalForm={forms.showGoalForm}
            goalForm={forms.goalForm}
            setGoalForm={forms.setGoalForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            clock={{ period: timerContext.currentPeriod, minutes: 0, seconds: 0, isRunning: timerContext.isRunning }}
            loading={forms.loading}
            getPlayersForTeam={matchData.getPlayersForTeam}
            onRecordGoal={forms.recordGoal}
            onClose={() => forms.setShowGoalForm(false)}
          />

          <PenaltyRecordingForm
            showPenaltyForm={forms.showPenaltyForm}
            penaltyForm={forms.penaltyForm}
            setPenaltyForm={forms.setPenaltyForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            clock={{ period: timerContext.currentPeriod, minutes: 0, seconds: 0, isRunning: timerContext.isRunning }}
            loading={forms.loading}
            getPlayersForTeam={matchData.getPlayersForTeam}
            onRecordPenalty={forms.recordPenalty}
            onClose={() => forms.setShowPenaltyForm(false)}
          />
        </div>
        
        <div className="right-section">
          <LiveMatchScoreboard
            leftTeam={leftSideTeamData}
            rightTeam={rightSideTeamData}
            leftScore={leftSideScore}
            rightScore={rightSideScore}
          />

          <LiveMatchEventsHistory
            allEvents={matchEvents.allEvents}
            onDeleteEvent={(group) => {
              // A group always carries at least one event from the child component, so an
              // empty list here would indicate a programming error. Reject it loudly via
              // the visible error popup instead of silently opening an empty dialog.
              if (group.events.length === 0 || !group.representative.eventId) {
                matchData.setError('Cannot delete: missing event id');
                return;
              }
              setEventToDelete(group);
            }}
            /* Once the match is Completed/Cancelled the backend blocks event deletion */
            /* (the only legitimate edit path is to reopen the match first), so hide   */
            /* the per-row delete affordance to match the user's mental model.         */
            canDelete={matchData.currentMatch.status !== 'Completed' && matchData.currentMatch.status !== 'Cancelled'}
          />
        </div>
      </div>

      {bulkSaveTarget && (
        <BulkSaveDialog
          isOpen={true}
          goalieName={matchData.getPlayerNameById(bulkSaveTarget.goalieId)}
          teamName={
            (bulkSaveTarget.team === 'home' ? matchData.homeTeam?.name : matchData.awayTeam?.name) ?? ''
          }
          currentPeriod={timerContext.currentPeriod}
          numberOfPeriods={matchData.currentMatch.matchRules?.numberOfPeriods ?? 3}
          periodDurationMinutes={matchData.currentMatch.matchRules?.periodDurationMinutes ?? 20}
          currentElapsedSeconds={
            timerContext.callbacks.getCurrentElapsedSeconds
              ? timerContext.callbacks.getCurrentElapsedSeconds()
              : timerContext.elapsedTimeSeconds
          }
          onSubmit={handleSubmitBulkSave}
          onClose={handleCloseBulkSave}
          loading={bulkSaveLoading}
          errorMessage={bulkSaveError}
        />
      )}
    </>
  );
};

/**
 * Wrapper component that provides the timer context
 */
const ManageMatchPageWithContext = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => {
  return (
    <MatchTimerProvider initialPeriod={1}>
      <ManageMatchPageContent match={match} setMatch={setMatch} onClose={onClose} />
    </MatchTimerProvider>
  );
};

/**
 * Default landing page used when the user opens the match management view directly (no
 * originating view to return to). Kept as a single source of truth so the Close button and
 * any future "back" affordances stay in sync.
 */
const DEFAULT_RETURN_PATH = '/admin/floorball/matches';

/**
 * Whitelists the `returnTo` query parameter to internal absolute paths only. This prevents
 * an open-redirect via a crafted URL and also guards against accidental protocol-relative
 * paths (e.g. `//evil.example`).
 */
const sanitizeReturnTo = (raw: string | null): string => {
  if (!raw) return DEFAULT_RETURN_PATH;
  // Reject anything that isn't a same-origin absolute path. Allow `/foo` but not `//foo`,
  // `http://...`, `javascript:...`, etc.
  if (!raw.startsWith('/') || raw.startsWith('//')) return DEFAULT_RETURN_PATH;
  return raw;
};

/**
 * Main page component with data loading
 */
const ManageMatchPage = (): ReactElement => {
  const { matchId } = useParams<{ matchId: string }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const returnTo: string = useMemo(
    () => sanitizeReturnTo(searchParams.get('returnTo')),
    [searchParams]
  );

  const handleClose = useCallback((): void => {
    navigate(returnTo);
  }, [navigate, returnTo]);

  const handleNavigateToEdit = useCallback((): void => {
    if (matchId) {
      navigate(`/admin/floorball/matches/${matchId}/edit`);
    }
  }, [matchId, navigate]);

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

  const isMatchFinished = match.status === 'Completed';

  return (
    <PageTemplate title="Manage match page">
    <div className="manage-match-page">
        <div className="page-header">
          <div className="page-header__top">
            <h1 className="page-title-compact font-title">MATCH MANAGEMENT</h1>
            <div className="page-header__actions">
              {/* "Edit match details" navigates to a separate form that mutates schedule / teams. */}
              {/* Hide it once the match is Finished: at that point the only sanctioned recovery */}
              {/* path is "Open match" in the header, which reverts season aggregates safely.    */}
              {!isMatchFinished && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={handleNavigateToEdit}
                  disabled={!matchId}
                  title={t('floorball.matches.actions.edit')}
                >
                  <span className="edit-match-button__icon" aria-hidden="true">✏️</span>
                  <span className="edit-match-button__label">{t('floorball.matches.actions.edit')}</span>
                </button>
              )}
            </div>
          </div>
        </div>
        <ManageMatchPageWithContext match={match} setMatch={setMatch} onClose={handleClose} />
    </div>
    </PageTemplate>
  );
};

export default ManageMatchPage;
