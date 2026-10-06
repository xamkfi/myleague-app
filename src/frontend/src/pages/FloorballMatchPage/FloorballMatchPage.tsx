import { useParams } from 'react-router-dom';
import { useEffect, useState, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './FloorballMatchPage.scss';
import { signalRService, type MatchEvent } from '../../services/signalRService';
import { MATCH_NOTIFICATION_EVENTS } from '../../constants/MatchNotifications';
import {
  MatchPageShell,
  resolveTableTabVariant,
  type MatchTabType,
} from '../../components/match';
import MatchTabContent from './components/MatchTabContent';
import { getCompetitionPath, isTournamentCompetition } from '../../utils/competitionPath';
import { getTeamPath } from '../../utils/sportRoutes';
import { slugify } from '../../utils/slugUtils';

const LIVE_RELOAD_DEBOUNCE_MS = 400;

export default function FloorballMatchPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<MatchTabType>('summary');

  const latestRequestRef = useRef(0);

  const loadMatch = useCallback(async () => {
    if (!id) return;
    const requestId = ++latestRequestRef.current;
    try {
      const response = await floorballMatchService.getById(id);
      if (requestId !== latestRequestRef.current) return;
      setMatch(response.data);
    } catch (err) {
      if (requestId !== latestRequestRef.current) return;
      console.error(err);
      setError((err as Error).message);
    } finally {
      if (requestId === latestRequestRef.current) {
        setLoading(false);
      }
    }
  }, [id]);

  const isLive = match?.status === FloorballMatchStatus.InProgress;

  useEffect(() => {
    if (!id || !isLive) return;

    let unsubscribeCallback: (() => void) | null = null;
    let reloadTimer: ReturnType<typeof setTimeout> | null = null;
    const scheduleReload = () => {
      if (reloadTimer) clearTimeout(reloadTimer);
      reloadTimer = setTimeout(() => {
        reloadTimer = null;
        void loadMatch();
      }, LIVE_RELOAD_DEBOUNCE_MS);
    };

    const setupMatchSignalR = async () => {
      try {
        await signalRService.connect();
        await signalRService.subscribeToMatch(id);

        unsubscribeCallback = signalRService.onMatchEvent((evt: MatchEvent) => {
          switch (evt.eventType) {
            case MATCH_NOTIFICATION_EVENTS.GOAL_SCORED:
            case MATCH_NOTIFICATION_EVENTS.PENALTY_ASSIGNED:
            case MATCH_NOTIFICATION_EVENTS.SAVE_RECORDED:
            case MATCH_NOTIFICATION_EVENTS.MATCH_STARTED:
            case MATCH_NOTIFICATION_EVENTS.MATCH_COMPLETED:
              scheduleReload();
              break;
            default:
              break;
          }
        });
      } catch (signalRError) {
        console.error('Failed to setup SignalR for match:', signalRError);
      }
    };

    void setupMatchSignalR();

    return () => {
      if (reloadTimer) clearTimeout(reloadTimer);
      if (unsubscribeCallback) {
        unsubscribeCallback();
      }
      if (id) {
        signalRService.unsubscribeFromMatch(id).catch(console.error);
      }
    };
  }, [id, isLive, loadMatch]);

  useEffect(() => {
    const fetchMatch = async () => {
      if (!id) return;
      try {
        setLoading(true);
        await loadMatch();
      } catch (err) {
        console.error(err);
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    };

    void fetchMatch();
  }, [id, loadMatch]);

  const isTournament = match
    ? isTournamentCompetition({
        tournamentGroupId: match.tournamentGroupId,
        tournamentStage: match.tournamentStage,
      })
    : false;
  const tableVariant = resolveTableTabVariant(isTournament, match?.tournamentGroupId);

  return (
    <MatchPageShell
      isLoading={loading}
      error={error}
      competitionName={match?.competitionName}
      competitionPath={
        match
          ? getCompetitionPath(match.competitionId, {
              tournamentGroupId: match.tournamentGroupId,
              tournamentStage: match.tournamentStage,
            })
          : undefined
      }
      header={
        match
          ? {
              home: {
                name: match.homeTeamName,
                logo: match.homeTeamLogo,
                href: match.homeTeamName ? getTeamPath('floorball', slugify(match.homeTeamName)) : null,
              },
              away: {
                name: match.awayTeamName,
                logo: match.awayTeamLogo,
                href: match.awayTeamName ? getTeamPath('floorball', slugify(match.awayTeamName)) : null,
              },
              homeScore: match.homeScore,
              awayScore: match.awayScore,
              scheduledDateTime: match.scheduledDateTime,
              venue: match.venue,
              isScheduled: match.status === FloorballMatchStatus.Scheduled,
              isLive: match.status === FloorballMatchStatus.InProgress,
              isFinal: match.status === FloorballMatchStatus.Completed,
              statusLabel:
                match.status === FloorballMatchStatus.Postponed ||
                match.status === FloorballMatchStatus.Cancelled
                  ? t(`floorball.matches.status.${match.status}`)
                  : null,
            }
          : undefined
      }
      activeTab={activeTab}
      onTabChange={setActiveTab}
      tableVariant={tableVariant}
    >
      {match && <MatchTabContent activeTab={activeTab} match={match} />}
    </MatchPageShell>
  );
}
