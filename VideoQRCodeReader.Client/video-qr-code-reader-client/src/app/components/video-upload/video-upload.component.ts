import { Component, OnInit, OnDestroy } from '@angular/core';
import { VideoService } from '../../services/video.service';
import {
  SignalRService,
  VideoStatusUpdate,
  ProcessingCompleteEvent,
  ProcessingErrorEvent,
} from '../../services/signalr.service';
import { Subscription } from 'rxjs';
import {
  VideoUploadResult,
  VideoStatusResponse,
  VideoResultsResponse,
  UploadProgress,
} from '../../models/video.models';

@Component({
  selector: 'app-video-upload',
  templateUrl: './video-upload.component.html',
  styleUrls: ['./video-upload.component.scss'],
  standalone: false,
})
export class VideoUploadComponent implements OnInit, OnDestroy {
  selectedFile: File | null = null;
  isDragOver = false;
  isUploading = false;
  uploadProgress = 0;
  uploadResult: VideoUploadResult | null = null;
  videoStatus: VideoStatusResponse | null = null;
  videoResults: VideoResultsResponse | null = null;
  error: string | null = null;
  isSignalRConnected = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private videoService: VideoService,
    private signalRService: SignalRService
  ) {}

  // Getter to return unique QR codes by content
  get uniqueQrCodes() {
    if (!this.videoResults?.qrCodeDetections) {
      return [];
    }

    const uniqueMap = new Map();

    // Group by content and keep the first occurrence (earliest timestamp)
    this.videoResults.qrCodeDetections.forEach((qr) => {
      if (!uniqueMap.has(qr.content)) {
        uniqueMap.set(qr.content, qr);
      } else {
        // If we already have this content, keep the one with earlier timestamp
        const existing = uniqueMap.get(qr.content);
        if (qr.timestampSeconds < existing.timestampSeconds) {
          uniqueMap.set(qr.content, qr);
        }
      }
    });

    return Array.from(uniqueMap.values()).sort(
      (a, b) => a.timestampSeconds - b.timestampSeconds
    );
  }

  async ngOnInit(): Promise<void> {
    try {
      // Start SignalR connection
      await this.signalRService.startConnection();

      // Subscribe to connection status
      this.subscriptions.push(
        this.signalRService.connectionStatus$.subscribe((status) => {
          this.isSignalRConnected = status;
          console.log('SignalR connection status:', status);
        })
      );

      // Subscribe to status updates
      this.subscriptions.push(
        this.signalRService.statusUpdates$.subscribe((update) => {
          if (
            update &&
            this.uploadResult &&
            update.videoId === this.uploadResult.videoId
          ) {
            this.handleStatusUpdate(update);
          }
        })
      );

      // Subscribe to processing complete events
      this.subscriptions.push(
        this.signalRService.processingComplete$.subscribe((event) => {
          if (
            event &&
            this.uploadResult &&
            event.videoId === this.uploadResult.videoId
          ) {
            this.handleProcessingComplete(event);
          }
        })
      );

      // Subscribe to processing errors
      this.subscriptions.push(
        this.signalRService.processingErrors$.subscribe((error) => {
          if (
            error &&
            this.uploadResult &&
            error.videoId === this.uploadResult.videoId
          ) {
            this.handleProcessingError(error);
          }
        })
      );
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
      this.error = 'Failed to initialize real-time updates';
    }
  }

  ngOnDestroy(): void {
    // Clean up subscriptions
    this.subscriptions.forEach((sub) => sub.unsubscribe());

    // Leave video group if we're tracking one
    if (this.uploadResult) {
      this.signalRService.leaveVideoGroup(this.uploadResult.videoId);
    }
  }

  private handleStatusUpdate(update: VideoStatusUpdate): void {
    console.log('Received real-time status update:', update);
    this.videoStatus = {
      status: update.status,
      message: update.message || '',
      videoId: update.videoId,
    };

    // If status becomes Completed and we don't have results yet, fetch them
    if (update.status === 'Completed' && !this.videoResults) {
      console.log('Status became Completed, fetching results...');
      this.getVideoResults(update.videoId);
    }
  }

  private handleProcessingComplete(event: ProcessingCompleteEvent): void {
    console.log('Received real-time processing complete:', event);
    this.videoStatus = {
      status: 'Completed',
      message: 'Video processing completed successfully',
      videoId: event.videoId,
    };

    // Map the results to match our interface
    if (
      event.results &&
      event.results.QrCodes &&
      event.results.QrCodes.length > 0
    ) {
      this.videoResults = {
        videoId: event.videoId,
        status: 'Completed',
        completedAt: event.timestamp.toISOString(),
        qrCodeDetections: event.results.QrCodes.map((qr: any) => ({
          content: qr.Content,
          timestampSeconds: qr.TimestampSeconds,
          frameNumber: qr.FrameNumber || 0,
          framePath: qr.FramePath || '',
        })),
      };
    } else {
      // Fallback: fetch results from API if SignalR event doesn't contain results
      console.log('SignalR event missing results, fetching from API...');
      this.getVideoResults(event.videoId);
    }
  }

  private handleProcessingError(error: ProcessingErrorEvent): void {
    console.log('Received real-time processing error:', error);
    this.error = `Processing failed: ${error.error}`;
    this.videoStatus = {
      status: 'Failed',
      message: error.error,
      videoId: error.videoId,
    };
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFileSelected(event: any): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  handleFileInput(input: HTMLInputElement): void {
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  private handleFileSelection(file: File): void {
    if (this.validateFile(file)) {
      this.selectedFile = file;
      this.error = null;
    }
  }

  private validateFile(file: File): boolean {
    // Check file type
    const allowedTypes = [
      'video/mp4',
      'video/avi',
      'video/mov',
      'video/quicktime',
    ];
    if (!allowedTypes.includes(file.type)) {
      this.error = 'Please select a valid video file (MP4, AVI, MOV)';
      return false;
    }

    // Check file size (500MB limit)
    const maxSize = 500 * 1024 * 1024; // 500MB
    if (file.size > maxSize) {
      this.error = 'File size must be less than 500MB';
      return false;
    }

    return true;
  }

  uploadVideo(): void {
    if (!this.selectedFile) return;

    this.isUploading = true;
    this.uploadProgress = 0;
    this.error = null;
    this.uploadResult = null;
    this.videoStatus = null;
    this.videoResults = null;

    console.log(
      'Starting upload for file:',
      this.selectedFile.name,
      'Size:',
      this.selectedFile.size
    );

    this.videoService.uploadVideoWithProgress(this.selectedFile).subscribe({
      next: (event) => {
        if ('percentage' in event) {
          // This is upload progress
          this.uploadProgress = event.percentage;
          console.log('Upload progress:', event.percentage + '%');
        } else {
          // This is the final result
          this.uploadResult = event as VideoUploadResult;
          this.isUploading = false;
          console.log('Upload completed, result:', this.uploadResult);

          // Set initial processing status
          this.videoStatus = {
            status: 'Queued',
            message: 'Video uploaded successfully. Queued for processing...',
            videoId: this.uploadResult.videoId,
          };

          // Join SignalR group for real-time updates
          if (this.isSignalRConnected) {
            this.signalRService.joinVideoGroup(this.uploadResult.videoId);
          } else {
            // Fallback to polling if SignalR is not connected
            this.checkVideoStatus(this.uploadResult.videoId);
          }
        }
      },
      error: (error) => {
        console.error('Upload error details:', error);
        this.error = `Upload failed: ${
          error.error?.message || error.message || 'Please try again.'
        }`;
        this.isUploading = false;
      },
    });
  }

  private checkVideoStatus(videoId: string): void {
    const statusCheck = () => {
      this.videoService.getVideoStatus(videoId).subscribe({
        next: (status) => {
          this.videoStatus = status;
          if (status.status === 'Completed') {
            this.getVideoResults(videoId);
          } else if (
            status.status === 'Processing' ||
            status.status === 'Queued'
          ) {
            // Check again in 2 seconds
            setTimeout(statusCheck, 2000);
          }
        },
        error: (error) => {
          this.error = 'Failed to check video status';
          console.error('Status check error:', error);
        },
      });
    };

    statusCheck();
  }

  private getVideoResults(videoId: string): void {
    this.videoService.getVideoResults(videoId).subscribe({
      next: (results) => {
        this.videoResults = results;
      },
      error: (error) => {
        this.error = 'Failed to get video results';
        console.error('Results error:', error);
      },
    });
  }

  reset(): void {
    // Leave SignalR group if we're tracking a video
    if (this.uploadResult && this.isSignalRConnected) {
      this.signalRService.leaveVideoGroup(this.uploadResult.videoId);
    }

    this.selectedFile = null;
    this.isDragOver = false;
    this.isUploading = false;
    this.uploadProgress = 0;
    this.uploadResult = null;
    this.videoStatus = null;
    this.videoResults = null;
    this.error = null;
  }
}
