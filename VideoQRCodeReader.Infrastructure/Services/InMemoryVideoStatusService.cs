using System.Collections.Concurrent;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class InMemoryVideoStatusService : IVideoStatusService
    {
        private readonly ConcurrentDictionary<string, ProcessingStatusEvent> _statusStore = new();
        private readonly ConcurrentDictionary<string, CompletedEvent> _resultsStore = new();

        public Task UpdateStatusAsync(string videoId, string status, string? message = null)
        {
            var statusEvent = new ProcessingStatusEvent
            {
                VideoId = videoId,
                Status = status,
                UpdatedAt = DateTime.UtcNow,
                Message = message
            };

            _statusStore.AddOrUpdate(videoId, statusEvent, (key, oldValue) => statusEvent);
            
            return Task.CompletedTask;
        }

        public Task<ProcessingStatusEvent?> GetStatusAsync(string videoId)
        {
            _statusStore.TryGetValue(videoId, out var status);
            return Task.FromResult(status);
        }

        public Task StoreCompletedResultsAsync(CompletedEvent completedEvent)
        {
            _resultsStore.AddOrUpdate(completedEvent.VideoId, completedEvent, (key, oldValue) => completedEvent);
            
            var uniqueQrCodes = completedEvent.QrCodeDetections
                .GroupBy(qr => qr.Content)
                .Count();

            var totalDetections = completedEvent.QrCodeDetections.Count;

            string message;
            if (uniqueQrCodes == 0)
            {
                message = "Processing completed with no QR codes detected";
            }
            else
            {
                message = $"Processing completed with {uniqueQrCodes} unique QR codes detected";
            }

            return UpdateStatusAsync(completedEvent.VideoId, "Completed", message);
        }

        public Task<CompletedEvent?> GetCompletedResultsAsync(string videoId)
        {
            _resultsStore.TryGetValue(videoId, out var results);
            return Task.FromResult(results);
        }
    }
}
