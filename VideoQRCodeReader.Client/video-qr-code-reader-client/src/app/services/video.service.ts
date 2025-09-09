import { Injectable } from '@angular/core';
import {
  HttpClient,
  HttpEvent,
  HttpEventType,
  HttpRequest,
} from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  VideoUploadResult,
  VideoStatusResponse,
  VideoResultsResponse,
  UploadProgress,
} from '../models/video.models';

@Injectable({
  providedIn: 'root',
})
export class VideoService {
  private readonly baseUrl = '/api/video'; // Relative URL for Docker proxy

  constructor(private http: HttpClient) {}

  uploadVideoWithProgress(
    file: File
  ): Observable<UploadProgress | VideoUploadResult> {
    const formData = new FormData();
    formData.append('file', file);

    console.log('Making request to:', `${this.baseUrl}/upload`);
    console.log('File details:', {
      name: file.name,
      size: file.size,
      type: file.type,
    });

    const request = new HttpRequest(
      'POST',
      `${this.baseUrl}/upload`,
      formData,
      {
        reportProgress: true,
      }
    );

    return this.http.request(request).pipe(
      map((event: HttpEvent<any>) => {
        switch (event.type) {
          case HttpEventType.UploadProgress:
            if (event.total) {
              const percentage = Math.round((100 * event.loaded) / event.total);
              return {
                loaded: event.loaded,
                total: event.total,
                percentage: percentage,
              } as UploadProgress;
            }
            return {
              loaded: event.loaded,
              total: event.loaded,
              percentage: 0,
            } as UploadProgress;
          case HttpEventType.Response:
            return event.body as VideoUploadResult;
          default:
            return {
              loaded: 0,
              total: 0,
              percentage: 0,
            } as UploadProgress;
        }
      })
    );
  }

  getVideoStatus(videoId: string): Observable<VideoStatusResponse> {
    return this.http.get<VideoStatusResponse>(
      `${this.baseUrl}/${videoId}/status`
    );
  }

  getVideoResults(videoId: string): Observable<VideoResultsResponse> {
    return this.http.get<VideoResultsResponse>(
      `${this.baseUrl}/${videoId}/results`
    );
  }
}
