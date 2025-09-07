using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Domain.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Infrastructure.Services
{
    /// <summary>
    /// Implementation of video processing using FFMpegCore library
    /// Follows Single Responsibility Principle - only handles video processing
    /// </summary>
    public class FFMpegVideoProcessingService : IVideoProcessingService
    {
        private readonly ILogger<FFMpegVideoProcessingService> _logger;

        public FFMpegVideoProcessingService(ILogger<FFMpegVideoProcessingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure FFMpeg binary path if needed
            // FFMpegOptions.Configure(options => options.BinaryFolder = "path/to/ffmpeg");
        }

        public async Task<List<string>> ExtractFramesAsync(string videoFilePath, string outputDirectory, double frameInterval = 1.0)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(videoFilePath))
                    throw new ArgumentException("Video file path cannot be null or empty.", nameof(videoFilePath));
                
                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new ArgumentException("Output directory cannot be null or empty.", nameof(outputDirectory));

                if (!File.Exists(videoFilePath))
                    throw new FileNotFoundException($"Video file not found: {videoFilePath}");

                // Ensure output directory exists
                Directory.CreateDirectory(outputDirectory);

                _logger.LogInformation("Starting frame extraction from video: {VideoPath}", videoFilePath);

                var extractedFrames = new List<string>();
                var videoInfo = await FFProbe.AnalyseAsync(videoFilePath);
                var totalDuration = videoInfo.Duration;
                var frameCount = (int)(totalDuration.TotalSeconds / frameInterval);

                _logger.LogInformation("Extracting {FrameCount} frames from video with duration {Duration}", 
                    frameCount, totalDuration);

                // Extract frames at specified intervals
                for (int i = 0; i < frameCount; i++)
                {
                    var timeSpan = TimeSpan.FromSeconds(i * frameInterval);
                    var outputFileName = $"frame_{i:D6}_{timeSpan.TotalSeconds:F1}s.png";
                    var outputPath = Path.Combine(outputDirectory, outputFileName);

                    try
                    {
                        await FFMpeg.SnapshotAsync(videoFilePath, outputPath, null, timeSpan);
                        extractedFrames.Add(outputPath);
                        
                        _logger.LogDebug("Extracted frame {FrameNumber} at {TimeSpan}: {OutputPath}", 
                            i + 1, timeSpan, outputPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract frame at {TimeSpan}", timeSpan);
                        // Continue with next frame instead of failing completely
                    }
                }

                _logger.LogInformation("Successfully extracted {ExtractedCount} frames to {OutputDirectory}", 
                    extractedFrames.Count, outputDirectory);

                return extractedFrames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting frames from video: {VideoPath}", videoFilePath);
                throw;
            }
        }

        public async Task<Video> GetVideoInfoAsync(string videoFilePath)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(videoFilePath))
                    throw new ArgumentException("Video file path cannot be null or empty.", nameof(videoFilePath));

                if (!File.Exists(videoFilePath))
                    throw new FileNotFoundException($"Video file not found: {videoFilePath}");

                _logger.LogInformation("Analyzing video file: {VideoPath}", videoFilePath);

                // Analyze video using FFProbe
                var mediaInfo = await FFProbe.AnalyseAsync(videoFilePath);
                var fileInfo = new FileInfo(videoFilePath);

                var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                if (videoStream == null)
                    throw new InvalidOperationException("No video stream found in the file.");

                var videoInfo = new Video
                {
                    Duration = mediaInfo.Duration,
                    Width = videoStream.Width,
                    Height = videoStream.Height,
                    FrameRate = videoStream.FrameRate,
                    Format = mediaInfo.Format.FormatName ?? "Unknown",
                    FileSizeBytes = fileInfo.Length
                };

                _logger.LogInformation("Video analysis complete: Duration={Duration}, Resolution={Width}x{Height}, FrameRate={FrameRate}", 
                    videoInfo.Duration, videoInfo.Width, videoInfo.Height, videoInfo.FrameRate);

                return videoInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing video file: {VideoPath}", videoFilePath);
                throw;
            }
        }
    }
}
