import { useEffect, useCallback, useRef } from 'react';
import { signalRService, type MatchEvent } from '../services/signalRService';

interface UseSignalROptions {
  eventTypes?: string[];
  onEvent?: (event: MatchEvent) => void;
  autoConnect?: boolean;
}

export function useSignalR(options: UseSignalROptions = {}) {
  const { eventTypes = [], onEvent, autoConnect = true } = options;
  const unsubscribeRef = useRef<(() => void) | null>(null);
  const subscribedEventsRef = useRef<Set<string>>(new Set());

  const connect = useCallback(async () => {
    try {
      await signalRService.connect();
      console.log('SignalR connected');
      return true;
    } catch (error) {
      console.error('Failed to connect to SignalR:', error);
      return false;
    }
  }, []);

  const disconnect = useCallback(async () => {
    try {
      // Unsubscribe from events
      for (const eventType of subscribedEventsRef.current) {
        await signalRService.unsubscribeFromEventType(eventType);
      }
      subscribedEventsRef.current.clear();

      // Remove event listener
      if (unsubscribeRef.current) {
        unsubscribeRef.current();
        unsubscribeRef.current = null;
      }

      await signalRService.disconnect();
      console.log('SignalR disconnected');
    } catch (error) {
      console.error('Error disconnecting SignalR:', error);
    }
  }, []);

  const subscribeToEvent = useCallback(async (eventType: string) => {
    try {
      await signalRService.subscribeToEventType(eventType);
      subscribedEventsRef.current.add(eventType);
      console.log(`Subscribed to: ${eventType}`);
    } catch (error) {
      console.error(`Failed to subscribe to ${eventType}:`, error);
    }
  }, []);

  const unsubscribeFromEvent = useCallback(async (eventType: string) => {
    try {
      await signalRService.unsubscribeFromEventType(eventType);
      subscribedEventsRef.current.delete(eventType);
      console.log(`Unsubscribed from: ${eventType}`);
    } catch (error) {
      console.error(`Failed to unsubscribe from ${eventType}:`, error);
    }
  }, []);

  useEffect(() => {
    const setupSignalR = async () => {
      if (!autoConnect) return;

      // Connect to SignalR
      const connected = await connect();
      if (!connected) return;

      // Set up event listener
      if (onEvent) {
        unsubscribeRef.current = signalRService.onMatchEvent(onEvent);
      }

      // Subscribe to event types
      for (const eventType of eventTypes) {
        await subscribeToEvent(eventType);
      }
    };

    setupSignalR();

    // Cleanup on unmount
    return () => {
      disconnect();
    };
  }, [autoConnect, connect, disconnect, subscribeToEvent, onEvent, eventTypes]);

  return {
    connect,
    disconnect,
    subscribeToEvent,
    unsubscribeFromEvent,
    isConnected: signalRService.isConnected,
    connectionState: signalRService.connectionState
  };
} 