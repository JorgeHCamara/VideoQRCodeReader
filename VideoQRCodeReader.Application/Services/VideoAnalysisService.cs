using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Application.Services
{
    public class VideoAnalysisService : IVideoAnalysisService
    {
        private readonly IVideoProcessingService _videoProcessingService;
        private readonly IQrCodeDetectionService _qrCodeDetectionService;
        private readonly ILogger<VideoAnalysisService> _logger;

        public VideoAnalysisService(
            IVideoProcessingService videoProcessingService,
            IQrCodeDetectionService qrCodeDetectionService,
            ILogger<VideoAnalysisService> logger)
        {
            _videoProcessingService = videoProcessingService;
            _qrCodeDetectionService = qrCodeDetectionService;
            _logger = logger;
        }

        public async Task<List<QrCodeDetection>> AnalyzeVideoAsync(string videoFilePath, string videoId)
        {
            try
            {
                _logger.LogInformation("Starting video analysis for video {VideoId}: {VideoPath}", videoId, videoFilePath);

                // Get video information first
                var videoInfo = await _videoProcessingService.GetVideoInfoAsync(videoFilePath);
                _logger.LogInformation("Video analysis - Duration: {Duration}, Resolution: {Width}x{Height}", 
                    videoInfo.Duration, videoInfo.Width, videoInfo.Height);

                // Create output directory for frames
                var outputDirectory = Path.Combine(Path.GetTempPath(), "VideoQRFrames", videoId);
                Directory.CreateDirectory(outputDirectory);

                try
                {
                    // Extract frames from video
                    _logger.LogInformation("Extracting frames from video {VideoId}", videoId);
                    var frameInterval = CalculateOptimalFrameInterval(videoInfo.Duration);
                    var extractedFrames = await _videoProcessingService.ExtractFramesAsync(videoFilePath, outputDirectory, frameInterval);

                    if (!extractedFrames.Any())
                    {
                        _logger.LogWarning("No frames were extracted from video {VideoId}", videoId);
                        return new List<QrCodeDetection>();
                    }

                    _logger.LogInformation("Extracted {FrameCount} frames, analyzing for QR codes...", extractedFrames.Count);

                    // Analyze each frame for QR codes
                    var qrCodeDetections = new List<QrCodeDetection>();
                    var frameNumber = 0;

                    foreach (var framePath in extractedFrames)
                    {
                        try
                        {
                            var qrCodes = await _qrCodeDetectionService.DetectMultipleQrCodesAsync(framePath);
                            
                            foreach (var qrCode in qrCodes)
                            {
                                var detection = new QrCodeDetection
                                {
                                    Content = qrCode,
                                    FrameNumber = frameNumber,
                                    TimestampSeconds = frameNumber * frameInterval,
                                    FramePath = framePath
                                };

                                qrCodeDetections.Add(detection);
                                _logger.LogInformation("QR code detected in frame {FrameNumber} at {Timestamp}s: {Content}", 
                                    frameNumber, detection.TimestampSeconds, qrCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to analyze frame {FrameNumber} for video {VideoId}", frameNumber, videoId);
                            // Continue with next frame
                        }

                        frameNumber++;
                    }

                    _logger.LogInformation("Video analysis complete for {VideoId}. Found {QrCount} QR codes", 
                        videoId, qrCodeDetections.Count);

                    return qrCodeDetections;
                }
                finally
                {
                    // Clean up temporary frame files
                    try
                    {
                        if (Directory.Exists(outputDirectory))
                        {
                            Directory.Delete(outputDirectory, true);
                            _logger.LogDebug("Cleaned up temporary frames for video {VideoId}", videoId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up temporary frames for video {VideoId}", videoId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing video {VideoId}: {VideoPath}", videoId, videoFilePath);
                throw;
            }
        }

        /// <summary>
        /// Calculates optimal frame extraction interval based on video duration
        /// Shorter videos = more frequent sampling, longer videos = less frequent
        /// </summary>
        private static double CalculateOptimalFrameInterval(TimeSpan duration)
        {
            return duration.TotalMinutes switch
            {
                <= 1 => 0.5,    // Every 0.5 seconds for videos <= 1 minute
                <= 5 => 1.0,    // Every 1 second for videos <= 5 minutes  
                <= 15 => 2.0,   // Every 2 seconds for videos <= 15 minutes
                _ => 5.0         // Every 5 seconds for longer videos
            };
        }
    }
}
