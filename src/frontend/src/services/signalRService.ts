import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { API_URL } from '../constants/config';

const TOKEN_STORAGE_KEY = 'myleague_auth_tokens';

export interface MatchEvent {
  eventType: string;
  data: unknown;
}

function getAccessToken(): string | null {
  try {
    const raw = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!raw) return null;
    const tokens = JSON.parse(raw) as { accessToken?: string };
    return tokens.accessToken ?? null;
  } catch {
    return null;
  }
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private matchEventCallbacks: ((event: MatchEvent) => void)[] = [];
  private connectPromise: Promise<void> | null = null;
  private subscribedEventTypes = new Set<string>();
  private subscribedMatches = new Set<string>();

  async testBackendAccessibility(): Promise<boolean> {
    try {
      const apiUrl = import.meta.env.DEV
        ? 'http://localhost:8080/api/health/ready'
        : `${API_URL}/health/ready`;

      const response = await fetch(apiUrl, { method: 'GET' });
      return response.ok;
    } catch {
      return false;
    }
  }

  async connect(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) {
      return;
    }

    if (this.connectPromise) {
      await this.connectPromise;
      return;
    }

    this.connectPromise = this.startConnection();
    try {
      await this.connectPromise;
    } finally {
      this.connectPromise = null;
    }
  }

  private async startConnection(): Promise<void> {
    const signalRUrl = import.meta.env.DEV
      ? 'http://localhost:8080/api/hubs/domainevent'
      : `${API_URL.replace('/api', '')}/api/hubs/domainevent`;

    try {
      this.connection = new HubConnectionBuilder()
        .withUrl(signalRUrl, {
          withCredentials: true,
          accessTokenFactory: () => getAccessToken() ?? '',
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
        .build();

      this.connection.on('DomainEvent', (eventType: string, eventData: string) => {
        this.dispatchEvent(eventType, eventData);
      });

      this.connection.on('MatchEvent', (eventType: string, eventData: string) => {
        this.dispatchEvent(eventType, eventData);
      });

      this.connection.onreconnected(() => {
        void this.resubscribeAll();
      });

      await this.connection.start();
    } catch {
      this.connection = null;
    }
  }

  private dispatchEvent(eventType: string, eventData: string): void {
    try {
      const parsedEvent = JSON.parse(eventData);
      const matchEvent: MatchEvent = { eventType, data: parsedEvent };

      for (const callback of this.matchEventCallbacks) {
        try {
          callback(matchEvent);
        } catch (error) {
          console.error('Error in SignalR callback:', error);
        }
      }
    } catch (error) {
      console.error('Error parsing SignalR event:', error);
    }
  }

  private async resubscribeAll(): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      for (const eventType of this.subscribedEventTypes) {
        await this.connection.invoke('SubscribeToEventTypeAsync', eventType);
      }
      for (const matchId of this.subscribedMatches) {
        await this.connection.invoke('SubscribeToMatchAsync', matchId);
      }
    } catch (error) {
      console.error('Error resubscribing after reconnection:', error);
    }
  }

  private async ensureConnected(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) {
      return;
    }

    await this.connect();

    // After `await`, control-flow narrowing can exclude `Connected` from `state`'s union; read via a nested function instead.
    const isEstablished = (): boolean =>
      Boolean(this.connection?.state === HubConnectionState.Connected);

    let attempts = 0;
    while (!isEstablished() && attempts < 10) {
      await new Promise<void>((resolve) => setTimeout(resolve, 100));
      attempts++;
    }

    if (!isEstablished()) {
      throw new Error('SignalR connection failed to establish');
    }
  }

  async subscribeToEventType(eventType: string): Promise<void> {
    await this.ensureConnected();
    await this.connection!.invoke('SubscribeToEventTypeAsync', eventType);
    this.subscribedEventTypes.add(eventType);
  }

  async unsubscribeFromEventType(eventType: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke('UnsubscribeFromEventTypeAsync', eventType);
      this.subscribedEventTypes.delete(eventType);
    } catch (error) {
      console.error('Error unsubscribing from event type:', eventType, error);
    }
  }

  async subscribeToMatch(matchId: string): Promise<void> {
    await this.ensureConnected();
    await this.connection!.invoke('SubscribeToMatchAsync', matchId);
    this.subscribedMatches.add(matchId);
  }

  async unsubscribeFromMatch(matchId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      await this.connection.invoke('UnsubscribeFromMatchAsync', matchId);
      this.subscribedMatches.delete(matchId);
    } catch (error) {
      console.error('Error unsubscribing from match:', matchId, error);
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
      } catch (error) {
        console.error('Error disconnecting SignalR:', error);
      } finally {
        this.connection = null;
        this.matchEventCallbacks = [];
        this.subscribedEventTypes.clear();
        this.subscribedMatches.clear();
      }
    }
  }

  get isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected;
  }

  get connectionState(): HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  get subscribedEvents(): string[] {
    return Array.from(this.subscribedEventTypes);
  }
}

export const signalRService = new SignalRService();
