import { useParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import './MatchPage.scss';
import { signalRService, type MatchEvent } from '../../services/signalRService';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import MatchBreadcrumb from './components/MatchBreadcrumb';
import MatchHeader from './components/MatchHeader';
import MatchNavigation, { type TabType } from './components/MatchNavigation';
import MatchTabContent from './components/MatchTabContent';

// SignalR event names used by the backend. Consider centralising these to a constants file if reused elsewhere.
const GOAL_SCORED_EVENT = 'FloorballGoalScored';
const PENALTY_ASSIGNED_EVENT = 'FloorballPenaltyAssigned';



export default function MatchPage() {
  const { id } = useParams<{ id: string }>();
  const [match, setMatch] = useState<FloorballMatchDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('summary');

  // Helper that loads the latest state of the match from the API.
  const loadMatch = async () => {
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
  };

  // SignalR integration for match-specific events
  useEffect(() => {
    if (!id) return;

    let unsubscribeCallback: (() => void) | null = null;

    const setupMatchSignalR = async () => {
      try {
        // Connect to SignalR and wait for connection to be established
        await signalRService.connect();
        
        // Wait a bit for connection to stabilize
        await new Promise(resolve => setTimeout(resolve, 100));
        
        // Check if we're actually connected before subscribing
        if (!signalRService.isConnected) {
          throw new Error('SignalR connection not established');
        }
        
        // Subscribe to this specific match
        await signalRService.subscribeToMatch(id);
        
        // Listen for match-specific events
        unsubscribeCallback = signalRService.onMatchEvent((evt: MatchEvent) => {
          // These are events specific to our match, no need to filter by matchId
          if (evt.eventType === GOAL_SCORED_EVENT || evt.eventType === PENALTY_ASSIGNED_EVENT) {
            console.log(`Received match event for match ${id}:`, evt);
            // Simply reload the match so UI stays consistent
            loadMatch();
          }
        });
        
        console.log(`Successfully subscribed to match ${id} events`);
      } catch (error) {
        console.error('Failed to setup SignalR for match:', error);
      }
    };

    setupMatchSignalR();

    // Cleanup on unmount or when match ID changes
    return () => {
      if (unsubscribeCallback) {
        unsubscribeCallback();
      }
      if (id) {
        signalRService.unsubscribeFromMatch(id).catch(console.error);
      }
    };
  }, [id, loadMatch]);

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
  }, [id]);

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
    <PageTemplate title="Match Details">
      <div className="match-page">
        <MatchBreadcrumb />
        <MatchHeader match={match} />
        <MatchNavigation activeTab={activeTab} onTabChange={setActiveTab} />
        <MatchTabContent activeTab={activeTab} match={match} />
      </div>
    </PageTemplate>
  );
}