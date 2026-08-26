import { useParams } from 'react-router-dom';
import { useEffect, useState, useCallback } from 'react';
import { footballMatchService } from '../../api/football/footballMatchService';
import { FootballMatchStatus, type FootballMatchDto } from '../../types/football/footballTypes';
import './FootballMatchPage.scss';
import { signalRService, type MatchEvent } from '../../services/signalRService';
import { FOOTBALL_MATCH_NOTIFICATION_EVENTS } from '../../constants/FootballMatchNotifications';
import {
  MatchPageShell,
  resolveTableTabVariant,
  type MatchTabType,
} from '../../components/match';
import MatchTabContent from './components/MatchTabContent';
import {
  getFootballCompetitionPath,
  isFootballTournamentCompetition,
} from '../../utils/footballCompetitionPath';
import { slugify } from '../../utils/slugUtils';

export default function FootballMatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FootballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<MatchTabType>('summary');

  const loadMatch = useCallback(async () => {
    if (!id) return;
    try {
      const response = await footballMatchService.getById(id);
      setMatch(response.data);
    } catch (err) {
      console.error(err);
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }, [id]);

  const isLive = match?.status === FootballMatchStatus.InProgress;

  useEffect(() => {
    if (!id || !isLive) return;

    let unsubscribeCallback: (() => void) | null = null;

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
              loadMatch();
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
                href: match.homeTeamName ? `/football/team/${slugify(match.homeTeamName)}` : null,
              },
              away: {
                name: match.awayTeamName,
                logo: match.awayTeamLogo,
                href: match.awayTeamName ? `/football/team/${slugify(match.awayTeamName)}` : null,
              },
              homeScore: match.homeScore,
              awayScore: match.awayScore,
              scheduledDateTime: match.scheduledDateTime,
              isScheduled: match.status === FootballMatchStatus.Scheduled,
              isLive: match.status === FootballMatchStatus.InProgress,
              isFinal: match.status === FootballMatchStatus.Completed,
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
