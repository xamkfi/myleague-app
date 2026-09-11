import { useEffect, useMemo, useCallback, useState, useRef, type ReactElement } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { footballMatchService } from '../../../../api/football/footballMatchService';
import { timerService } from '../../../../api/common/timerService';
import type { FootballLineupPlayer, FootballMatchDto } from '../../../../types/football/footballTypes';
import type { FootballPlayerDto } from '../../../../api/football/footballPlayerService';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

import LiveMatchModalHeader from './components/LiveMatchModalHeader';
import LiveMatchScoreboard from './components/LiveMatchScoreboard';
import LiveMatchTimer from './components/LiveMatchTimer';
import LiveMatchQuickActions from './components/LiveMatchQuickActions';
import GoalRecordingForm from './components/GoalRecordingForm';
import CardRecordingForm from './components/CardRecordingForm';
import SubstitutionRecordingForm from './components/SubstitutionRecordingForm';
import LiveMatchEventsHistory from './components/LiveMatchEventsHistory';
import ActiveRosterCard from './components/ActiveRosterCard';
import EditActiveRosterDialog from './components/EditActiveRosterDialog';
import OfficialsSelectorSection from './components/OfficialsSelectorSection';
import MatchConfirmationDialogs from './components/MatchConfirmationDialogs';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import FootballAssignTeamsDialog from '../Components/FootballAssignTeamsDialog';
import type { EventGroup, ProcessedEvent } from './components/types';
import { footballRefereeService } from '../../../../api/football/footballRefereeService';

import { MatchTimerProvider, useMatchTimerContext } from './context';

import {
  useMatchData,
  useSignalR,
  usePeriodManagement,
  useMatchEvents,
  useFormState,
  useMatchControls,
} from './hooks';

import {
  areBothLineupsReady,
  getBenchPlayers,
  getOnFieldPlayers,
  getPeriodDurationSeconds,
  getTheoreticalPeriodStartSeconds,
  isPenaltyShootoutPeriod,
  resolveMatchRules,
} from './utils/lineupValidation';

import './ManageMatchPage.scss';

interface ManageMatchPageContentProps {
  match: FootballMatchDto;
  setMatch: (match: FootballMatchDto) => void;
  onClose: () => void;
}

