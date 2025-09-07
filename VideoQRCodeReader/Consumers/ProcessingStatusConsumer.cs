using MassTransit;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Consumers
{
    /// <summary>
    /// Consumer that updates video processing status in the API
    /// This allows the API to track processing progress from the Worker
    /// </summary>
    public class ProcessingStatusConsumer : IConsumer<ProcessingStatusEvent>
    {
        private readonly IVideoStatusService _videoStatusService;
        private readonly ILogger<ProcessingStatusConsumer> _logger;

        public ProcessingStatusConsumer(
            IVideoStatusService videoStatusService,
            ILogger<ProcessingStatusConsumer> logger)
        {
            _videoStatusService = videoStatusService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessingStatusEvent> context)
        {
            var statusEvent = context.Message;
            
            _logger.LogInformation("Updating status for video {VideoId}: {Status} - {Message}", 
                statusEvent.VideoId, statusEvent.Status, statusEvent.Message);

            await _videoStatusService.UpdateStatusAsync(
                statusEvent.VideoId, 
                statusEvent.Status, 
                statusEvent.Message);
        }
    }
}
