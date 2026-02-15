import { useEffect, useMemo, useCallback, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
import ActivePlayersSelector from './components/ActivePlayersSelector';
import OfficialsSelectorSection from './components/OfficialsSelectorSection';
import MatchConfirmationDialogs from './components/MatchConfirmationDialogs';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import type { ProcessedEvent } from './components/types';
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
}

/**
 * Main content component that uses the timer context
 */
const ManageMatchPageContent = ({ match, setMatch }: ManageMatchPageContentProps) => {
  const navigate = useNavigate();
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
  const [saveLoading, setSaveLoading] = useState(false);
  const [eventToDelete, setEventToDelete] = useState<ProcessedEvent | null>(null);
  const [deleteEventLoading, setDeleteEventLoading] = useState(false);
  const [shouldStartTimer, setShouldStartTimer] = useState(false);

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
    isOpen: true,
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

  const currentScore = useMemo(() => ({
    home: match?.homeScore ?? 0,
    away: match?.awayScore ?? 0,
  }), [match]);

  const isPeriodActive = periodManagement.startedPeriods.has(timerContext.currentPeriod) &&
    !periodManagement.endedPeriods.has(timerContext.currentPeriod);

  const keybindsEnabled = matchData.currentMatch.status === 'InProgress' &&
    isPeriodActive &&
    !forms.showGoalForm &&
    !forms.showPenaltyForm;

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
          
          const startedPeriods = new Set<number>();
          const endedPeriods = new Set<number>();
          const periodScores = matchData.currentMatch.periodScores || {};
          
          Object.entries(periodScores).forEach(([periodNum, scoreData]) => {
            const period = parseInt(periodNum);
            if (scoreData.isCompleted) {
              startedPeriods.add(period);
              endedPeriods.add(period);
            } else if (period === currentPeriod) {
              startedPeriods.add(period);
          }
          });
          
          startedPeriods.add(currentPeriod);
          
          const maxPeriod = periodManagement.maxPeriodNumber;
          let nextPeriod = 1;
          for (let i = 1; i <= maxPeriod; i++) {
            if (!startedPeriods.has(i)) {
              nextPeriod = i;
              break;
            }
          }
          if (nextPeriod > maxPeriod || startedPeriods.has(maxPeriod)) {
            nextPeriod = 0;
          }
          
          timerContext.setCurrentPeriod(currentPeriod);
          periodManagement.setStartedPeriods(startedPeriods);
          periodManagement.setEndedPeriods(endedPeriods);
          periodManagement.setNextPeriodToStart(nextPeriod);
        } else {
          periodManagement.setStartedPeriods(new Set());
          periodManagement.setEndedPeriods(new Set());
          periodManagement.setNextPeriodToStart(1);
        }
      } catch (error) {
        console.warn('Failed to initialize period state:', error);
        if (matchData.currentMatch.status === 'InProgress') {
          periodManagement.setStartedPeriods(new Set([1]));
          periodManagement.setNextPeriodToStart(2);
        }
      }
    };
    initializePeriodState();
  }, [matchData.currentMatch.status, match.id]);

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
  }, [match]);

  // Keyboard shortcuts
  useEffect(() => {
    if (!keybindsEnabled) return;
    
    const handler = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement;
      if (['INPUT', 'TEXTAREA'].includes(target?.tagName) || target?.isContentEditable) return;
      
      const key = e.key.toLowerCase();
      if (key === 'q' && homeGoalieId) {
        handleRecordSave('home', homeGoalieId);
        e.preventDefault();
      }
      if (key === 'r' && awayGoalieId) {
        handleRecordSave('away', awayGoalieId);
        e.preventDefault();
      }
      if (key === ' ' && timerContext.callbacks.toggle) {
        timerContext.callbacks.toggle();
        e.preventDefault();
      }
    };
    
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [keybindsEnabled, homeGoalieId, awayGoalieId, timerContext.callbacks.toggle]);

  // Trigger timer start after match starts
  useEffect(() => {
    if (shouldStartTimer && timerContext.callbacks.toggle) {
      timerContext.callbacks.toggle();
      setShouldStartTimer(false);
    }
  }, [shouldStartTimer, timerContext.callbacks.toggle]);

  // Handlers
  const handleRecordSave = useCallback(async (team: 'home' | 'away', goalieId: string) => {
    if (!match.id) return;

    const key = `${match.id}:${team}:${goalieId}`;
    const now = Date.now();
    if (lastSaveRef.current[key] && now - lastSaveRef.current[key] < 1000) return;
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
    if (!eventToDelete?.eventId) {
      matchData.setError('Cannot delete: missing event id');
      setEventToDelete(null);
      return;
    }
    
    try {
      setDeleteEventLoading(true);
      matchData.setError(null);
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
        onClose={() => navigate('/admin/floorball/matches')}
        onCompleteLive={() => setShowEndMatchConfirmation(true)}
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
        
        eventToDelete={eventToDelete}
        deleteEventLoading={deleteEventLoading}
        formatEventTime={timerContext.formatEventTime}
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
            onSelect={handleOfficialSelect}
            onRemove={handleOfficialRemove}
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
            homeTeam={matchData.homeTeam}
            awayTeam={matchData.awayTeam}
            currentScore={currentScore}
          />

          <LiveMatchEventsHistory
            allEvents={matchEvents.allEvents}
            formatEventTime={timerContext.formatEventTime}
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
  );
};

/**
 * Wrapper component that provides the timer context
 */
const ManageMatchPageWithContext = ({ match, setMatch }: ManageMatchPageContentProps) => {
  return (
    <MatchTimerProvider initialPeriod={1}>
      <ManageMatchPageContent match={match} setMatch={setMatch} />
    </MatchTimerProvider>
  );
};

/**
 * Main page component with data loading
 */
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
    <PageTemplate title="Manage match page">
    <div className="manage-match-page">
        <div className="page-header">
          <div className="page-header__top">
            <h1 className="page-title-compact font-title">MATCH MANAGEMENT</h1>
          </div>
        </div>
        <ManageMatchPageWithContext match={match} setMatch={setMatch} />
    </div>
    </PageTemplate>
  );
};

export default ManageMatchPage;
