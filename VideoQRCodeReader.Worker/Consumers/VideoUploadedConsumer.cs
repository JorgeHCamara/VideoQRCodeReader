using MassTransit;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Application.Services;
using VideoQRCodeReader.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Worker.Consumers
{
    /// <summary>
    /// Consumer that processes uploaded videos for QR code detection
    /// Follows Clean Architecture by using Application and Infrastructure layers
    /// </summary>
    public class VideoUploadedConsumer : IConsumer<VideoUploadedEvent>
    {
        private readonly IVideoAnalysisService _videoAnalysisService;
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<VideoUploadedConsumer> _logger;

        public VideoUploadedConsumer(
            IVideoAnalysisService videoAnalysisService,
            IMessageQueueService messageQueueService,
            ILogger<VideoUploadedConsumer> logger)
        {
            _videoAnalysisService = videoAnalysisService;
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<VideoUploadedEvent> context)
        {
            var message = context.Message;
            
            try
            {
                _logger.LogInformation("Processing video upload: {VideoId} - {FilePath}", message.VideoId, message.FilePath);

                // Publish processing status
                await _messageQueueService.PublishAsync(new ProcessingStatusEvent
                {
                    VideoId = message.VideoId,
                    Status = "Processing",
                    UpdatedAt = DateTime.UtcNow,
                    Message = "Video analysis started"
                });

                // Small delay to ensure processing status is visible
                await Task.Delay(500);

                // Analyze video for QR codes
                var qrCodeDetections = await _videoAnalysisService.AnalyzeVideoAsync(message.FilePath, message.VideoId);

                // Publish completion event
                await _messageQueueService.PublishAsync(new CompletedEvent
                {
                    VideoId = message.VideoId,
                    CompletedAt = DateTime.UtcNow,
                    QrCodeDetections = qrCodeDetections
                });

                _logger.LogInformation("Video processing completed for {VideoId}. Found {QrCount} QR codes", 
                    message.VideoId, qrCodeDetections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing video {VideoId}: {FilePath}", message.VideoId, message.FilePath);

                // Publish error status
                try
                {
                    await _messageQueueService.PublishAsync(new ProcessingStatusEvent
                    {
                        VideoId = message.VideoId,
                        Status = "Failed",
                        UpdatedAt = DateTime.UtcNow,
                        Message = $"Processing failed: {ex.Message}"
                    });
                }
                catch (Exception publishEx)
                {
                    _logger.LogError(publishEx, "Failed to publish error status for video {VideoId}", message.VideoId);
                }

                // Don't rethrow - we want to acknowledge the message to prevent infinite retry
                // In production, you might want to implement a dead letter queue pattern
            }
        }
    }
}
