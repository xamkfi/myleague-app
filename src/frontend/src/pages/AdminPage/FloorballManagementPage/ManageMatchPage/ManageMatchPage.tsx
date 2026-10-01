import { useEffect, useMemo, useCallback, useState, useRef, type ReactElement } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { floorballMatchEventService, type RecordSaveEventRequest } from '../../../../api/floorball/floorballMatchEventService';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballRefereeService } from '../../../../api/floorball/floorballRefereeService';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import { floorballPeriodEventFlags, floorballPeriodStartSeconds } from '../../../../utils/floorballPeriod';
import { formatEventTimeMmSs } from '../../../../utils/matchEventFormat';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import AssignTeamsDialog from '../../../../components/AssignTeamsDialog/AssignTeamsDialog';

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
import ScorekeepersSection from '../../../../components/match/ScorekeepersSection';
import MatchConfirmationDialogs from './components/MatchConfirmationDialogs';
import BulkSaveDialog, { type BulkSavePayload } from './components/BulkSaveDialog';
import type { EventGroup, ProcessedEvent } from './components/types';

import { MatchTimerProvider, useMatchTimerContext } from './context';
import {
  useMatchData,
  useSignalR,
  usePeriodManagement,
  useMatchEvents,
  useFormState,
  useMatchControls,
} from './hooks';
import { describeMatchError } from './utils/describeMatchError';

import './ManageMatchPage.scss';

type TeamSide = 'home' | 'away';

interface OfficialOption {
  id: string;
  name: string;
}

interface BulkSaveTarget {
  team: TeamSide;
  goalieId: string;
}

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

/** Minimum gap between two quick-save keypresses for the same goalie. */
const SAVE_THROTTLE_MS: number = 250;

/**
 * Live desk for one match. Owns page-level UI state (dialogs, side swap, officials) and wires
 * the feature hooks together; the timer itself lives in {@link MatchTimerProvider}.
 */
