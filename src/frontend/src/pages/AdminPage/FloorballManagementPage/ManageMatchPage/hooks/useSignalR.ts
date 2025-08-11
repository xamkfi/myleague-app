import { useCallback } from 'react';
import { signalRService, type MatchEvent } from '../../../../../services/signalRService';
import type { PeriodEventData, GoalEventData, PenaltyEventData, SaveEventData } from '../components/types';

interface UseSignalRProps {
  matchId: string;
  isOpen: boolean;
  onPeriodStarted: (eventData: PeriodEventData) => void;
  onGoalScored: (eventData: GoalEventData) => void;
  onPenaltyAssigned: (eventData: PenaltyEventData) => void;
  onSaveRecorded: (eventData: SaveEventData) => void;
}

export const useSignalR = ({
  matchId,
  onPeriodStarted,
  onGoalScored,
  onPenaltyAssigned,
  onSaveRecorded
}: UseSignalRProps) => {

  /**
   * Handles real-time SignalR events for this match
   * Filters events to only process those relevant to this match
   * Updates the UI immediately when events are received
   */
  const handleSignalREvent = useCallback((event: MatchEvent) => {
    const eventData = event.data as { MatchId?: string };
    
    if (eventData?.MatchId !== matchId) {
      return; // Event is not for this match
    }
    
    if (event.eventType === 'FloorballGoalScored') {
      onGoalScored(event.data as GoalEventData);
    } else if (event.eventType === 'FloorballPenaltyAssigned') {
      onPenaltyAssigned(event.data as PenaltyEventData);
    } else if (event.eventType === 'FloorballSaveEvent') {
      onSaveRecorded(event.data as SaveEventData);
    } else if (event.eventType === 'FloorballPeriodStartedEvent') {
      onPeriodStarted(event.data as PeriodEventData);
    }
  }, [matchId, onGoalScored, onPenaltyAssigned, onPeriodStarted, onSaveRecorded]);

  /**
   * Sets up SignalR connection for real-time updates
   * Subscribes to goal and penalty events for this specific match
   * This enables live updates when events are recorded
   */
  const setupSignalR = useCallback(async () => {
    try {
      // Test backend accessibility first
      const isBackendAccessible = await signalRService.testBackendAccessibility();
      if (!isBackendAccessible) {
        console.warn('Backend is not accessible, skipping SignalR setup');
        return;
      }
      
      // Connect to SignalR
      await signalRService.connect();
      
      // Wait a bit to ensure connection is stable
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Only subscribe if connection is established
      if (signalRService.isConnected) {
        // Subscribe to this specific match for all match-related events
        await signalRService.subscribeToMatch(matchId);
        
        // Also subscribe to specific event types for broader coverage
        await signalRService.subscribeToEventType('FloorballGoalScored');
        await signalRService.subscribeToEventType('FloorballPenaltyAssigned');
        await signalRService.subscribeToEventType('FloorballSaveEvent');
        await signalRService.subscribeToEventType('FloorballPeriodStartedEvent');
        
        const unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        return unsubscribe;
      } else {
        console.warn('SignalR connection not established, skipping event subscriptions');
      }
    } catch (error) {
      console.error('Error setting up SignalR:', error);
      // Don't throw - SignalR is not critical for basic functionality
    }
  }, [matchId, handleSignalREvent]);

  /**
   * Cleans up SignalR subscriptions when the modal is closed
   * This prevents memory leaks and unnecessary network traffic
   */
  const cleanupSignalR = useCallback(async () => {
    try {
      if (signalRService.isConnected) {
        // Unsubscribe from match-specific events
        await signalRService.unsubscribeFromMatch(matchId);
        
        // Unsubscribe from event types
        await signalRService.unsubscribeFromEventType('FloorballGoalScored');
        await signalRService.unsubscribeFromEventType('FloorballPenaltyAssigned');
        await signalRService.unsubscribeFromEventType('FloorballSaveEvent');
        await signalRService.unsubscribeFromEventType('FloorballPeriodStartedEvent');
      }
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
      // Don't throw - cleanup errors are not critical
    }
  }, [matchId]);

  return {
    setupSignalR,
    cleanupSignalR,
    handleSignalREvent
  };
}; 