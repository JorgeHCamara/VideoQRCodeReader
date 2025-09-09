import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

export interface VideoStatusUpdate {
  videoId: string;
  status: string;
  message?: string;
  timestamp: Date;
}

export interface ProcessingCompleteEvent {
  videoId: string;
  status: string;
  results: any;
  timestamp: Date;
}

export interface ProcessingErrorEvent {
  videoId: string;
  status: string;
  error: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private connectionStatus = new BehaviorSubject<boolean>(false);

  // Event streams
  private statusUpdates = new BehaviorSubject<VideoStatusUpdate | null>(null);
  private processingComplete =
    new BehaviorSubject<ProcessingCompleteEvent | null>(null);
  private processingErrors = new BehaviorSubject<ProcessingErrorEvent | null>(
    null
  );

  public connectionStatus$ = this.connectionStatus.asObservable();
  public statusUpdates$ = this.statusUpdates.asObservable();
  public processingComplete$ = this.processingComplete.asObservable();
  public processingErrors$ = this.processingErrors.asObservable();

  constructor() {}

  public async startConnection(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      return;
    }

    try {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl('/videohub')
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      this.setupEventHandlers();

      await this.hubConnection.start();
      this.connectionStatus.next(true);
      console.log('SignalR connection established');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionStatus.next(false);
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionStatus.next(false);
      console.log('SignalR connection stopped');
    }
  }

  public async joinVideoGroup(videoId: string): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('JoinGroup', videoId);
        console.log(`Joined video group: ${videoId}`);
      } catch (error) {
        console.error('Error joining video group:', error);
      }
    }
  }

  public async leaveVideoGroup(videoId: string): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('LeaveGroup', videoId);
        console.log(`Left video group: ${videoId}`);
      } catch (error) {
        console.error('Error leaving video group:', error);
      }
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle status updates
    this.hubConnection.on('StatusUpdate', (data: VideoStatusUpdate) => {
      console.log('Received status update:', data);
      this.statusUpdates.next(data);
    });

    // Handle processing complete
    this.hubConnection.on(
      'ProcessingComplete',
      (data: ProcessingCompleteEvent) => {
        console.log('Received processing complete:', data);
        this.processingComplete.next(data);
      }
    );

    // Handle processing errors
    this.hubConnection.on('ProcessingError', (data: ProcessingErrorEvent) => {
      console.log('Received processing error:', data);
      this.processingErrors.next(data);
    });

    // Handle connection events
    this.hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.connectionStatus.next(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.connectionStatus.next(true);
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR connection closed');
      this.connectionStatus.next(false);
    });
  }

  public isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }
}
