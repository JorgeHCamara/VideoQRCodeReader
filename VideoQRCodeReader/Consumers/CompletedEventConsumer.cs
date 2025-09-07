using MassTransit;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Consumers
{
    /// <summary>
    /// Consumer that handles completed video processing results in the API
    /// Stores the final results for retrieval via the results endpoint
    /// </summary>
    public class CompletedEventConsumer : IConsumer<CompletedEvent>
    {
        private readonly IVideoStatusService _videoStatusService;
        private readonly ILogger<CompletedEventConsumer> _logger;

        public CompletedEventConsumer(
            IVideoStatusService videoStatusService,
            ILogger<CompletedEventConsumer> logger)
        {
            _videoStatusService = videoStatusService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CompletedEvent> context)
        {
            var completedEvent = context.Message;
            
            _logger.LogInformation("Video processing completed for {VideoId}: {QrCount} QR codes detected", 
                completedEvent.VideoId, completedEvent.QrCodeDetections.Count);

            await _videoStatusService.StoreCompletedResultsAsync(completedEvent);
        }
    }
}
