export interface VideoUploadResult {
  videoId: string;
  message: string;
}

export interface VideoStatusResponse {
  videoId: string;
  status: string;
  message?: string;
}

export interface QrCodeDetectionResponse {
  content: string;
  frameNumber: number;
  timestampSeconds: number;
  framePath?: string;
}

export interface VideoResultsResponse {
  videoId: string;
  status: string;
  completedAt?: string;
  message?: string;
  qrCodeDetections: QrCodeDetectionResponse[];
}

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}
