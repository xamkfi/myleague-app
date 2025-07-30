import { useCallback } from 'react';
import { signalRService, type MatchEvent } from '../../../../../../../services/signalRService';
import type { PeriodEventData, GoalEventData, PenaltyEventData } from '../types';

interface UseSignalRProps {
  matchId: string;
  isOpen: boolean;
  onPeriodStarted: (eventData: PeriodEventData) => void;
  onGoalScored: (eventData: GoalEventData) => void;
  onPenaltyAssigned: (eventData: PenaltyEventData) => void;
}

export const useSignalR = ({
  matchId,
  onPeriodStarted,
  onGoalScored,
  onPenaltyAssigned
}: UseSignalRProps) => {

  /**
   * Handles real-time SignalR events for this match
   * Filters events to only process those relevant to this match
   * Updates the UI immediately when events are received
   */
  const handleSignalREvent = useCallback((event: MatchEvent) => {
    console.log('Received match event:', event);
    console.log('Event type:', event.eventType);
    console.log('Event data:', event.data);
    
    const eventData = event.data as { MatchId?: string };
    console.log('Extracted MatchId from event data:', eventData?.MatchId);
    console.log('Current match ID:', matchId);
    
    if (eventData?.MatchId !== matchId) {
      console.log('Event is not for this match, ignoring');
      return; // Event is not for this match
    }
    
    console.log('Processing event for this match');
    
    // IGNORE timer events - let the Timer component handle them
    if (event.eventType === 'TimerUpdateEvent') {
      console.log('Ignoring timer event - Timer component will handle it');
      return;
    }
    
    if (event.eventType === 'FloorballGoalScored') {
      console.log('Handling goal scored event');
      onGoalScored(event.data as GoalEventData);
    } else if (event.eventType === 'FloorballPenaltyAssigned') {
      console.log('Handling penalty assigned event');
      onPenaltyAssigned(event.data as PenaltyEventData);
    } else if (event.eventType === 'FloorballPeriodStartedEvent') {
      console.log('Handling period started event');
      onPeriodStarted(event.data as PeriodEventData);
    } else {
      console.log('Unknown event type:', event.eventType);
    }
  }, [matchId, onGoalScored, onPenaltyAssigned, onPeriodStarted]);

  /**
   * Sets up SignalR connection for real-time updates
   * Subscribes to goal and penalty events for this specific match
   * This enables live updates when events are recorded
   */
  const setupSignalR = useCallback(async () => {
    try {
      console.log('Setting up SignalR connection...');
      
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
        console.log('SignalR connected, subscribing to match events...');
        
        // Subscribe to this specific match for all match-related events
        await signalRService.subscribeToMatch(matchId);
        
        // Also subscribe to specific event types for broader coverage
        await signalRService.subscribeToEventType('FloorballGoalScored');
        await signalRService.subscribeToEventType('FloorballPenaltyAssigned');
        await signalRService.subscribeToEventType('FloorballPeriodStartedEvent');
        
        const unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        console.log('SignalR setup completed successfully');
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