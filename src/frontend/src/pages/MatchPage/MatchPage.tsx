import { useParams } from 'react-router-dom';
import { useEffect, useState, useCallback } from 'react';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './MatchPage.scss';
import { signalRService, type MatchEvent } from '../../services/signalRService';
import { MATCH_NOTIFICATION_EVENTS } from '../../constants/MatchNotifications';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import MatchBreadcrumb from './components/MatchBreadcrumb';
import MatchHeader from './components/MatchHeader';
import MatchNavigation, { type TabType } from './components/MatchNavigation';
import MatchTabContent from './components/MatchTabContent';



export default function MatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('summary');

  // Helper that loads the latest state of the match from the API.
  const loadMatch = useCallback(async () => {
    if (!id) return;
    try {
      const response = await floorballMatchService.getById(id);
      setMatch(response.data);
    } catch (err) {
      console.error(err);
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }, [id]);

  // SignalR integration — only for live (InProgress) matches
  const isLive = match?.status === FloorballMatchStatus.InProgress;

  useEffect(() => {
    if (!id || !isLive) return;

    let unsubscribeCallback: (() => void) | null = null;

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



  return (
    <div className="match-page-wrapper">
      <PageTemplate title="Match Details">
        <div className="match-page">
          <MatchBreadcrumb 
            seasonName={match.seasonName}
            competitionId={match.competitionId}
          />
          <MatchHeader match={match} />
          <MatchNavigation activeTab={activeTab} onTabChange={setActiveTab} />
          <MatchTabContent activeTab={activeTab} match={match} />
        </div>
      </PageTemplate>
    </div>
  );
}