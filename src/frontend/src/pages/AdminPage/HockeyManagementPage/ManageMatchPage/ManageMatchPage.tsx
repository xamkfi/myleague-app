import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import ConfirmationDialog from '../../../../components/ConfirmationDialog/ConfirmationDialog';
import { MatchTimerProvider, useMatchTimerContext } from '../../../../components/MatchTimer';
import { hockeyMatchService } from '../../../../api/hockey/hockeyMatchService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeyOfficialService } from '../../../../api/hockey/hockeyOfficialService';
import { hockeyStatisticsService } from '../../../../api/hockey/hockeyStatisticsService';
import { timerService } from '../../../../api/common/timerService';
import { useIntervalWhen } from '../../../../hooks/useIntervalWhen';
import {
  hockeyAwayTeam,
  hockeyHomeTeam,
  hockeyOpposingGoalieId,
  hockeyShotCreditsGoalieSave,
  hockeyShotIsOnGoal,
  isHockeyMatchFinished,
  isHockeyMatchLive,
  HOCKEY_FACEOFF_SPOTS_BY_ZONE,
  type HockeyFaceoffSpot,
  type HockeyFaceoffZone,
  type HockeyGoalStrength,
  type HockeyMatchDto,
  type HockeyMatchTeamDto,
  type HockeyOfficialRole,
  type HockeyPenaltyOffence,
  type HockeyPenaltySeverity,
  type HockeyShotResult,
  type HockeyTeamDto,
} from '../../../../types/hockey/hockeyTypes';
import { loadHockeyRosterNameMaps, loadPersonNameMap, loadTeamNameMap } from '../../../../utils/hockeyLookups';
import {
  DEFAULT_HOCKEY_MATCH_RULES,
  useHockeyPeriodManagement,
} from './hooks/useHockeyPeriodManagement';
import LiveMatchModalHeader from './components/LiveMatchModalHeader';
import LiveMatchScoreboard from './components/LiveMatchScoreboard';
import LiveMatchTimer from './components/LiveMatchTimer';
import LiveMatchQuickActions from './components/LiveMatchQuickActions';
import GoalRecordingForm from './components/GoalRecordingForm';
import PenaltyRecordingForm from './components/PenaltyRecordingForm';
import ShotRecordingForm from './components/ShotRecordingForm';
import FaceoffRecordingForm from './components/FaceoffRecordingForm';
import LiveMatchEventsHistory from './components/LiveMatchEventsHistory';
import ActiveRosterCard from './components/ActiveRosterCard';
import EditActiveRosterDialog from './components/EditActiveRosterDialog';
import OfficialsSelectorSection from './components/OfficialsSelectorSection';
import { toFormPlayers } from './components/eventFormHelpers';
import './ManageMatchPage.scss';

type EventFormKind = 'goal' | 'penalty' | 'shot' | 'faceoff' | null;

function theoreticalPeriodStartSeconds(
  period: number,
  overtimePeriodNumber: number,
  shootoutPeriodNumber: number,
  wentToOvertime: boolean,
): number {
  const periodDurationSeconds = DEFAULT_HOCKEY_MATCH_RULES.periodDurationMinutes * 60;
  const overtimeDurationSeconds = DEFAULT_HOCKEY_MATCH_RULES.overtimeDurationMinutes * 60;
  const regulationLength = DEFAULT_HOCKEY_MATCH_RULES.numberOfPeriods * periodDurationSeconds;
  if (period === overtimePeriodNumber) {
    return regulationLength;
  }
  if (period === shootoutPeriodNumber) {
    return wentToOvertime ? regulationLength + overtimeDurationSeconds : regulationLength;
  }
  return Math.max(0, (period - 1) * periodDurationSeconds);
}

interface ManageHockeyMatchContentProps {
  match: HockeyMatchDto;
  setMatch: (match: HockeyMatchDto) => void;
  onClose: () => void;
}

