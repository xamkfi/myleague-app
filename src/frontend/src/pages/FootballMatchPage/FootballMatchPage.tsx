import { useParams } from 'react-router-dom';
import { useEffect, useState, useCallback } from 'react';
import { footballMatchService } from '../../api/football/footballMatchService';
import { FootballMatchStatus, type FootballMatchDto } from '../../types/football/footballTypes';
import './FootballMatchPage.scss';
import { signalRService, type MatchEvent } from '../../services/signalRService';
import { FOOTBALL_MATCH_NOTIFICATION_EVENTS } from '../../constants/FootballMatchNotifications';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import MatchBreadcrumb from './components/MatchBreadcrumb';
import MatchHeader from './components/MatchHeader';
import MatchNavigation, { type TabType } from './components/MatchNavigation';
import MatchTabContent from './components/MatchTabContent';
import { isFootballTournamentCompetition } from '../../utils/footballCompetitionPath';



export default function FootballMatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FootballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('summary');

  // Helper that loads the latest state of the match from the API.
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

  // SignalR integration — only for live (InProgress) matches
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
      } catch (error) {
        console.error('Failed to setup SignalR for match:', error);
      }
    };

    setupMatchSignalR();

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

    fetchMatch();
  }, [id, loadMatch]);

  if (loading) {
    return (
      <div className="match-page">
        <div className="loading">Loading match...</div>
      </div>
    );
  }

  if (error || !match) {
    return (
      <div className="match-page">
        <div className="error">{error || 'Match not found'}</div>
      </div>
    );
  }



  // Pick a variant for the "table" tab label based on the match type so the standings tab
  // reads naturally in tournament context.
  const isTournament: boolean = isFootballTournamentCompetition({
    competitionType: match.competitionType,
    tournamentGroupId: match.tournamentGroupId,
    tournamentStage: match.tournamentStage,
  });
  const tableVariant: 'season' | 'tournamentGroup' | 'tournamentPlayoff' = isTournament
    ? match.tournamentGroupId
      ? 'tournamentGroup'
      : 'tournamentPlayoff'
    : 'season';

  return (
    <div className="match-page-wrapper">
      <PageTemplate title="Match Details">
        <div className="match-page">
          <MatchBreadcrumb
            competitionName={match.competitionName}
            competitionId={match.competitionId}
            hints={{
              competitionType: match.competitionType,
              tournamentGroupId: match.tournamentGroupId,
              tournamentStage: match.tournamentStage,
            }}
          />
          <MatchHeader match={match} />
          <MatchNavigation
            activeTab={activeTab}
            onTabChange={setActiveTab}
            tableVariant={tableVariant}
          />
          <MatchTabContent activeTab={activeTab} match={match} />
        </div>
      </PageTemplate>
    </div>
  );
}