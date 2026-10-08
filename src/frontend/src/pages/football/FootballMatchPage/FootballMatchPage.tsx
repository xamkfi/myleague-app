import { useParams } from 'react-router-dom';
import { useEffect, useState, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { footballMatchService } from '../../../api/football/footballMatchService';
import { FootballMatchStatus, type FootballMatchDto } from '../../../types/football/footballTypes';
import './FootballMatchPage.scss';
import { signalRService, type MatchEvent } from '../../../services/signalRService';
import { FOOTBALL_MATCH_NOTIFICATION_EVENTS } from '../../../constants/FootballMatchNotifications';
import {
  MatchPageShell,
  resolveTableTabVariant,
  type MatchTabType,
} from '../../../components/match';
import MatchTabContent from './components/MatchTabContent';
import {
  getFootballCompetitionPath,
  isFootballTournamentCompetition,
} from '../../../utils/footballCompetitionPath';
import { getTeamPath } from '../../../utils/sportRoutes';
import { slugify } from '../../../utils/slugUtils';

const LIVE_RELOAD_DEBOUNCE_MS = 400;

export default function FootballMatchPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FootballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<MatchTabType>('summary');

  const latestRequestRef = useRef(0);

  const loadMatch = useCallback(async () => {
    if (!id) return;
    const requestId = ++latestRequestRef.current;
    try {
      const response = await footballMatchService.getById(id);
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

  const isLive = match?.status === FootballMatchStatus.InProgress;

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
            case FOOTBALL_MATCH_NOTIFICATION_EVENTS.GOAL_SCORED:
            case FOOTBALL_MATCH_NOTIFICATION_EVENTS.CARD_ASSIGNED:
            case FOOTBALL_MATCH_NOTIFICATION_EVENTS.SUBSTITUTION_RECORDED:
            case FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_STARTED:
            case FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_COMPLETED:
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
    ? isFootballTournamentCompetition({
        competitionType: match.competitionType,
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
          ? getFootballCompetitionPath(match.competitionId, {
              competitionType: match.competitionType,
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
                href: match.homeTeamName ? getTeamPath('football', slugify(match.homeTeamName)) : null,
              },
              away: {
                name: match.awayTeamName,
                logo: match.awayTeamLogo,
                href: match.awayTeamName ? getTeamPath('football', slugify(match.awayTeamName)) : null,
              },
              homeScore: match.homeScore,
              awayScore: match.awayScore,
              scheduledDateTime: match.scheduledDateTime,
              venue: match.venue,
              isScheduled: match.status === FootballMatchStatus.Scheduled,
              isLive: match.status === FootballMatchStatus.InProgress,
              isFinal: match.status === FootballMatchStatus.Completed,
              statusLabel:
                match.status === FootballMatchStatus.Postponed ||
                match.status === FootballMatchStatus.Cancelled
                  ? t(`football.matches.status.${match.status}`, match.status)
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