function ManageHockeyMatchContent({ match, setMatch, onClose }: ManageHockeyMatchContentProps) {
  const { t } = useTranslation();
  const timer = useMatchTimerContext();
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [teamNames, setTeamNames] = useState<Map<string, string>>(new Map());
  const [playerNames, setPlayerNames] = useState<Map<string, string>>(new Map());
  const [officials, setOfficials] = useState<Array<{ id: string; personId: string; officialRole: string }>>([]);
  const [officialNames, setOfficialNames] = useState<Map<string, string>>(new Map());
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [isSidesSwapped, setIsSidesSwapped] = useState(false);
  const [eventForm, setEventForm] = useState<EventFormKind>(null);
  const [selectedTeamId, setSelectedTeamId] = useState(hockeyHomeTeam(match)?.id ?? '');
  const [playerId, setPlayerId] = useState('');
  const [assistId, setAssistId] = useState('');
  const [secondaryAssistId, setSecondaryAssistId] = useState('');
  const [goalStrength, setGoalStrength] = useState<HockeyGoalStrength>('EvenStrength');
  const [penaltyMinutes, setPenaltyMinutes] = useState(2);
  const [penaltyOffence, setPenaltyOffence] = useState<HockeyPenaltyOffence>('Tripping');
  const [penaltySeverity, setPenaltySeverity] = useState<HockeyPenaltySeverity>('Minor');
  const [shotResult, setShotResult] = useState<HockeyShotResult>('Saved');
  const [faceoffWinnerId, setFaceoffWinnerId] = useState('');
  const [faceoffZone, setFaceoffZone] = useState<HockeyFaceoffZone>('NeutralZone');
  const [faceoffSpot, setFaceoffSpot] = useState<HockeyFaceoffSpot>('CenterIce');
  const [faceoffWinnerPlayerId, setFaceoffWinnerPlayerId] = useState('');
  const [faceoffLoserPlayerId, setFaceoffLoserPlayerId] = useState('');
  const [showFinishConfirm, setShowFinishConfirm] = useState(false);
  const [showReopenConfirm, setShowReopenConfirm] = useState(false);
  const [isLineupDialogOpen, setIsLineupDialogOpen] = useState(false);
  const [shouldStartTimer, setShouldStartTimer] = useState(false);
  const [showOfficialDraft, setShowOfficialDraft] = useState(false);
  const restoredStatusRef = useRef('');
  const eventStampRef = useRef<{ periodNumber: number; timeInSeconds: number } | null>(null);

  const getElapsedSeconds = useCallback((): number => {
    if (timer.callbacks.getCurrentElapsedSeconds) {
      return timer.callbacks.getCurrentElapsedSeconds();
    }
    return timer.elapsedTimeSeconds;
  }, [timer.callbacks, timer.elapsedTimeSeconds]);

  const periodManagement = useHockeyPeriodManagement({
    currentMatch: match,
    currentPeriod: timer.currentPeriod,
    setCurrentPeriod: timer.setCurrentPeriod,
    getElapsedSeconds,
    onMatchUpdated: setMatch,
  });

  const getInPeriodSeconds = useCallback((): number => {
    return Math.max(0, getElapsedSeconds() - timer.currentPeriodStartSeconds);
  }, [getElapsedSeconds, timer.currentPeriodStartSeconds]);

  const captureEventStamp = useCallback((): { periodNumber: number; timeInSeconds: number } => {
    const stamp = {
      periodNumber: timer.currentPeriod,
      timeInSeconds: getInPeriodSeconds(),
    };
    eventStampRef.current = stamp;
    return stamp;
  }, [timer.currentPeriod, getInPeriodSeconds]);

  const clearEventStamp = useCallback((): void => {
    eventStampRef.current = null;
  }, []);

  const reloadLookups = useCallback(async (): Promise<void> => {
    const [teamList, officialList] = await Promise.all([
      hockeyTeamService.getAll(),
      hockeyOfficialService.getAll(true),
    ]);
    setTeams(teamList);
    setTeamNames(await loadTeamNameMap(teamList));
    const rosterNames = await loadHockeyRosterNameMaps(teamList);
    setPlayerNames(rosterNames.byTeamPlayerId);
    setOfficials(officialList);
    const people = await loadPersonNameMap(officialList.map((item) => item.personId));
    setOfficialNames(people);
  }, []);

  useEffect(() => {
    void reloadLookups().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load match extras'));
  }, [reloadLookups]);

  const restoreFromMatch = periodManagement.restoreFromMatch;
  useEffect(() => {
    const key = `${match.id}:${match.status}`;
    if (restoredStatusRef.current === key) {
      return;
    }
    restoredStatusRef.current = key;
    if (isHockeyMatchLive(match.status) || isHockeyMatchFinished(match.status)) {
      restoreFromMatch(match);
    }
  }, [match, restoreFromMatch]);

  useEffect(() => {
    if (shouldStartTimer && timer.callbacks.toggle) {
      void timer.callbacks.toggle();
      setShouldStartTimer(false);
    }
  }, [shouldStartTimer, timer.callbacks]);

  const pollMatch = useCallback(async (): Promise<void> => {
    try {
      const latest = await hockeyMatchService.getById(match.id);
      setMatch(latest);
    } catch {
      /* keep current snapshot */
    }
  }, [match.id, setMatch]);

  useIntervalWhen(isHockeyMatchLive(match.status), () => {
    void pollMatch();
  }, 4000);

  const run = async (action: () => Promise<HockeyMatchDto | void>): Promise<boolean> => {
    setBusy(true);
    setError(null);
    try {
      const result = await action();
      if (result) {
        setMatch(result);
      }
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Operation failed');
      return false;
    } finally {
      setBusy(false);
    }
  };

  const stopClock = useCallback((): void => {
    if (timer.callbacks.stop) {
      timer.callbacks.stop();
    }
  }, [timer.callbacks]);

  const startMatchAndTimer = async (): Promise<void> => {
    await run(async () => {
      const started = await hockeyMatchService.start(match.id);
      await hockeyMatchService.setPeriod(started.id, 1);
      try {
        await hockeyMatchService.addPeriodScore(started.id, 1, 'RegularPeriod');
      } catch {
        /* period score may already exist */
      }
      const withPeriod = await hockeyMatchService.recordPeriodEvent(started.id, {
        periodNumber: 1,
        timeInSeconds: 0,
        action: 'PeriodStarted',
        description: 'PeriodStarted',
      });
      periodManagement.setStartedPeriods(new Set([1]));
      periodManagement.setNextPeriodToStart(2);
      timer.setCurrentPeriod(1);
      timer.setPeriodStartTime(1, 0);
      setShouldStartTimer(true);
      return withPeriod;
    });
  };

  const handlePeriodControlClick = (): void => {
    if (periodManagement.canEndPeriod()) {
      if (periodManagement.isInShootout() || timer.currentPeriod === periodManagement.maxPeriodNumber) {
        setShowFinishConfirm(true);
        return;
      }
      const durationSeconds = periodManagement.isInOvertime()
        ? DEFAULT_HOCKEY_MATCH_RULES.overtimeDurationMinutes * 60
        : DEFAULT_HOCKEY_MATCH_RULES.periodDurationMinutes * 60;
      if (getInPeriodSeconds() < durationSeconds) {
        periodManagement.setShowEndPeriodConfirmation(true);
        return;
      }
      void (async () => {
        await periodManagement.endPeriod();
        if (timer.callbacks.stop) {
          timer.callbacks.stop();
        }
      })();
      return;
    }

    void (async () => {
      const startingPeriod = periodManagement.nextPeriodToStart;
      const startMark = theoreticalPeriodStartSeconds(
        startingPeriod,
        periodManagement.overtimePeriodNumber,
        periodManagement.shootoutPeriodNumber,
        match.wentToOvertime,
      );
      if (startingPeriod > 0) {
        timer.setPeriodStartTime(startingPeriod, startMark);
        timer.setCurrentPeriod(startingPeriod);
      }
      if (startMark > 0) {
        try {
          await timerService.setTimer(match.id, startMark);
        } catch (err) {
          console.warn('Failed to align timer with period start mark:', err);
        }
      }
      await periodManagement.startPeriod();
      if (startingPeriod !== periodManagement.shootoutPeriodNumber) {
        if (timer.callbacks.start) {
          await timer.callbacks.start();
        } else if (timer.callbacks.toggle) {
          await timer.callbacks.toggle();
        }
      }
    })();
  };

  const home = hockeyHomeTeam(match);
  const away = hockeyAwayTeam(match);
  const leftSide = isSidesSwapped ? away : home;
  const rightSide = isSidesSwapped ? home : away;
  const leftName = leftSide ? teamNames.get(leftSide.teamId) ?? 'TBD' : 'TBD';
  const rightName = rightSide ? teamNames.get(rightSide.teamId) ?? 'TBD' : 'TBD';
  const selectedSide = match.matchTeams.find((side) => side.id === selectedTeamId);
  const selectedTeamName = selectedSide ? teamNames.get(selectedSide.teamId) ?? '' : '';
  const formPlayers = toFormPlayers(selectedSide?.activePlayers ?? [], playerNames);
  const homeFormPlayers = toFormPlayers(home?.activePlayers ?? [], playerNames);
  const awayFormPlayers = toFormPlayers(away?.activePlayers ?? [], playerNames);
  const finished = isHockeyMatchFinished(match.status);
  const canRecord = isHockeyMatchLive(match.status);
  const homeTeamEntity = teams.find((item) => item.id === home?.teamId);
  const awayTeamEntity = teams.find((item) => item.id === away?.teamId);
  const homeGoalieId = home?.activeGoalieMatchPlayerId ?? home?.activePlayers.find((player) => player.isGoalie)?.id ?? '';
  const awayGoalieId = away?.activeGoalieMatchPlayerId ?? away?.activePlayers.find((player) => player.isGoalie)?.id ?? '';

  const openEventForm = (kind: EventFormKind, side: HockeyMatchTeamDto | undefined): void => {
    if (!side) {
      return;
    }
    captureEventStamp();
    if (kind === 'goal' || kind === 'penalty') {
      stopClock();
    }
    setSelectedTeamId(side.id);
    if (playerId && !side.activePlayers.some((player) => player.id === playerId)) {
      setPlayerId('');
    }
    setAssistId('');
    setSecondaryAssistId('');
    setEventForm(kind);
  };

  const resumeClock = useCallback((): void => {
    if (timer.callbacks.start) {
      void timer.callbacks.start();
      return;
    }
    if (timer.callbacks.toggle && !timer.isRunning) {
      void timer.callbacks.toggle();
    }
  }, [timer.callbacks, timer.isRunning]);

  const openFaceoffForm = (): void => {
    captureEventStamp();
    resumeClock();
    const defaultWinner = home?.id ?? away?.id ?? '';
    setFaceoffWinnerId((current) => current || defaultWinner);
    setFaceoffWinnerPlayerId('');
    setFaceoffLoserPlayerId('');
    setEventForm('faceoff');
  };

  const closeEventForm = (): void => {
    clearEventStamp();
    setEventForm(null);
  };

  const submitEvent = async (): Promise<void> => {
    const stamp = eventStampRef.current;
    const periodNumber = stamp?.periodNumber ?? timer.currentPeriod;
    const timeInSeconds = stamp?.timeInSeconds ?? getInPeriodSeconds();
    if (eventForm === 'goal' && playerId) {
      const defendingGoalieId = hockeyOpposingGoalieId(match, selectedTeamId);
      await run(() => hockeyMatchService.recordGoal(match.id, {
        scoringMatchTeamId: selectedTeamId,
        scorerActivePlayerId: playerId,
        periodNumber,
        timeInSeconds,
        goalStrength,
        primaryAssistActivePlayerId: assistId || undefined,
        secondaryAssistActivePlayerId: secondaryAssistId || undefined,
        goalieActivePlayerId: defendingGoalieId || undefined,
        wasEmptyNet: !defendingGoalieId,
      }));
    } else if (eventForm === 'penalty') {
      await run(() => hockeyMatchService.recordPenalty(match.id, {
        penaltyMatchTeamId: selectedTeamId,
        periodNumber,
        timeInSeconds,
        severity: penaltySeverity,
        offence: penaltyOffence,
        penaltyMinutes,
        penalizedActivePlayerId: playerId || undefined,
      }));
    } else if (eventForm === 'shot') {
      const defendingGoalieId = hockeyOpposingGoalieId(match, selectedTeamId);
      const creditsSave = hockeyShotCreditsGoalieSave(shotResult);
      if (creditsSave && !defendingGoalieId) {
        setError(t('hockey.matches.shotNeedsGoalie', 'A save requires an opposing goalie in the lineup.'));
        return;
      }
      await run(() => hockeyMatchService.recordShot(match.id, {
        shootingMatchTeamId: selectedTeamId,
        periodNumber,
        timeInSeconds,
        shotResult,
        countsAsShotOnGoal: hockeyShotIsOnGoal(shotResult),
        shooterActivePlayerId: playerId || undefined,
        goalieActivePlayerId: creditsSave ? (defendingGoalieId || undefined) : undefined,
        description: shotResult,
      }));
    } else if (eventForm === 'faceoff' && faceoffWinnerId) {
      const losingMatchTeamId = faceoffWinnerId === home?.id ? away?.id : home?.id;
      if (!losingMatchTeamId) {
        setError(t('hockey.matches.assignTeamsFirst', 'Assign both teams before starting'));
        return;
      }
      await run(() => hockeyMatchService.recordFaceoff(match.id, {
        winningMatchTeamId: faceoffWinnerId,
        losingMatchTeamId,
        periodNumber,
        timeInSeconds,
        zone: faceoffZone,
        spot: faceoffSpot,
        winningActivePlayerId: faceoffWinnerPlayerId || undefined,
        losingActivePlayerId: faceoffLoserPlayerId || undefined,
        description: `${faceoffZone} ${faceoffSpot}`,
      }));
    }
    clearEventStamp();
    setEventForm(null);
  };

  const recordOffside = async (): Promise<void> => {
    const stamp = captureEventStamp();
    stopClock();
    await run(() => hockeyMatchService.recordStoppage(match.id, {
      periodNumber: stamp.periodNumber,
      timeInSeconds: stamp.timeInSeconds,
      reason: 'Offside',
      description: 'Offside',
    }));
    clearEventStamp();
  };

  const deleteEvent = async (eventId: string, eventType: string): Promise<void> => {
    const type = eventType.toLowerCase();
    if (type.includes('goal')) {
      await run(() => hockeyMatchService.deleteGoal(match.id, eventId));
    } else if (type.includes('penalty')) {
      await run(() => hockeyMatchService.deletePenalty(match.id, eventId));
    } else if (type.includes('shot')) {
      await run(() => hockeyMatchService.deleteShot(match.id, eventId));
    }
  };

  const teamNamesByMatchTeamId = useMemo(() => {
    const names = new Map<string, string>();
    for (const side of match.matchTeams) {
      names.set(side.id, teamNames.get(side.teamId) ?? side.teamId.slice(0, 8));
    }
    return names;
  }, [match.matchTeams, teamNames]);

  const selectedOfficialIds = match.officials.map((item) => item.officialId);
  const officialOptions = officials.map((item) => ({
    id: item.id,
    name: `${officialNames.get(item.personId) ?? item.id.slice(0, 8)} (${item.officialRole})`,
  }));

  return (
    <>
      <ErrorPopup message={error} />
      <LiveMatchModalHeader
        homeTeam={{ name: home ? teamNames.get(home.teamId) ?? 'Home' : 'Home' }}
        awayTeam={{ name: away ? teamNames.get(away.teamId) ?? 'Away' : 'Away' }}
        currentMatch={match}
        isSidesSwapped={isSidesSwapped}
        onToggleSides={() => setIsSidesSwapped((prev) => !prev)}
        onClose={onClose}
        onCompleteLive={() => setShowFinishConfirm(true)}
        onReopen={() => setShowReopenConfirm(true)}
      />
      <LiveMatchScoreboard
        leftTeam={{ name: leftName }}
        rightTeam={{ name: rightName }}
        leftScore={leftSide?.goals ?? 0}
        rightScore={rightSide?.goals ?? 0}
      />
      <div className="modal-content">
        <div className="left-section">
          <LiveMatchTimer
            currentMatch={match}
            isOpen
            loading={busy}
            startedPeriods={periodManagement.startedPeriods}
            endedPeriods={periodManagement.endedPeriods}
            nextPeriodToStart={periodManagement.nextPeriodToStart}
            periodLoading={periodManagement.periodLoading}
            onStartMatch={startMatchAndTimer}
            onPeriodControlClick={handlePeriodControlClick}
            canEndPeriod={periodManagement.canEndPeriod}
            getPeriodControlButtonText={periodManagement.getPeriodControlButtonText}
            keybindsEnabled={!eventForm && !isLineupDialogOpen}
            isStartMatchDisabled={!homeGoalieId || !awayGoalieId || !match.homeTeamId || !match.awayTeamId}
            startDisabledReason={
              !match.homeTeamId || !match.awayTeamId
                ? t('hockey.matches.assignTeamsFirst', 'Assign both teams before starting')
                : (!homeGoalieId || !awayGoalieId)
                  ? t('hockey.matches.selectGoalies', 'Select goalies to start')
                  : undefined
            }
            overtimePeriodNumber={periodManagement.overtimePeriodNumber}
            shootoutPeriodNumber={periodManagement.shootoutPeriodNumber}
          />
          <LiveMatchQuickActions
            loading={busy}
            currentMatch={match}
            leftTeamId={leftSide?.id}
            rightTeamId={rightSide?.id}
            leftTeamName={leftName}
            rightTeamName={rightName}
            onShowGoalForm={(teamId) => openEventForm('goal', match.matchTeams.find((side) => side.id === teamId))}
            onShowPenaltyForm={(teamId) => openEventForm('penalty', match.matchTeams.find((side) => side.id === teamId))}
            onShowShotForm={(teamId) => openEventForm('shot', match.matchTeams.find((side) => side.id === teamId))}
            onShowFaceoffForm={openFaceoffForm}
            onRecordOffside={() => void recordOffside()}
            keybindsEnabled={!eventForm && !isLineupDialogOpen && canRecord}
          />
          <ActiveRosterCard
            leftTeamName={leftName}
            rightTeamName={rightName}
            leftPlayers={leftSide?.activePlayers ?? []}
            rightPlayers={rightSide?.activePlayers ?? []}
            playerNames={playerNames}
            leftGoalieId={isSidesSwapped ? awayGoalieId : homeGoalieId}
            rightGoalieId={isSidesSwapped ? homeGoalieId : awayGoalieId}
            onEditLineup={() => setIsLineupDialogOpen(true)}
            selectedPlayerId={playerId}
            onSelectPlayer={(id, side) => {
              const matchTeam = side === 'left' ? leftSide : rightSide;
              if (matchTeam) {
                setSelectedTeamId(matchTeam.id);
                setPlayerId(id);
              }
            }}
            disabled={finished || match.status === 'Cancelled'}
          />
        </div>
        <div className="right-section">
          <LiveMatchEventsHistory
            events={match.events}
            teamNamesByMatchTeamId={teamNamesByMatchTeamId}
            playerNames={playerNames}
            canDelete={canRecord}
            busy={busy}
            onDeleteEvent={(eventItem) => void deleteEvent(eventItem.id, eventItem.eventType)}
          />
          <OfficialsSelectorSection
            selectedOfficials={
              selectedOfficialIds.length === 0
                ? ['']
                : showOfficialDraft
                  ? [...selectedOfficialIds, '']
                  : selectedOfficialIds
            }
            options={officialOptions}
            saving={busy}
            disabled={finished || match.status === 'Cancelled'}
            onAddRow={() => setShowOfficialDraft(true)}
            onSelect={(_index, officialId) => {
              if (!officialId || selectedOfficialIds.includes(officialId)) {
                return;
              }
              const profile = officials.find((item) => item.id === officialId);
              setShowOfficialDraft(false);
              void run(() => hockeyMatchService.addOfficial(match.id, officialId, (profile?.officialRole ?? 'Referee') as HockeyOfficialRole, profile?.officialRole === 'Referee'));
            }}
            onRemove={(_index, officialId) => {
              if (officialId) {
                void run(() => hockeyMatchService.removeOfficial(match.id, officialId));
              } else {
                setShowOfficialDraft(false);
              }
            }}
          />
        </div>
      </div>

      <GoalRecordingForm
        showGoalForm={eventForm === 'goal'}
        teamName={selectedTeamName}
        players={formPlayers}
        playerId={playerId}
        assistId={assistId}
        secondaryAssistId={secondaryAssistId}
        goalStrength={goalStrength}
        loading={busy}
        onPlayerChange={setPlayerId}
        onAssistChange={setAssistId}
        onSecondaryAssistChange={setSecondaryAssistId}
        onStrengthChange={setGoalStrength}
        onRecordGoal={submitEvent}
        onClose={closeEventForm}
      />
      <PenaltyRecordingForm
        showPenaltyForm={eventForm === 'penalty'}
        teamName={selectedTeamName}
        players={formPlayers}
        playerId={playerId}
        penaltyOffence={penaltyOffence}
        penaltySeverity={penaltySeverity}
        penaltyMinutes={penaltyMinutes}
        loading={busy}
        onPlayerChange={setPlayerId}
        onOffenceChange={setPenaltyOffence}
        onSeverityChange={setPenaltySeverity}
        onMinutesChange={setPenaltyMinutes}
        onRecordPenalty={submitEvent}
        onClose={closeEventForm}
      />
      <ShotRecordingForm
        showShotForm={eventForm === 'shot'}
        teamName={selectedTeamName}
        players={formPlayers}
        playerId={playerId}
        shotResult={shotResult}
        loading={busy}
        onPlayerChange={setPlayerId}
        onResultChange={setShotResult}
        onRecordShot={submitEvent}
        onClose={closeEventForm}
      />
      <FaceoffRecordingForm
        showFaceoffForm={eventForm === 'faceoff'}
        homeTeamId={home?.id ?? ''}
        awayTeamId={away?.id ?? ''}
        homeTeamName={home ? teamNames.get(home.teamId) ?? t('hockey.matches.home', 'Home') : t('hockey.matches.home', 'Home')}
        awayTeamName={away ? teamNames.get(away.teamId) ?? t('hockey.matches.away', 'Away') : t('hockey.matches.away', 'Away')}
        homePlayers={homeFormPlayers}
        awayPlayers={awayFormPlayers}
        winningMatchTeamId={faceoffWinnerId}
        zone={faceoffZone}
        spot={faceoffSpot}
        winningPlayerId={faceoffWinnerPlayerId}
        losingPlayerId={faceoffLoserPlayerId}
        loading={busy}
        onWinnerChange={(teamId) => {
          setFaceoffWinnerId(teamId);
          setFaceoffWinnerPlayerId('');
          setFaceoffLoserPlayerId('');
        }}
        onZoneChange={(zone) => {
          setFaceoffZone(zone);
          const nextSpots = HOCKEY_FACEOFF_SPOTS_BY_ZONE[zone];
          setFaceoffSpot(nextSpots.includes(faceoffSpot) ? faceoffSpot : nextSpots[0]);
        }}
        onSpotChange={setFaceoffSpot}
        onWinningPlayerChange={setFaceoffWinnerPlayerId}
        onLosingPlayerChange={setFaceoffLoserPlayerId}
        onRecordFaceoff={submitEvent}
        onClose={closeEventForm}
      />
      <EditActiveRosterDialog
        isOpen={isLineupDialogOpen}
        match={match}
        homeTeam={homeTeamEntity}
        awayTeam={awayTeamEntity}
        playerNames={playerNames}
        onClose={() => setIsLineupDialogOpen(false)}
        onSaved={setMatch}
        onError={setError}
      />
      <ConfirmationDialog
        isOpen={periodManagement.showEndPeriodConfirmation}
        icon="⏱️"
        title={t('hockey.matches.endPeriodEarlyTitle', 'End period early?')}
        message={t('hockey.matches.endPeriodEarlyConfirm', 'The configured period length has not been reached yet. End the period anyway?')}
        confirmText={t('hockey.matches.endPeriod', 'End period')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={busy || Boolean(periodManagement.periodLoading[timer.currentPeriod])}
        onConfirm={() => {
          void (async () => {
            await periodManagement.endPeriod();
            if (timer.callbacks.stop) {
              timer.callbacks.stop();
            }
            periodManagement.setShowEndPeriodConfirmation(false);
          })();
        }}
        onCancel={() => periodManagement.setShowEndPeriodConfirmation(false)}
      />
      <ConfirmationDialog
        isOpen={showFinishConfirm}
        icon="⏹️"
        title={t('hockey.matches.finishTitle', 'Finish match')}
        message={t('hockey.matches.finishConfirm', 'Mark this match as finished and recalculate statistics?')}
        confirmText={t('hockey.matches.finish', 'Finish')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={busy}
        onConfirm={() => {
          void run(async () => {
            const finishedMatch = await hockeyMatchService.finish(match.id);
            try {
              await hockeyStatisticsService.recalculateMatch(match.id);
            } catch {
              /* stats recalculation is best-effort */
            }
            setShowFinishConfirm(false);
            return finishedMatch;
          });
        }}
        onCancel={() => setShowFinishConfirm(false)}
      />
      <ConfirmationDialog
        isOpen={showReopenConfirm}
        icon="🔓"
        title={t('hockey.matches.manage.reopenMatch', 'Open match')}
        message={t('hockey.matches.manage.reopenConfirm', 'Reopen this match for live recording?')}
        confirmText={t('hockey.matches.manage.reopenMatch', 'Open match')}
        cancelText={t('common.cancel', 'Cancel')}
        isLoading={busy}
        onConfirm={() => {
          void run(async () => {
            const reopened = await hockeyMatchService.setStatus(match.id, 'InProgress');
            setShowReopenConfirm(false);
            return reopened;
          });
        }}
        onCancel={() => setShowReopenConfirm(false)}
      />
    </>
  );
}

function ManageHockeyMatchPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { matchId } = useParams<{ matchId: string }>();
  const [searchParams] = useSearchParams();
  const [match, setMatch] = useState<HockeyMatchDto | null>(null);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [homeTeamId, setHomeTeamId] = useState('');
  const [awayTeamId, setAwayTeamId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [assigning, setAssigning] = useState(false);

  const returnTo = searchParams.get('returnTo') || '/admin/hockey/matches';

  useEffect(() => {
    if (!matchId) {
      setError('Match ID is missing');
      setLoading(false);
      return;
    }
    const fetchMatch = async (): Promise<void> => {
      try {
        const [loaded, teamList] = await Promise.all([
          hockeyMatchService.getById(matchId),
          hockeyTeamService.getAll(),
        ]);
        setMatch(loaded);
        setTeams(teamList);
        setHomeTeamId(loaded.homeTeamId ?? '');
        setAwayTeamId(loaded.awayTeamId ?? '');
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred while fetching match data.');
      } finally {
        setLoading(false);
      }
    };
    void fetchMatch();
  }, [matchId]);

  if (loading) {
    return <div>{t('common.loading', 'Loading...')}</div>;
  }

  if (error && !match) {
    return (
      <div className="manage-match-page">
        <ErrorPopup message={error} />
      </div>
    );
  }

  if (!match) {
    return <div>Match not found.</div>;
  }

  const isTeamsAssignable = match.status === 'Scheduled' || match.status === 'Postponed';
  const isMissingTeams = !match.homeTeamId || !match.awayTeamId;

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
                  onClick={() => navigate(`/admin/hockey/matches/${match.id}/edit`)}
                >
                  <span className="edit-match-button__icon" aria-hidden="true">👥</span>
                  <span className="edit-match-button__label">
                    {isMissingTeams
                      ? t('hockey.matches.assignTeams.actionMissing', 'Assign teams')
                      : t('hockey.matches.assignTeams.actionChange', 'Change teams')}
                  </span>
                </button>
              )}
              {!isHockeyMatchFinished(match.status) && (
                <button
                  type="button"
                  className="edit-match-button"
                  onClick={() => navigate(`/admin/hockey/matches/${match.id}/edit`)}
                >
                  <span className="edit-match-button__icon" aria-hidden="true">✏️</span>
                  <span className="edit-match-button__label">{t('hockey.matches.actions.edit', 'Edit')}</span>
                </button>
              )}
            </div>
          </div>
          {isMissingTeams && isTeamsAssignable && (
            <div className="page-header__missing-teams" role="status">
              <i className="fas fa-info-circle" aria-hidden="true"></i>
              {t('hockey.matches.assignTeams.missingBanner', 'This match does not have both teams yet. Assign teams before starting.')}
            </div>
          )}
        </div>
        {isMissingTeams ? (
          <div className="manage-match-page__placeholder">
            <i className="fas fa-users-slash" aria-hidden="true"></i>
            <p>{t('hockey.matches.assignTeams.placeholderBody', 'Both teams must be assigned before the live desk can open.')}</p>
            <div className="form-row" style={{ display: 'flex', gap: '8px', flexWrap: 'wrap', justifyContent: 'center' }}>
              <select value={homeTeamId} onChange={(event) => setHomeTeamId(event.target.value)}>
                <option value="">{t('hockey.matches.homeTeamPlaceholder', 'Select home team')}</option>
                {teams.map((team) => <option key={team.id} value={team.id}>{team.name}</option>)}
              </select>
              <select value={awayTeamId} onChange={(event) => setAwayTeamId(event.target.value)}>
                <option value="">{t('hockey.matches.awayTeamPlaceholder', 'Select away team')}</option>
                {teams.map((team) => <option key={team.id} value={team.id}>{team.name}</option>)}
              </select>
            </div>
            <button
              type="button"
              className="manage-match-page__placeholder-action"
              disabled={!homeTeamId || !awayTeamId || homeTeamId === awayTeamId || assigning}
              onClick={() => {
                void (async () => {
                  setAssigning(true);
                  try {
                    setMatch(await hockeyMatchService.assignTeams(match.id, homeTeamId, awayTeamId));
                  } catch (err) {
                    setError(err instanceof Error ? err.message : 'Failed to assign teams');
                  } finally {
                    setAssigning(false);
                  }
                })();
              }}
            >
              <i className="fas fa-user-plus" aria-hidden="true"></i>
              {t('hockey.matches.assignTeams.action', 'Assign teams')}
            </button>
          </div>
        ) : (
          <MatchTimerProvider matchId={match.id} initialPeriod={match.currentPeriodNumber || 1}>
            <ManageHockeyMatchContent match={match} setMatch={setMatch} onClose={() => navigate(returnTo)} />
          </MatchTimerProvider>
        )}
      </div>
    </PageTemplate>
  );
}

export default ManageHockeyMatchPage;