const ManageMatchPageContent = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => {
  const timerContext = useMatchTimerContext();

  const [selectedOfficials, setSelectedOfficials] = useState<string[]>(match.officials || []);
  const [officialOptions, setOfficialOptions] = useState<Array<{ id: string; name: string }>>([]);
  const [officialsSaving, setOfficialsSaving] = useState(false);

  const [showEndMatchConfirmation, setShowEndMatchConfirmation] = useState(false);
  const [showReopenConfirmation, setShowReopenConfirmation] = useState(false);
  const [groupsToDelete, setGroupsToDelete] = useState<EventGroup[] | null>(null);
  const [deleteEventLoading, setDeleteEventLoading] = useState(false);
  const [shouldStartTimer, setShouldStartTimer] = useState(false);
  const [extraTimeLoading, setExtraTimeLoading] = useState(false);
  const [penaltyShootoutLoading, setPenaltyShootoutLoading] = useState(false);

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
      /* noop */
    }
  }, [sidesStorageKey, isSidesSwapped]);
  const [isLineupDialogOpen, setIsLineupDialogOpen] = useState(false);

  useEffect(() => {
    setSelectedOfficials(match.officials || []);
  }, [match.officials]);

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
    onGoLive: (_matchId: string, updatedMatch?: FootballMatchDto) => {
      if (updatedMatch) setMatch(updatedMatch);
    },
    onCompleteLive: (_matchId: string, updatedMatch?: FootballMatchDto) => {
      if (updatedMatch) setMatch(updatedMatch);
    },
    onReopen: (_matchId: string, updatedMatch?: FootballMatchDto) => {
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
    setNextPeriodToStart,
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
    onCardAssigned: matchEvents.handleCardAssigned,
    onSubstitutionRecorded: matchEvents.handleSubstitutionRecorded,
    onMatchStarted: matchEvents.handleMatchStarted,
    onMatchCompleted: matchEvents.handleMatchCompleted,
  });

  const homeTeamId = matchData.homeTeam?.id ?? '';
  const awayTeamId = matchData.awayTeam?.id ?? '';
  const toggleTimer = timerContext.callbacks.toggle;
  const timerCurrentPeriod = timerContext.currentPeriod;
  const setTimerCurrentPeriod = timerContext.setCurrentPeriod;
  const matchRules = resolveMatchRules(matchData.currentMatch.matchRules);

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

  const homeLineup: FootballLineupPlayer[] = useMemo(
    () => matchData.currentMatch.homeLineup ?? [],
    [matchData.currentMatch.homeLineup],
  );
  const awayLineup: FootballLineupPlayer[] = useMemo(
    () => matchData.currentMatch.awayLineup ?? [],
    [matchData.currentMatch.awayLineup],
  );
  const leftLineup = leftSideTeam === 'home' ? homeLineup : awayLineup;
  const rightLineup = rightSideTeam === 'home' ? homeLineup : awayLineup;

  const isPeriodActive = periodManagement.startedPeriods.has(timerContext.currentPeriod) &&
    !periodManagement.endedPeriods.has(timerContext.currentPeriod);

  const keybindsEnabled = matchData.currentMatch.status === 'InProgress' &&
    isPeriodActive &&
    !forms.showGoalForm &&
    !forms.showCardForm &&
    !forms.showSubstitutionForm &&
    !isLineupDialogOpen &&
    !showEndMatchConfirmation &&
    !showReopenConfirmation &&
    !groupsToDelete &&
    !periodManagement.showEndPeriodConfirmation;

  const lineupReady = areBothLineupsReady(homeLineup, awayLineup, matchRules);
  const hasOfficials = selectedOfficials.some((id) => id.trim().length > 0);
  const officialsBlocking = matchRules.requireOfficialsToStart && !hasOfficials;
  const isStartMatchDisabled =
    !matchData.currentMatch.homeTeamId ||
    !matchData.currentMatch.awayTeamId ||
    !lineupReady ||
    officialsBlocking;

  const startDisabledReason: string | undefined = !matchData.currentMatch.homeTeamId || !matchData.currentMatch.awayTeamId
    ? 'Assign both teams before starting'
    : !lineupReady
      ? 'Set lineup for both teams to start'
      : officialsBlocking
        ? 'Assign officials to start'
        : undefined;

  const showExtraTimeButton =
    matchData.currentMatch.status === 'InProgress' &&
    matchRules.allowExtraTime &&
    !matchData.currentMatch.wentToExtraTime;

  const showPenaltyShootoutButton =
    matchData.currentMatch.status === 'InProgress' &&
    matchRules.allowPenaltyShootout &&
    !matchData.currentMatch.wentToPenaltyShootout;

  const playersFromLineup = useCallback((
    teamId: string,
    selector: (lineup: FootballLineupPlayer[]) => FootballLineupPlayer[],
  ): FootballPlayerDto[] => {
    const lineup = teamId === homeTeamId ? homeLineup : awayLineup;
    const roster = matchData.getPlayersForTeam(teamId);
    const selectedIds = new Set(selector(lineup).map((entry) => entry.playerId));
    return roster.filter((player) => selectedIds.has(player.id));
  }, [homeTeamId, homeLineup, awayLineup, matchData]);

  const getOnFieldPlayersForTeam = useCallback(
    (teamId: string) => playersFromLineup(teamId, getOnFieldPlayers),
    [playersFromLineup],
  );

  const getBenchPlayersForTeam = useCallback(
    (teamId: string) => playersFromLineup(teamId, getBenchPlayers),
    [playersFromLineup],
  );

  const getSquadPlayersForTeam = useCallback((teamId: string): FootballPlayerDto[] => {
    const lineup = teamId === homeTeamId ? homeLineup : awayLineup;
    const roster = matchData.getPlayersForTeam(teamId);
    const squadIds = new Set(
      lineup.filter((entry) => !entry.isSentOff).map((entry) => entry.playerId),
    );
    return roster.filter((player) => squadIds.has(player.id));
  }, [homeTeamId, homeLineup, awayLineup, matchData]);

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

  useEffect(() => {
    let isCancelled = false;
    const loadOfficials = async () => {
      if (!match.id) return;
      try {
        const response = await footballRefereeService.getAll({ pageSize: 50 });
        if (!isCancelled && response.success && response.data) {
          const mapped = response.data.map((ref) => ({ id: ref.id, name: ref.person.fullName }));
          const sorted = [...mapped].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }));
          const guestIndex = sorted.findIndex((option) => option.name.toUpperCase() === 'GUEST REFEREE');
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
            const period = parseInt(periodNum, 10);
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
    areNumberSetsEqual,
  ]);

  useEffect(() => {
    matchData.loadTeamData();
    matchEvents.loadMatchEvents();
    matchData.loadCurrentMatchStatus();
    signalR.setupSignalR();
    return () => { signalR.cleanupSignalR(); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (match && (match.id !== matchData.currentMatch.id || match.status !== matchData.currentMatch.status)) {
      matchData.setCurrentMatch(match);
    }
  }, [match, matchData]);

  useEffect(() => {
    if (!keybindsEnabled) return;

    const handler = (e: KeyboardEvent) => {
      if (e.ctrlKey || e.metaKey || e.altKey) return;

      const target = e.target as HTMLElement | null;
      if (target) {
        const tagName: string = target.tagName;
        if (
          tagName === 'INPUT' ||
          tagName === 'TEXTAREA' ||
          tagName === 'SELECT' ||
          target.isContentEditable
        ) {
          return;
        }
        if (
          typeof target.closest === 'function' &&
          target.closest(
            '[role="dialog"], dialog, .modal, .goal-record-modal, .card-record-modal, .sub-record-modal, .eard-dialog',
          )
        ) {
          return;
        }
      }

      if (e.key === ' ' && toggleTimer) {
        toggleTimer();
        e.preventDefault();
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [keybindsEnabled, toggleTimer]);

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

      const currentElapsedSeconds = timerContext.callbacks.getCurrentElapsedSeconds
        ? timerContext.callbacks.getCurrentElapsedSeconds()
        : timerContext.elapsedTimeSeconds;

      const periodDurationSeconds: number = getPeriodDurationSeconds(timerContext.currentPeriod, matchRules);
      const inPeriodElapsedSeconds: number = Math.max(0, currentElapsedSeconds - timerContext.currentPeriodStartSeconds);
      const isUnderConfiguredPeriodLength: boolean =
        periodDurationSeconds > 0 && inPeriodElapsedSeconds < periodDurationSeconds;
      if (isUnderConfiguredPeriodLength) {
        periodManagement.setShowEndPeriodConfirmation(true);
      } else {
        (async () => {
          await periodManagement.endPeriod();
          if (timerContext.callbacks.stop) timerContext.callbacks.stop();
        })();
      }
    } else {
      (async () => {
        const startingPeriod: number = periodManagement.nextPeriodToStart;
        const theoreticalStartSeconds = getTheoreticalPeriodStartSeconds(startingPeriod, matchRules);

        if (startingPeriod > 0) {
          timerContext.setPeriodStartTime(startingPeriod, theoreticalStartSeconds);
        }

        if (theoreticalStartSeconds > 0) {
          try {
            await timerService.setTimer(match.id, theoreticalStartSeconds);
          } catch (err) {
            console.warn('Failed to align timer with half start mark:', err);
          }
        }

        await periodManagement.startPeriod();
        if (!isPenaltyShootoutPeriod(startingPeriod, matchRules)) {
          if (timerContext.callbacks.start) {
            await timerContext.callbacks.start();
          } else if (timerContext.callbacks.toggle) {
            await timerContext.callbacks.toggle();
          }
        }
      })();
    }
  }, [periodManagement, timerContext, matchRules, match.id]);

  const handleRecordExtraTime = useCallback(async () => {
    try {
      setExtraTimeLoading(true);
      matchData.setError(null);
      await periodManagement.recordOvertime();
    } catch (error) {
      matchData.setError(error instanceof Error ? error.message : 'Failed to start extra time');
    } finally {
      setExtraTimeLoading(false);
    }
  }, [periodManagement, matchData]);

  const handleRecordPenaltyShootout = useCallback(async () => {
    try {
      setPenaltyShootoutLoading(true);
      matchData.setError(null);
      await periodManagement.recordShootout();
    } catch (error) {
      matchData.setError(error instanceof Error ? error.message : 'Failed to start penalty shootout');
    } finally {
      setPenaltyShootoutLoading(false);
    }
  }, [periodManagement, matchData]);

  const handleDeleteEvent = useCallback(async () => {
    if (!groupsToDelete || groupsToDelete.length === 0) {
      setGroupsToDelete(null);
      return;
    }
    const allEvents: ProcessedEvent[] = groupsToDelete.flatMap((group) => group.events);
    const eventsToDelete: ProcessedEvent[] = allEvents.filter((e) => !!e.eventId);
    if (eventsToDelete.length !== allEvents.length || eventsToDelete.length === 0) {
      matchData.setError('Cannot delete: missing event id');
      setGroupsToDelete(null);
      return;
    }

    try {
      setDeleteEventLoading(true);
      matchData.setError(null);
      await matchData.loadCurrentMatchStatus();

      for (const evt of eventsToDelete) {
        const eventId: string = evt.eventId as string;
        if (evt.type === 'goal') {
          await footballMatchService.deleteGoal(match.id, eventId);
        } else if (evt.type === 'card') {
          await footballMatchService.deleteCard(match.id, eventId);
        } else if (evt.type === 'substitution') {
          await footballMatchService.deleteSubstitution(match.id, eventId);
        }
      }

      await matchData.loadCurrentMatchStatus();
      await matchEvents.loadMatchEvents();
      setGroupsToDelete(null);
    } catch (err) {
      matchData.setError(err instanceof Error ? err.message : 'Failed to delete event');
      setGroupsToDelete(null);
    } finally {
      setDeleteEventLoading(false);
    }
  }, [groupsToDelete, match.id, matchData, matchEvents]);

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
        ? await footballMatchService.addOfficial(match.id, refereeId)
        : await footballMatchService.updateOfficials(match.id, next);

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
      setSelectedOfficials((prev) => prev.filter((_, idx) => idx !== index));
      return;
    }

    try {
      setOfficialsSaving(true);
      matchData.setError(null);
      const resp = await footballMatchService.deleteOfficial(match.id, refereeId);
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

  const currentTimeFormatted = timerContext.formatTime(
    Math.floor(timerContext.elapsedTimeSeconds / 60),
    timerContext.elapsedTimeSeconds % 60,
  );

  return (
    <>
      <LiveMatchModalHeader
        homeTeam={matchData.homeTeam}
        awayTeam={matchData.awayTeam}
        currentMatch={matchData.currentMatch}
        isSidesSwapped={isSidesSwapped}
        onToggleSides={() => setIsSidesSwapped((prev) => !prev)}
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
          if (timerContext.callbacks.stop) timerContext.callbacks.stop();
          periodManagement.setShowEndPeriodConfirmation(false);
        }}
        onEndPeriodCancel={periodManagement.cancelEndPeriod}
        showOvertimeConfirmation={periodManagement.showOvertimeConfirmation}
        onOvertimeConfirm={handleRecordExtraTime}
        onOvertimeCancel={() => periodManagement.setShowOvertimeConfirmation(false)}
        showShootoutConfirmation={periodManagement.showShootoutConfirmation}
        onShootoutConfirm={handleRecordPenaltyShootout}
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
        groupsToDelete={groupsToDelete}
        deleteEventLoading={deleteEventLoading}
        onDeleteEventConfirm={handleDeleteEvent}
        onDeleteEventCancel={() => setGroupsToDelete(null)}
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
            isStartMatchDisabled={isStartMatchDisabled}
            startDisabledReason={startDisabledReason}
            overtimePeriodNumber={periodManagement.overtimePeriodNumber}
            shootoutPeriodNumber={periodManagement.shootoutPeriodNumber}
            showExtraTimeButton={showExtraTimeButton}
            showPenaltyShootoutButton={showPenaltyShootoutButton}
            extraTimeLoading={extraTimeLoading}
            penaltyShootoutLoading={penaltyShootoutLoading}
            onRecordExtraTime={() => periodManagement.setShowOvertimeConfirmation(true)}
            onRecordPenaltyShootout={() => periodManagement.setShowShootoutConfirmation(true)}
          />

          <LiveMatchQuickActions
            loading={forms.loading}
            currentMatch={matchData.currentMatch}
            leftTeamId={leftSideTeamId}
            rightTeamId={rightSideTeamId}
            leftTeamName={leftSideTeamData?.name}
            rightTeamName={rightSideTeamData?.name}
            onShowGoalForm={forms.openGoalFormForTeam}
            onShowCardForm={forms.openCardFormForTeam}
            onShowSubstitutionForm={forms.openSubstitutionFormForTeam}
          />

          <ActiveRosterCard
            leftTeamName={leftSideTeamData?.name}
            rightTeamName={rightSideTeamData?.name}
            leftPlayers={leftSidePlayers}
            rightPlayers={rightSidePlayers}
            leftLineup={leftLineup}
            rightLineup={rightLineup}
            matchRules={matchRules}
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
            initialHomeLineup={homeLineup}
            initialAwayLineup={awayLineup}
            matchRules={matchRules}
            onClose={() => setIsLineupDialogOpen(false)}
            onSaved={(updated) => {
              matchData.setCurrentMatch(updated);
              setMatch(updated);
            }}
            onError={matchData.setError}
          />

          <OfficialsSelectorSection
            selectedOfficials={selectedOfficials}
            options={officialOptions}
            saving={officialsSaving}
            onAddRow={() => setSelectedOfficials((prev) => [...prev, ''])}
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
            loading={forms.loading}
            getOnFieldPlayersForTeam={getOnFieldPlayersForTeam}
            onRecordGoal={forms.recordGoal}
            onClose={() => forms.setShowGoalForm(false)}
          />

          <CardRecordingForm
            showCardForm={forms.showCardForm}
            cardForm={forms.cardForm}
            setCardForm={forms.setCardForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            loading={forms.loading}
            getPlayersForTeam={getSquadPlayersForTeam}
            onRecordCard={forms.recordCard}
            onClose={() => forms.setShowCardForm(false)}
          />

          <SubstitutionRecordingForm
            showSubstitutionForm={forms.showSubstitutionForm}
            substitutionForm={forms.substitutionForm}
            setSubstitutionForm={forms.setSubstitutionForm}
            currentMatch={matchData.currentMatch}
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            loading={forms.loading}
            getOnFieldPlayersForTeam={getOnFieldPlayersForTeam}
            getBenchPlayersForTeam={getBenchPlayersForTeam}
            onRecordSubstitution={forms.recordSubstitution}
            onClose={() => forms.setShowSubstitutionForm(false)}
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
              if (group.events.length === 0 || !group.representative.eventId) {
                matchData.setError('Cannot delete: missing event id');
                return;
              }
              setGroupsToDelete([group]);
            }}
            onBulkDelete={(groups) => {
              if (groups.length === 0) {
                matchData.setError('Cannot delete: no events selected');
                return;
              }
              const malformed: boolean = groups.some(
                (g) => g.events.length === 0 || g.events.some((e) => !e.eventId),
              );
              if (malformed) {
                matchData.setError('Cannot delete: missing event id');
                return;
              }
              setGroupsToDelete(groups);
            }}
            canDelete={matchData.currentMatch.status !== 'Completed' && matchData.currentMatch.status !== 'Cancelled'}
          />
        </div>
      </div>
    </>
  );
};

