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
        private readonly IVideoResultsService _videoResultsService;
        private readonly ILogger<CompletedEventConsumer> _logger;

        public CompletedEventConsumer(
            IVideoResultsService videoResultsService,
            ILogger<CompletedEventConsumer> logger)
        {
            _videoResultsService = videoResultsService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CompletedEvent> context)
        {
            var completedEvent = context.Message;
            
            _logger.LogInformation("Video processing completed for {VideoId}: {QrCount} QR codes detected", 
                completedEvent.VideoId, completedEvent.QrCodeDetections.Count);

            await _videoResultsService.StoreCompletedResultsAsync(completedEvent);
        }
    }
}