const ManageMatchPageContent = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => {
  const { t } = useTranslation();
  const { currentPeriod, setCurrentPeriod, currentPeriodStartSeconds, setPeriodStartTime, timer } = useMatchTimerContext();
  const lastSaveRef = useRef<Record<string, number>>({});

  // Goalie and official state
  const [homeGoalieId, setHomeGoalieId] = useState<string>(match.homeActiveGoalieId || '');
  const [awayGoalieId, setAwayGoalieId] = useState<string>(match.awayActiveGoalieId || '');
  const [selectedOfficials, setSelectedOfficials] = useState<string[]>(match.officials || []);
  const [officialOptions, setOfficialOptions] = useState<OfficialOption[]>([]);
  const [officialsSaving, setOfficialsSaving] = useState<boolean>(false);
  const [scorekeepersSaving, setScorekeepersSaving] = useState<boolean>(false);

  // Dialog state
  const [showEndMatchConfirmation, setShowEndMatchConfirmation] = useState<boolean>(false);
  const [showReopenConfirmation, setShowReopenConfirmation] = useState<boolean>(false);
  /** Match clock captured when the end-period confirmation was opened. */
  const [endPeriodTimeSnapshot, setEndPeriodTimeSnapshot] = useState<string>('');
  const [saveLoading, setSaveLoading] = useState<boolean>(false);
  // Holds the groups the user picked for deletion (always non-empty when set). For a
  // single-row delete the array contains exactly one group; for multi-select bulk deletes
  // it carries every selected group, and the delete handler walks every underlying event
  // in sequence so a partial failure leaves the rest of the batch consistent.
  const [groupsToDelete, setGroupsToDelete] = useState<EventGroup[] | null>(null);
  const [deleteEventLoading, setDeleteEventLoading] = useState<boolean>(false);
  const [isLineupDialogOpen, setIsLineupDialogOpen] = useState<boolean>(false);
  // Bulk save dialog state. `null` means "closed"; an object means the dialog is open for the
  // captured side + goalie.
  const [bulkSaveTarget, setBulkSaveTarget] = useState<BulkSaveTarget | null>(null);
  const [bulkSaveLoading, setBulkSaveLoading] = useState<boolean>(false);
  const [bulkSaveError, setBulkSaveError] = useState<string | null>(null);

  // Persist the visual side swap per-match in localStorage so that leaving the page and
  // returning preserves the operator's chosen orientation.
  const sidesStorageKey: string = `manage-match-sides-swapped:${match.id}`;
  const [isSidesSwapped, setIsSidesSwapped] = useState<boolean>(() => {
    try {
      return localStorage.getItem(sidesStorageKey) === 'true';
    } catch {
      return false;
    }
  });
  useEffect(() => {
    try {
      localStorage.setItem(sidesStorageKey, String(isSidesSwapped));
    } catch {
      /* noop – localStorage may be unavailable (private mode, quota, etc.) */
    }
  }, [sidesStorageKey, isSidesSwapped]);

  // Sync goalie state with match prop
  useEffect(() => {
    setHomeGoalieId(match.homeActiveGoalieId || '');
    setAwayGoalieId(match.awayActiveGoalieId || '');
    setSelectedOfficials(match.officials || []);
  }, [match.homeActiveGoalieId, match.awayActiveGoalieId, match.officials]);

  // Feature hooks
  const matchData = useMatchData({ match, onMatchUpdated: setMatch });
  const { currentMatch, setCurrentMatch, setError, loadCurrentMatchStatus } = matchData;

  const matchControls = useMatchControls({
    currentMatch,
    setCurrentMatch,
    setError,
    setLoading: matchData.setLoading,
    onMatchChanged: setMatch,
  });

  const matchEvents = useMatchEvents({
    matchId: match.id,
    currentMatch,
    homeTeam: matchData.homeTeam,
    awayTeam: matchData.awayTeam,
    getPlayerNameById: matchData.getPlayerNameById,
    loadCurrentMatchStatus,
  });
  const { loadMatchEvents } = matchEvents;

  const periodManagement = usePeriodManagement({
    currentMatch,
    currentPeriod,
    setCurrentPeriod,
    timerPeriodNumber: timer.periodNumber,
    loadCurrentMatchStatus,
  });

  const forms = useFormState({
    currentMatch,
    currentPeriod,
    getCurrentElapsedSeconds: timer.getCurrentElapsedSeconds,
    loadMatchEvents,
    loadCurrentMatchStatus,
    setError,
  });

  const signalR = useSignalR({
    matchId: match.id,
    onPeriodStarted: periodManagement.handlePeriodStarted,
    onGoalScored: matchEvents.handleGoalScored,
    onPenaltyAssigned: matchEvents.handlePenaltyAssigned,
    onSaveRecorded: matchEvents.handleSaveRecorded,
  });

  // Derived values
  const homeTeamId: string = matchData.homeTeam?.id ?? '';
  const awayTeamId: string = matchData.awayTeam?.id ?? '';
  const matchRules = periodManagement.matchRules;
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';
  const isMatchClosed: boolean = currentMatch.status === 'Completed' || currentMatch.status === 'Cancelled';

  // A shootout only makes sense when regulation ended level.
  const showSkipToShootout: boolean =
    isMatchInProgress
    && (currentMatch.matchRules?.allowShootout ?? true)
    && (currentMatch.homeScore ?? 0) === (currentMatch.awayScore ?? 0)
    && !currentMatch.wentToOvertime
    && !currentMatch.wentToShootout
    && periodManagement.nextPeriodToStart === periodManagement.overtimePeriodNumber
    && periodManagement.endedPeriods.has(matchRules.numberOfPeriods);

  const leftSideTeam: TeamSide = isSidesSwapped ? 'away' : 'home';
  const rightSideTeam: TeamSide = isSidesSwapped ? 'home' : 'away';

  const leftSideTeamData = leftSideTeam === 'home' ? matchData.homeTeam : matchData.awayTeam;
  const rightSideTeamData = rightSideTeam === 'home' ? matchData.homeTeam : matchData.awayTeam;
  const leftSideScore: number = leftSideTeam === 'home' ? (match.homeScore ?? 0) : (match.awayScore ?? 0);
  const rightSideScore: number = rightSideTeam === 'home' ? (match.homeScore ?? 0) : (match.awayScore ?? 0);
  const leftSideTeamId: string = leftSideTeam === 'home' ? homeTeamId : awayTeamId;
  const rightSideTeamId: string = rightSideTeam === 'home' ? homeTeamId : awayTeamId;
  const leftSidePlayers = leftSideTeam === 'home' ? matchData.homePlayers : matchData.awayPlayers;
  const rightSidePlayers = rightSideTeam === 'home' ? matchData.homePlayers : matchData.awayPlayers;
  const leftSideGoalieId: string = leftSideTeam === 'home' ? homeGoalieId : awayGoalieId;
  const rightSideGoalieId: string = rightSideTeam === 'home' ? homeGoalieId : awayGoalieId;

  const isPeriodActive: boolean =
    periodManagement.startedPeriods.has(currentPeriod) && !periodManagement.endedPeriods.has(currentPeriod);

  // Keyboard shortcuts (Q/R/Space) must stay off while any dialog that collects input is open.
  const keybindsEnabled: boolean = isMatchInProgress
    && isPeriodActive
    && !forms.showGoalForm
    && !forms.showPenaltyForm
    && !isLineupDialogOpen
    && !bulkSaveTarget
    && !showEndMatchConfirmation
    && !showReopenConfirmation
    && !groupsToDelete
    && !periodManagement.showEndPeriodConfirmation;

  // Load officials once per match
  useEffect(() => {
    let isCancelled: boolean = false;
    const loadOfficials = async (): Promise<void> => {
      try {
        const response = await floorballRefereeService.getAll({ pageSize: 50 });
        if (isCancelled || !response.success || !response.data) return;
        const sorted: OfficialOption[] = response.data
          .map(ref => ({ id: ref.id, name: ref.person.fullName }))
          .sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
        const guestIndex: number = sorted.findIndex(option => option.name.toUpperCase() === 'GUEST REFEREE');
        if (guestIndex > 0) {
          const [guest] = sorted.splice(guestIndex, 1);
          sorted.unshift(guest);
        }
        setOfficialOptions(sorted);
      } catch (error) {
        if (!isCancelled) console.error('Failed to load officials:', error);
      }
    };
    void loadOfficials();
    return () => { isCancelled = true; };
  }, [match.id]);

  // Load initial data
  useEffect(() => {
    void matchData.loadTeamData();
    void loadMatchEvents();
    void loadCurrentMatchStatus();
    signalR.setupSignalR();
    return () => { signalR.cleanupSignalR(); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Keep the live view aligned with the match prop. Team reassignment does not change id or
  // status, and the initial load above runs only once, so without this the scoreboard and
  // rosters would keep the clubs that were on the page when it opened.
  useEffect(() => {
    const teamsChanged: boolean =
      (match.homeTeamId ?? null) !== (currentMatch.homeTeamId ?? null)
      || (match.awayTeamId ?? null) !== (currentMatch.awayTeamId ?? null);

    if (match.id !== currentMatch.id || match.status !== currentMatch.status || teamsChanged) {
      setCurrentMatch(match);
    }
    if (teamsChanged) {
      void matchData.loadTeamData();
    }
  }, [match, currentMatch, setCurrentMatch, matchData]);

  const reportError = useCallback((error: unknown, fallback: string): void => {
    setError(describeMatchError(error, fallback, t));
  }, [setError, t]);

  // ---------------------------------------------------------------------------
  // Saves
  // ---------------------------------------------------------------------------
  const handleRecordSave = useCallback(async (team: TeamSide, goalieId: string): Promise<void> => {
    const key: string = `${match.id}:${team}:${goalieId}`;
    const now: number = Date.now();
    if (lastSaveRef.current[key] && now - lastSaveRef.current[key] < SAVE_THROTTLE_MS) return;
    lastSaveRef.current[key] = now;

    const currentElapsedSeconds: number = timer.getCurrentElapsedSeconds();
    const periodFlags = floorballPeriodEventFlags(currentPeriod, matchRules.numberOfPeriods);
    const payload: RecordSaveEventRequest = {
      goalieId,
      matchId: match.id,
      teamId: team === 'home' ? homeTeamId : awayTeamId,
      playerId: goalieId,
      periodNumber: currentPeriod,
      timeInSeconds: currentElapsedSeconds,
      wasInOvertime: periodFlags.wasInOvertime,
      wasInShootout: periodFlags.wasInShootout,
    };

    setSaveLoading(true);
    try {
      await floorballMatchEventService.recordSave(payload);
      await loadMatchEvents();
      setError(null);
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.recordSave', 'Failed to record save'));
    } finally {
      setSaveLoading(false);
    }
  }, [match.id, homeTeamId, awayTeamId, currentPeriod, matchRules.numberOfPeriods, timer, loadMatchEvents, setError, reportError, t]);

  const handleOpenBulkSave = useCallback((team: TeamSide, goalieId: string): void => {
    if (!goalieId) return;
    setBulkSaveError(null);
    setBulkSaveTarget({ team, goalieId });
  }, []);

  const handleCloseBulkSave = useCallback((): void => {
    if (bulkSaveLoading) return;
    setBulkSaveTarget(null);
    setBulkSaveError(null);
  }, [bulkSaveLoading]);

  const handleSubmitBulkSave = useCallback(async (payload: BulkSavePayload): Promise<void> => {
    if (!bulkSaveTarget) return;
    const { team, goalieId } = bulkSaveTarget;
    const teamId: string = team === 'home' ? homeTeamId : awayTeamId;
    if (!teamId) {
      setBulkSaveError(t('floorball.matches.manage.errors.cannotDetermineTeam', 'Cannot determine team — try reopening the match.'));
      return;
    }

    // The backend accepts a `count` field and writes all saves inside a single transaction.
    setBulkSaveLoading(true);
    setBulkSaveError(null);
    try {
      const periodFlags = floorballPeriodEventFlags(payload.periodNumber, matchRules.numberOfPeriods);
      const request: RecordSaveEventRequest = {
        goalieId,
        matchId: match.id,
        teamId,
        playerId: goalieId,
        periodNumber: payload.periodNumber,
        timeInSeconds: payload.timeInSeconds,
        wasInOvertime: periodFlags.wasInOvertime,
        wasInShootout: periodFlags.wasInShootout,
        count: payload.count,
      };
      await floorballMatchEventService.recordSave(request);
      await loadMatchEvents();
      setError(null);
      setBulkSaveTarget(null);
    } catch (error) {
      setBulkSaveError(describeMatchError(error, t('floorball.matches.manage.errors.recordSaves', 'Failed to record saves'), t));
      // Refresh so any saves the backend committed before the failure are still shown.
      await loadMatchEvents();
    } finally {
      setBulkSaveLoading(false);
    }
  }, [bulkSaveTarget, match.id, homeTeamId, awayTeamId, matchRules.numberOfPeriods, loadMatchEvents, setError, t]);

  // ---------------------------------------------------------------------------
  // Keyboard shortcuts
  // ---------------------------------------------------------------------------
  useEffect(() => {
    if (!keybindsEnabled) return;

    const handler = (e: KeyboardEvent): void => {
      // Never react to key combinations (Ctrl+R reload, Cmd+Q quit, ...).
      if (e.ctrlKey || e.metaKey || e.altKey) return;

      const target = e.target as HTMLElement | null;
      if (target) {
        const tagName: string = target.tagName;
        if (tagName === 'INPUT' || tagName === 'TEXTAREA' || tagName === 'SELECT' || target.isContentEditable) {
          return;
        }
        // Second safety net: never fire while focus is inside any open dialog.
        if (
          typeof target.closest === 'function'
          && target.closest('[role="dialog"], dialog, .modal, .goal-record-modal, .penalty-record-modal, .bulk-save-modal, .eard-dialog')
        ) {
          return;
        }
      }

      const key: string = e.key.toLowerCase();
      if (key === 'q' && leftSideGoalieId) {
        void handleRecordSave(leftSideTeam, leftSideGoalieId);
        e.preventDefault();
      } else if (key === 'r' && rightSideGoalieId) {
        void handleRecordSave(rightSideTeam, rightSideGoalieId);
        e.preventDefault();
      } else if (key === ' ') {
        void timer.toggle();
        e.preventDefault();
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [keybindsEnabled, leftSideGoalieId, rightSideGoalieId, leftSideTeam, rightSideTeam, timer, handleRecordSave]);

  // ---------------------------------------------------------------------------
  // Match and period lifecycle
  // ---------------------------------------------------------------------------
  const handleStartMatchAndTimer = useCallback(async (): Promise<void> => {
    const started: boolean = await matchControls.handleStartMatch();
    if (!started) return;
    try {
      await timer.start(1);
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.startTimer', 'Match started but the clock could not be started'));
    }
  }, [matchControls, timer, reportError, t]);

  /** Ends the current period: freezes the clock first so no stale tick can restart it. */
  const finishCurrentPeriod = useCallback(async (): Promise<void> => {
    setError(null);
    try {
      await timer.stop();
      await periodManagement.endPeriod();
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.endPeriod', 'Failed to end period'));
    }
  }, [timer, periodManagement, setError, reportError, t]);

  /** Starts the next period, anchoring the clock at that period's theoretical start mark. */
  const startNextPeriod = useCallback(async (): Promise<void> => {
    const startingPeriod: number = periodManagement.nextPeriodToStart;
    if (startingPeriod <= 0) return;
    setError(null);

    // Each period anchors at its theoretical start mark regardless of when the previous
    // period was actually ended: with 15-minute periods, period 2 always begins at 15:00.
    const theoreticalStartSeconds: number = floorballPeriodStartSeconds(startingPeriod, matchRules, currentMatch.wentToOvertime);
    setPeriodStartTime(startingPeriod, theoreticalStartSeconds);

    try {
      if (theoreticalStartSeconds > 0) {
        try {
          await timer.setTime(theoreticalStartSeconds);
        } catch (alignError) {
          console.warn('Failed to align timer with period start mark:', alignError);
        }
      }
      await periodManagement.startPeriod();
      // Shootouts are not timed.
      if (startingPeriod !== periodManagement.shootoutPeriodNumber) {
        await timer.start(startingPeriod);
      }
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.startPeriod', 'Failed to start period'));
    }
  }, [periodManagement, matchRules, currentMatch.wentToOvertime, setPeriodStartTime, timer, setError, reportError, t]);

  const handleSkipToShootout = useCallback(async (): Promise<void> => {
    setError(null);
    try {
      await timer.stop();
      setPeriodStartTime(
        periodManagement.shootoutPeriodNumber,
        floorballPeriodStartSeconds(periodManagement.shootoutPeriodNumber, matchRules, false),
      );
      await periodManagement.skipToShootout();
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.skipToShootout', 'Failed to start penalty shootout'));
    }
  }, [timer, periodManagement, matchRules, setPeriodStartTime, setError, reportError, t]);

  const handlePeriodControlClick = useCallback((): void => {
    const control = periodManagement.periodControl;
    if (!control || control.loading) return;

    if (control.action === 'start') {
      void startNextPeriod();
      return;
    }

    // Nothing can follow the current period (shootout, last allowed period, or the score
    // is not level): ending it means finishing the match.
    if (control.action === 'finish') {
      setShowEndMatchConfirmation(true);
      return;
    }

    // Ask for confirmation only when the period is being cut short. The check is made against
    // time played *in this period*, not the continuous match clock.
    const currentElapsedSeconds: number = timer.getCurrentElapsedSeconds();
    const periodDurationSeconds: number = matchRules.periodDurationMinutes * 60;
    const inPeriodElapsedSeconds: number = Math.max(0, currentElapsedSeconds - currentPeriodStartSeconds);
    if (inPeriodElapsedSeconds < periodDurationSeconds) {
      setEndPeriodTimeSnapshot(formatEventTimeMmSs(currentElapsedSeconds));
      periodManagement.setShowEndPeriodConfirmation(true);
    } else {
      void finishCurrentPeriod();
    }
  }, [periodManagement, currentPeriodStartSeconds, matchRules.periodDurationMinutes, timer, startNextPeriod, finishCurrentPeriod]);

  const handleEndPeriodConfirm = useCallback(async (): Promise<void> => {
    await finishCurrentPeriod();
    periodManagement.setShowEndPeriodConfirmation(false);
  }, [finishCurrentPeriod, periodManagement]);

  const handleEndMatchConfirm = useCallback(async (): Promise<void> => {
    try {
      await timer.stop();
    } catch (error) {
      console.warn('Failed to stop timer before completing match:', error);
    }
    await matchControls.handleCompleteLive();
    setShowEndMatchConfirmation(false);
  }, [timer, matchControls]);

  const handleReopenConfirm = useCallback(async (): Promise<void> => {
    await matchControls.handleReopenMatch();
    setShowReopenConfirmation(false);
  }, [matchControls]);

  // ---------------------------------------------------------------------------
  // Event deletion
  // ---------------------------------------------------------------------------
  const requestDelete = useCallback((groups: EventGroup[]): void => {
    if (groups.length === 0) {
      setError(t('floorball.matches.manage.errors.noEventsSelected', 'Cannot delete: no events selected'));
      return;
    }
    const malformed: boolean = groups.some(g => g.events.length === 0 || g.events.some(e => !e.eventId));
    if (malformed) {
      setError(t('floorball.matches.manage.errors.missingEventId', 'Cannot delete: missing event id'));
      return;
    }
    setGroupsToDelete(groups);
  }, [setError, t]);

  const handleDeleteEvent = useCallback(async (): Promise<void> => {
    if (!groupsToDelete || groupsToDelete.length === 0) {
      setGroupsToDelete(null);
      return;
    }
    const eventsToDelete: ProcessedEvent[] = groupsToDelete.flatMap(group => group.events);

    setDeleteEventLoading(true);
    setError(null);
    try {
      for (const evt of eventsToDelete) {
        const eventId: string = evt.eventId as string;
        if (evt.type === 'goal') {
          await floorballMatchService.deleteGoal(match.id, eventId);
        } else if (evt.type === 'penalty') {
          await floorballMatchService.deletePenalty(match.id, eventId);
        } else {
          await floorballMatchService.deleteSave(match.id, eventId);
        }
      }
      await loadCurrentMatchStatus();
      await loadMatchEvents();
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.deleteEvent', 'Failed to delete event'));
    } finally {
      setGroupsToDelete(null);
      setDeleteEventLoading(false);
    }
  }, [groupsToDelete, match.id, loadCurrentMatchStatus, loadMatchEvents, setError, reportError, t]);

  // ---------------------------------------------------------------------------
  // Officials
  // ---------------------------------------------------------------------------
  const applyMatchUpdate = useCallback((updated: FloorballMatchDto): void => {
    setSelectedOfficials(updated.officials);
    setCurrentMatch(updated);
    setMatch(updated);
  }, [setCurrentMatch, setMatch]);

  const handleOfficialSelect = useCallback(async (index: number, refereeId: string): Promise<void> => {
    if (!refereeId) return;
    const isDuplicate: boolean = selectedOfficials.some((id, idx) => id === refereeId && idx !== index);
    if (isDuplicate) {
      setError(t('floorball.matches.manage.errors.refereeDuplicate', 'Referee already selected in another slot'));
      return;
    }

    const next: string[] = [...selectedOfficials];
    const wasEmpty: boolean = next[index] === '';
    next[index] = refereeId;

    setOfficialsSaving(true);
    setError(null);
    try {
      const resp = wasEmpty
        ? await floorballMatchService.addOfficial(match.id, refereeId)
        : await floorballMatchService.updateOfficials(match.id, next);
      if (resp.success && resp.data) applyMatchUpdate(resp.data);
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.setOfficial', 'Failed to set official'));
    } finally {
      setOfficialsSaving(false);
    }
  }, [match.id, selectedOfficials, applyMatchUpdate, setError, reportError, t]);

  const handleOfficialRemove = useCallback(async (index: number, refereeId: string): Promise<void> => {
    if (!refereeId) {
      setSelectedOfficials(prev => prev.filter((_, idx) => idx !== index));
      return;
    }
    setOfficialsSaving(true);
    setError(null);
    try {
      const resp = await floorballMatchService.deleteOfficial(match.id, refereeId);
      if (resp.success && resp.data) applyMatchUpdate(resp.data);
    } catch (error) {
      reportError(error, t('floorball.matches.manage.errors.removeOfficial', 'Failed to remove official'));
    } finally {
      setOfficialsSaving(false);
    }
  }, [match.id, applyMatchUpdate, setError, reportError, t]);

  const handleScorekeeperAdd = useCallback(async (personId: string): Promise<void> => {
    setScorekeepersSaving(true);
    setError(null);
    try {
      const resp = await floorballMatchService.addScorekeeper(match.id, personId);
      if (resp.success && resp.data) applyMatchUpdate(resp.data);
    } catch (error) {
      reportError(error, t('matchScorekeepers.errors.add', 'Failed to add scorekeeper'));
    } finally {
      setScorekeepersSaving(false);
    }
  }, [match.id, applyMatchUpdate, setError, reportError, t]);

  const handleScorekeeperRemove = useCallback(async (personId: string): Promise<void> => {
    setScorekeepersSaving(true);
    setError(null);
    try {
      const resp = await floorballMatchService.removeScorekeeper(match.id, personId);
      if (resp.success && resp.data) applyMatchUpdate(resp.data);
    } catch (error) {
      reportError(error, t('matchScorekeepers.errors.remove', 'Failed to remove scorekeeper'));
    } finally {
      setScorekeepersSaving(false);
    }
  }, [match.id, applyMatchUpdate, setError, reportError, t]);

  const isStartMatchDisabled: boolean = !homeGoalieId || !awayGoalieId || !currentMatch.homeTeamId || !currentMatch.awayTeamId;
  const startDisabledReason: string | undefined = !currentMatch.homeTeamId || !currentMatch.awayTeamId
    ? t('floorball.matches.manage.startDisabled.assignTeams', 'Assign both teams before starting')
    : (!homeGoalieId || !awayGoalieId)
      ? t('floorball.matches.manage.startDisabled.selectGoalies', 'Select goalies to start')
      : undefined;

  return (
    <>
      <LiveMatchModalHeader
        homeTeam={matchData.homeTeam}
        awayTeam={matchData.awayTeam}
        currentMatch={currentMatch}
        isSidesSwapped={isSidesSwapped}
        onToggleSides={() => setIsSidesSwapped(prev => !prev)}
        onClose={onClose}
        onCompleteLive={() => setShowEndMatchConfirmation(true)}
        onReopen={() => setShowReopenConfirmation(true)}
      />

      <ErrorPopup message={matchData.error} />

      <MatchConfirmationDialogs
        showEndPeriodConfirmation={periodManagement.showEndPeriodConfirmation}
        currentPeriod={currentPeriod}
        isOvertimePeriod={currentPeriod === periodManagement.overtimePeriodNumber}
        currentTimeFormatted={endPeriodTimeSnapshot}
        periodLoading={periodManagement.periodLoading}
        onEndPeriodConfirm={handleEndPeriodConfirm}
        onEndPeriodCancel={() => periodManagement.setShowEndPeriodConfirmation(false)}
        showEndMatchConfirmation={showEndMatchConfirmation}
        isShootout={periodManagement.isInShootout && periodManagement.startedPeriods.has(periodManagement.shootoutPeriodNumber)}
        onEndMatchConfirm={handleEndMatchConfirm}
        onEndMatchCancel={() => setShowEndMatchConfirmation(false)}
        showReopenConfirmation={showReopenConfirmation}
        onReopenConfirm={handleReopenConfirm}
        onReopenCancel={() => setShowReopenConfirmation(false)}
        groupsToDelete={groupsToDelete}
        deleteEventLoading={deleteEventLoading}
        onDeleteEventConfirm={handleDeleteEvent}
        onDeleteEventCancel={() => setGroupsToDelete(null)}
        matchLoading={matchData.loading}
      />

      <div className="modal-content">
        <div className="left-section">
          <LiveMatchTimer
            currentMatch={currentMatch}
            loading={matchData.loading}
            startedPeriods={periodManagement.startedPeriods}
            endedPeriods={periodManagement.endedPeriods}
            periodControl={periodManagement.periodControl}
            onPeriodControlClick={handlePeriodControlClick}
            onStartMatch={handleStartMatchAndTimer}
            keybindsEnabled={keybindsEnabled}
            isStartMatchDisabled={isStartMatchDisabled}
            startDisabledReason={startDisabledReason}
            overtimePeriodNumber={periodManagement.overtimePeriodNumber}
            shootoutPeriodNumber={periodManagement.shootoutPeriodNumber}
            showSkipToShootout={showSkipToShootout}
            skipToShootoutLoading={Boolean(periodManagement.periodLoading[periodManagement.shootoutPeriodNumber])}
            onSkipToShootout={() => { void handleSkipToShootout(); }}
          />

          <LiveMatchQuickActions
            loading={forms.loading}
            currentMatch={currentMatch}
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
            leftLineup={leftSideTeam === 'home' ? currentMatch.homeActivePlayers : currentMatch.awayActivePlayers}
            rightLineup={rightSideTeam === 'home' ? currentMatch.homeActivePlayers : currentMatch.awayActivePlayers}
            leftGoalieId={leftSideGoalieId}
            rightGoalieId={rightSideGoalieId}
            onEditLineup={() => setIsLineupDialogOpen(true)}
            disabled={isMatchClosed}
          />

          <EditActiveRosterDialog
            isOpen={isLineupDialogOpen}
            matchId={currentMatch.id}
            homeTeamId={homeTeamId}
            awayTeamId={awayTeamId}
            homeTeamName={matchData.homeTeam?.name ?? ''}
            awayTeamName={matchData.awayTeam?.name ?? ''}
            homePlayers={matchData.homePlayers}
            awayPlayers={matchData.awayPlayers}
            initialHomeLineup={currentMatch.homeActivePlayers ?? []}
            initialAwayLineup={currentMatch.awayActivePlayers ?? []}
            initialHomeGoalieId={homeGoalieId}
            initialAwayGoalieId={awayGoalieId}
            onClose={() => setIsLineupDialogOpen(false)}
            onSaved={(updated) => {
              setCurrentMatch(updated);
              setMatch(updated);
              setHomeGoalieId(updated.homeActiveGoalieId ?? '');
              setAwayGoalieId(updated.awayActiveGoalieId ?? '');
            }}
            onError={setError}
          />

          <OfficialsSelectorSection
            selectedOfficials={selectedOfficials}
            options={officialOptions}
            saving={officialsSaving}
            onAddRow={() => setSelectedOfficials(prev => [...prev, ''])}
            onSelect={handleOfficialSelect}
            onRemove={handleOfficialRemove}
            disabled={isMatchClosed}
          />

          <ScorekeepersSection
            scorekeepers={currentMatch.scorekeepers ?? []}
            saving={scorekeepersSaving}
            onAdd={handleScorekeeperAdd}
            onRemove={handleScorekeeperRemove}
            disabled={isMatchClosed}
          />

          <GoalRecordingForm
            showGoalForm={forms.showGoalForm}
            goalForm={forms.goalForm}
            setGoalForm={forms.setGoalForm}
            currentMatch={currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            loading={forms.loading}
            getPlayersForTeam={matchData.getPlayersForTeam}
            onRecordGoal={forms.recordGoal}
            onClose={() => forms.setShowGoalForm(false)}
          />

          <PenaltyRecordingForm
            showPenaltyForm={forms.showPenaltyForm}
            penaltyForm={forms.penaltyForm}
            setPenaltyForm={forms.setPenaltyForm}
            currentMatch={currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
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
            onDeleteEvent={(group) => requestDelete([group])}
            onBulkDelete={requestDelete}
            // Once the match is Completed/Cancelled the backend blocks event deletion; the only
            // legitimate edit path is to reopen the match first.
            canDelete={!isMatchClosed}
          />
        </div>
      </div>

      {bulkSaveTarget && (
        <BulkSaveDialog
          isOpen={true}
          goalieName={matchData.getPlayerNameById(bulkSaveTarget.goalieId)}
          teamName={(bulkSaveTarget.team === 'home' ? matchData.homeTeam?.name : matchData.awayTeam?.name) ?? ''}
          currentPeriod={currentPeriod}
          numberOfPeriods={matchRules.numberOfPeriods}
          periodDurationMinutes={matchRules.periodDurationMinutes}
          currentElapsedSeconds={timer.getCurrentElapsedSeconds()}
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
 * Wrapper component that provides the timer context. `matchId` lets the provider persist
 * per-period start times for the duration of the match (continuous-clock behaviour).
 */
const ManageMatchPageWithContext = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => (
  <MatchTimerProvider initialPeriod={1} matchId={match.id}>
    <ManageMatchPageContent match={match} setMatch={setMatch} onClose={onClose} />
  </MatchTimerProvider>
);

/**
 * Default landing page used when the user opens the match management view directly (no
 * originating view to return to).
 */
const DEFAULT_RETURN_PATH: string = '/admin/floorball/matches';

/**
 * Whitelists the `returnTo` query parameter to internal absolute paths only. This prevents
 * an open-redirect via a crafted URL and also guards against protocol-relative paths.
 */
const sanitizeReturnTo = (raw: string | null): string => {
  if (!raw) return DEFAULT_RETURN_PATH;
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
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showAssignTeams, setShowAssignTeams] = useState<boolean>(false);

  const returnTo: string = useMemo(() => sanitizeReturnTo(searchParams.get('returnTo')), [searchParams]);

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
      setError(t('floorball.matches.manage.errors.matchIdMissing', 'Match ID is missing'));
      setLoading(false);
      return;
    }

    const fetchMatch = async (): Promise<void> => {
      try {
        const response = await floorballMatchService.getById(matchId);
        if (response.success && response.data) {
          setMatch(response.data);
        } else {
          setError(t('floorball.matches.manage.errors.fetchMatch', 'Failed to fetch match data'));
        }
      } catch (err) {
        setError(describeMatchError(err, t('floorball.matches.manage.errors.fetchMatch', 'Failed to fetch match data'), t));
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    void fetchMatch();
  }, [matchId, t]);

  const pageTitle: string = t('floorball.matches.manage.title', 'Match management');

  if (loading) {
    return (
      <PageTemplate title={pageTitle}>
        <div className="manage-match-page manage-match-page--loading">{t('common.loading', 'Loading...')}</div>
      </PageTemplate>
    );
  }

  if (error) {
    return (
      <PageTemplate title={pageTitle}>
        <div className="manage-match-page">
          <ErrorPopup message={error} />
        </div>
      </PageTemplate>
    );
  }

  if (!match) {
    return (
      <PageTemplate title={pageTitle}>
        <div className="manage-match-page manage-match-page--loading">
          {t('floorball.matches.manage.notFound', 'Match not found.')}
        </div>
      </PageTemplate>
    );
  }

  const isMatchFinished: boolean = match.status === 'Completed';
  const isTeamsAssignable: boolean = match.status === 'Scheduled' || match.status === 'Postponed';
  const isMissingTeams: boolean = !match.homeTeamId || !match.awayTeamId;

  return (
    <PageTemplate title={pageTitle}>
      <div className="manage-match-page">
        <div className="page-header">
          <div className="page-header__top">
            <h1 className="page-title-compact font-title">{pageTitle}</h1>
            <div className="page-header__actions">
              {/* "Assign teams" is only meaningful while the match is still Scheduled/Postponed. */}
              {isTeamsAssignable && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={() => setShowAssignTeams(true)}
                  title={t('floorball.matches.assignTeams.action', 'Assign teams')}
                >
                  <i className="fas fa-users edit-match-button__icon" aria-hidden="true"></i>
                  <span className="edit-match-button__label">
                    {isMissingTeams
                      ? t('floorball.matches.assignTeams.actionMissing', 'Assign teams')
                      : t('floorball.matches.assignTeams.actionChange', 'Change teams')}
                  </span>
                </button>
              )}
              {/* Hidden once the match is Finished: the sanctioned recovery path is "Reopen match". */}
              {!isMatchFinished && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={handleNavigateToEdit}
                  disabled={!matchId}
                  title={t('floorball.matches.actions.edit')}
                >
                  <i className="fas fa-pen edit-match-button__icon" aria-hidden="true"></i>
                  <span className="edit-match-button__label">{t('floorball.matches.actions.edit')}</span>
                </button>
              )}
            </div>
          </div>
          {isMissingTeams && isTeamsAssignable && (
            <div className="page-header__missing-teams" role="status">
              <i className="fas fa-info-circle" aria-hidden="true"></i>
              {t(
                'floorball.matches.assignTeams.missingBanner',
                'This match does not have both teams yet. Assign the teams before starting the match.'
              )}
            </div>
          )}
        </div>
        {isMissingTeams ? (
          /* Without both teams the live UI cannot render rosters, goalies, or the scoreboard. */
          <div className="manage-match-page__placeholder">
            <i className="fas fa-users-slash" aria-hidden="true"></i>
            <p>
              {t(
                'floorball.matches.assignTeams.placeholderBody',
                'Both teams have not been assigned to this match yet, so the live management view cannot be opened.'
              )}
            </p>
            <button
              type="button"
              className="manage-match-page__placeholder-action"
              onClick={() => setShowAssignTeams(true)}
            >
              <i className="fas fa-user-plus" aria-hidden="true"></i>
              {t('floorball.matches.assignTeams.action', 'Assign teams')}
            </button>
          </div>
        ) : (
          <ManageMatchPageWithContext match={match} setMatch={setMatch} onClose={handleClose} />
        )}

        <AssignTeamsDialog
          isOpen={showAssignTeams}
          match={match}
          onClose={() => setShowAssignTeams(false)}
          onSaved={(updated) => {
            setMatch(updated);
            setShowAssignTeams(false);
          }}
        />
      </div>
    </PageTemplate>
  );
};

export default ManageMatchPage;