const ManageMatchPageWithContext = ({ match, setMatch, onClose }: ManageMatchPageContentProps) => {
  return (
    <MatchTimerProvider initialPeriod={1} matchId={match.id}>
      <ManageMatchPageContent match={match} setMatch={setMatch} onClose={onClose} />
    </MatchTimerProvider>
  );
};

const DEFAULT_RETURN_PATH = '/admin/football/matches';

const sanitizeReturnTo = (raw: string | null): string => {
  if (!raw) return DEFAULT_RETURN_PATH;
  if (!raw.startsWith('/') || raw.startsWith('//')) return DEFAULT_RETURN_PATH;
  return raw;
};

const ManageMatchPage = (): ReactElement => {
  const { matchId } = useParams<{ matchId: string }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();
  const [match, setMatch] = useState<FootballMatchDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAssignTeams, setShowAssignTeams] = useState<boolean>(false);

  const returnTo: string = useMemo(
    () => sanitizeReturnTo(searchParams.get('returnTo')),
    [searchParams],
  );

  const handleClose = useCallback((): void => {
    navigate(returnTo);
  }, [navigate, returnTo]);

  const handleNavigateToEdit = useCallback((): void => {
    if (matchId) {
      navigate(`/admin/football/matches/${matchId}/edit`);
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
        const response = await footballMatchService.getById(matchId);
        if (response.success && response.data) {
          setMatch(response.data);
        } else {
          setError('Failed to fetch match data');
        }
      } catch {
        setError('An error occurred while fetching match data.');
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
  const isTeamsAssignable: boolean = match.status === 'Scheduled' || match.status === 'Postponed';
  const isMissingTeams: boolean = !match.homeTeamId || !match.awayTeamId;

  return (
    <PageTemplate title="Manage match page">
      <div className="manage-match-page">
        <div className="page-header">
          <div className="page-header__top">
            <h1 className="page-title-compact font-title">MATCH MANAGEMENT</h1>
            <div className="page-header__actions">
              {isTeamsAssignable && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={() => setShowAssignTeams(true)}
                  title={t('football.matches.assignTeams.action', 'Aseta joukkueet')}
                >
                  <span className="edit-match-button__icon" aria-hidden="true">👥</span>
                  <span className="edit-match-button__label">
                    {isMissingTeams
                      ? t('football.matches.assignTeams.actionMissing', 'Aseta joukkueet')
                      : t('football.matches.assignTeams.actionChange', 'Muuta joukkueita')}
                  </span>
                </button>
              )}
              {!isMatchFinished && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={handleNavigateToEdit}
                  disabled={!matchId}
                  title={t('football.matches.actions.edit')}
                >
                  <span className="edit-match-button__icon" aria-hidden="true">✏️</span>
                  <span className="edit-match-button__label">{t('football.matches.actions.edit')}</span>
                </button>
              )}
            </div>
          </div>
          {isMissingTeams && isTeamsAssignable && (
            <div className="page-header__missing-teams" role="status">
              <i className="fas fa-info-circle" aria-hidden="true"></i>
              {t(
                'football.matches.assignTeams.missingBanner',
                'Tällä ottelulla ei ole molempia joukkueita. Aseta joukkueet ennen ottelun aloittamista.',
              )}
            </div>
          )}
        </div>
        {isMissingTeams ? (
          <div className="manage-match-page__placeholder">
            <i className="fas fa-users-slash" aria-hidden="true"></i>
            <p>
              {t(
                'football.matches.assignTeams.placeholderBody',
                'Otteluun ei ole vielä asetettu molempia joukkueita, joten ottelun hallintanäkymää ei voi vielä avata.',
              )}
            </p>
            <button
              type="button"
              className="manage-match-page__placeholder-action"
              onClick={() => setShowAssignTeams(true)}
            >
              <i className="fas fa-user-plus" aria-hidden="true"></i>
              {t('football.matches.assignTeams.action', 'Aseta joukkueet')}
            </button>
          </div>
        ) : (
          <ManageMatchPageWithContext match={match} setMatch={setMatch} onClose={handleClose} />
        )}

        <FootballAssignTeamsDialog
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
