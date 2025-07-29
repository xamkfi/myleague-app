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
  private maxReconnectAttempts = 5;

  /**
   * Tests the SignalR connection by trying to connect and then disconnect
   */
  async testConnection(): Promise<boolean> {
    try {
      console.log('Testing SignalR connection...');
      await this.connect();
      console.log('SignalR connection test successful');
      await this.disconnect();
      return true;
    } catch (error) {
      console.error('SignalR connection test failed:', error);
      return false;
    }
  }

  /**
   * Tests if the backend API is accessible by checking the health endpoint
   * @returns Promise<boolean> - true if accessible, false otherwise
   */
  async testBackendAccessibility(): Promise<boolean> {
    try {
      const apiUrl = import.meta.env.DEV 
        ? 'http://localhost:8080/api/health/ready'
        : `${import.meta.env.VITE_API_URL}/health/ready`;
      
      const response = await fetch(apiUrl, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return response.ok;
    } catch (error) {
      console.error('Backend accessibility test failed:', error);
      return false;
    }
  }

  async connect(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected || this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    let signalRUrl: string = '';

    try {
      // Test if backend is accessible first (but don't fail if it doesn't work)
      try {
        const isBackendAccessible = await this.testBackendAccessibility();
        console.log('Backend accessibility test result:', isBackendAccessible);
        if (!isBackendAccessible) {
          console.warn('Backend health check failed, but attempting SignalR connection anyway...');
        }
      } catch (error) {
        console.warn('Backend accessibility test failed, but attempting SignalR connection anyway:', error);
      }

      if (import.meta.env.DEV) {
        signalRUrl = 'http://localhost:8080/api/hubs/domainevent';
      } else {
        const apiUrl = import.meta.env.VITE_API_URL || '';
        const baseUrl = apiUrl.replace('/api', '');
        signalRUrl = `${baseUrl}/api/hubs/domainevent`;
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
          
          // Only log very occasionally to avoid spam
          const shouldLog = Math.random() < 0.01; // Log ~1% of events
          if (shouldLog) {
            console.log('=== SIGNALR DOMAIN EVENT ===');
            console.log('Event type:', eventType);
            console.log('Event data:', parsedEvent);
            console.log('=== END SIGNALR DOMAIN EVENT ===');
          }
          
          // Notify all registered callbacks
          this.matchEventCallbacks.forEach(callback => {
            try {
              callback(matchEvent);
            } catch (error) {
              console.error('Error in SignalR callback:', error);
            }
          });
        } catch (error) {
          console.error('Error parsing SignalR event:', error);
        }
      });

      this.connection.on('MatchEvent', (eventType: string, eventData: string) => {
        try {
          const parsedEvent = JSON.parse(eventData);
          const matchEvent: MatchEvent = { eventType, data: parsedEvent };
          
          // Only log occasionally to avoid spam
          const shouldLog = Math.random() < 0.05; // Log ~5% of events
          if (shouldLog) {
            console.log('🔔 RECEIVED SIGNALR MATCH EVENT');
            console.log('Match event type:', eventType);
            console.log('Number of callbacks registered:', this.matchEventCallbacks.length);
          }
          
          this.matchEventCallbacks.forEach((callback, index) => {
            try {
              if (shouldLog) {
                console.log(`Calling callback ${index + 1}/${this.matchEventCallbacks.length}`);
              }
              callback(matchEvent);
            } catch (error) {
              console.error(`Error in SignalR event callback ${index + 1}:`, error);
            }
          });
        } catch (error) {
          console.error('Error parsing SignalR event data:', error);
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

      console.log('Starting SignalR connection...');
      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('Error connecting to SignalR:', error);
      console.error('SignalR connection details:', {
        url: signalRUrl,
        connectionState: this.connection?.state,
        error: error instanceof Error ? error.message : String(error),
        errorStack: error instanceof Error ? error.stack : undefined
      });
      
      // Log additional connection details
      console.error('Connection attempt details:', {
        isConnecting: this.isConnecting,
        connectionExists: !!this.connection,
        connectionState: this.connection?.state,
        environment: import.meta.env.DEV ? 'development' : 'production'
      });
      
      this.connection = null;
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
      try {
        console.log('SignalR not connected, connecting first...');
        await this.connect();
        
        // Wait a moment for the connection to be fully established
        console.log('Waiting for SignalR connection to be established...');
        let attempts = 0;
        while (this.connection && this.connection.state !== HubConnectionState.Connected && attempts < 10) {
          await new Promise(resolve => setTimeout(resolve, 100));
          attempts++;
          console.log(`Connection attempt ${attempts}/10, state: ${this.connection?.state}`);
        }
        
        if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
          throw new Error(`SignalR connection failed to establish. Current state: ${this.connection?.state}`);
        }
      } catch (error) {
        console.error(`Error connecting to SignalR for event type ${eventType}:`, error);
        throw new Error(`Cannot subscribe to event type ${eventType}: Connection failed`);
      }
    }

    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      throw new Error(`Cannot send data if the connection is not in the 'Connected' State`);
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
      console.log('SignalR not connected, connecting first...');
      await this.connect();
      
      // Wait a moment for the connection to be fully established
      console.log('Waiting for SignalR connection to be established...');
      let attempts = 0;
      while (this.connection && this.connection.state !== HubConnectionState.Connected && attempts < 10) {
        await new Promise(resolve => setTimeout(resolve, 100));
        attempts++;
        console.log(`Connection attempt ${attempts}/10, state: ${this.connection?.state}`);
      }
      
      if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
        throw new Error(`SignalR connection failed to establish. Current state: ${this.connection?.state}`);
      }
    }

    try {
      console.log(`Subscribing to match: ${matchId}`);
      console.log(`SignalR connection state: ${this.connection?.state}`);
      await this.connection!.invoke('SubscribeToMatchAsync', matchId);
      console.log(`✅ Successfully subscribed to match: ${matchId}`);
    } catch (error) {
      console.error(`❌ Error subscribing to match ${matchId}:`, error);
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
    console.log(`🔧 REGISTERED NEW MATCH EVENT CALLBACK. Total callbacks: ${this.matchEventCallbacks.length}`);
    
    return () => {
      const index = this.matchEventCallbacks.indexOf(callback);
      if (index > -1) {
        this.matchEventCallbacks.splice(index, 1);
        console.log(`🔧 UNREGISTERED MATCH EVENT CALLBACK. Total callbacks: ${this.matchEventCallbacks.length}`);
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
