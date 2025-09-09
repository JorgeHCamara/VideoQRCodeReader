using MassTransit;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Services;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Consumers
{
    /// <summary>
    /// Consumer that handles completed video processing results in the API
    /// Stores the final results for retrieval via the results endpoint
    /// </summary>
    public class CompletedEventConsumer : IConsumer<CompletedEvent>
    {
        private readonly IVideoResultsService _videoResultsService;
        private readonly ISignalRNotificationService _signalRService;
        private readonly ILogger<CompletedEventConsumer> _logger;

        public CompletedEventConsumer(
            IVideoResultsService videoResultsService,
            ISignalRNotificationService signalRService,
            ILogger<CompletedEventConsumer> logger)
        {
            _videoResultsService = videoResultsService;
            _signalRService = signalRService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CompletedEvent> context)
        {
            var completedEvent = context.Message;
            
            _logger.LogInformation("Video processing completed for {VideoId}: {QrCount} QR codes detected", 
                completedEvent.VideoId, completedEvent.QrCodeDetections.Count);

            // Store results in database
            await _videoResultsService.StoreCompletedResultsAsync(completedEvent);

            // Send real-time notification with results via SignalR
            await _signalRService.SendProcessingComplete(completedEvent.VideoId, new
            {
                QrCodes = completedEvent.QrCodeDetections.Select(qr => new
                {
                    Content = qr.Content,
                    TimestampSeconds = qr.TimestampSeconds,
                    FrameNumber = qr.FrameNumber,
                    FramePath = qr.FramePath
                }).ToList()
            });
        }
    }
}
