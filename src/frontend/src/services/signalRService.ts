import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';

export interface MatchEvent {
  eventType: string;
  data: unknown;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private matchEventCallbacks: ((event: MatchEvent) => void)[] = [];
  private isConnecting = false;
  private subscribedEventTypes = new Set<string>();
  private reconnectAttempts = 0;

  async connect(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected || this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    try {
      let signalRUrl: string;

      if (import.meta.env.DEV) {
        signalRUrl = 'http://localhost:8080/hubs/domainevent';
      } else {
        const apiUrl = import.meta.env.VITE_API_URL || '';
        const baseUrl = apiUrl.replace('/api', '');
        signalRUrl = `${baseUrl}/hubs/domainevent`;
      }

      console.log(`Connecting to SignalR at: ${signalRUrl}`);

      this.connection = new HubConnectionBuilder()
        .withUrl(signalRUrl, { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      this.connection.on('DomainEvent', (eventType: string, eventData: string) => {
        try {
          const parsedEvent = JSON.parse(eventData);
          const matchEvent: MatchEvent = { eventType, data: parsedEvent };
          console.log('Received SignalR domain event:', matchEvent);
          this.matchEventCallbacks.forEach(callback => {
            try {
              callback(matchEvent);
            } catch (error) {
              console.error('Error in SignalR event callback:', error);
            }
          });
        } catch (error) {
          console.error('Error parsing SignalR event data:', error);
        }
      });

      this.connection.on('MatchEvent', (eventType: string, eventData: string) => {
        try {
          const parsedEvent = JSON.parse(eventData);
          const matchEvent: MatchEvent = { eventType, data: parsedEvent };
          console.log('Received SignalR match event:', matchEvent);
          this.matchEventCallbacks.forEach(callback => {
            try {
              callback(matchEvent);
            } catch (error) {
              console.error('Error in SignalR match event callback:', error);
            }
          });
        } catch (error) {
          console.error('Error parsing SignalR match event data:', error);
        }
      });

      this.connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        this.reconnectAttempts++;
      });

      this.connection.onreconnected(() => {
        console.log('SignalR reconnected');
        this.reconnectAttempts = 0;
        this.resubscribeToAllEvents();
      });

      this.connection.onclose(() => {
        console.log('SignalR connection closed');
        this.reconnectAttempts = 0;
      });

      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('Error connecting to SignalR:', error);
      throw error;
    } finally {
      this.isConnecting = false;
    }
  }

  private async resubscribeToAllEvents(): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      for (const eventType of this.subscribedEventTypes) {
        await this.connection.invoke('SubscribeToEventTypeAsync', eventType);
        console.log(`Resubscribed to event type: ${eventType}`);
      }
    } catch (error) {
      console.error('Error resubscribing to events after reconnection:', error);
    }
  }

  async subscribeToEventType(eventType: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      await this.connect();
    }

    try {
      await this.connection!.invoke('SubscribeToEventTypeAsync', eventType);
      this.subscribedEventTypes.add(eventType);
      console.log(`Subscribed to event type: ${eventType}`);
    } catch (error) {
      console.error(`Error subscribing to event type ${eventType}:`, error);
      throw error;
    }
  }

  async unsubscribeFromEventType(eventType: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke('UnsubscribeFromEventTypeAsync', eventType);
      this.subscribedEventTypes.delete(eventType);
      console.log(`Unsubscribed from event type: ${eventType}`);
    } catch (error) {
      console.error(`Error unsubscribing from event type ${eventType}:`, error);
    }
  }

  async subscribeToMatch(matchId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      await this.connect();
    }

    try {
      await this.connection!.invoke('SubscribeToMatchAsync', matchId);
      console.log(`Subscribed to match: ${matchId}`);
    } catch (error) {
      console.error(`Error subscribing to match ${matchId}:`, error);
      throw error;
    }
  }

  async unsubscribeFromMatch(matchId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke('UnsubscribeFromMatchAsync', matchId);
      console.log(`Unsubscribed from match: ${matchId}`);
    } catch (error) {
      console.error(`Error unsubscribing from match ${matchId}:`, error);
    }
  }

  onMatchEvent(callback: (event: MatchEvent) => void): () => void {
    this.matchEventCallbacks.push(callback);
    return () => {
      const index = this.matchEventCallbacks.indexOf(callback);
      if (index > -1) {
        this.matchEventCallbacks.splice(index, 1);
      }
    };
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR disconnected');
      } catch (error) {
        console.error('Error disconnecting SignalR:', error);
      } finally {
        this.connection = null;
        this.matchEventCallbacks = [];
        this.subscribedEventTypes.clear();
      }
    }
  }

  get isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected;
  }

  get connectionState(): HubConnectionState | null {
    return this.connection?.state || null;
  }

  get subscribedEvents(): string[] {
    return Array.from(this.subscribedEventTypes);
  }
}

// Singleton instance
export const signalRService = new SignalRService();
