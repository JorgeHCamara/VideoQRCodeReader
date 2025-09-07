using MongoDB.Driver;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Data.Documents;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class VideoResultsService : IVideoResultsService
    {
        private readonly IMongoCollection<CompletedEventDocument> _resultsCollection;
        private readonly IVideoStatusService _statusService;

        public VideoResultsService(IMongoDatabase database, IVideoStatusService statusService)
        {
            _resultsCollection = database.GetCollection<CompletedEventDocument>("completed_results");
            _statusService = statusService;
        }

        public async Task StoreCompletedResultsAsync(CompletedEvent completedEvent)
        {
            var document = new CompletedEventDocument
            {
                VideoId = completedEvent.VideoId,
                CompletedAt = completedEvent.CompletedAt,
                QrCodeDetections = completedEvent.QrCodeDetections?.Select(qr => new QrCodeDetectionDocument
                {
                    Content = qr.Content,
                    FrameNumber = qr.FrameNumber,
                    TimestampSeconds = qr.TimestampSeconds,
                    FramePath = qr.FramePath
                }).ToList() ?? new List<QrCodeDetectionDocument>()
            };

            // Since VideoId is now the _id, we can use it directly for upsert
            await _resultsCollection.ReplaceOneAsync(
                Builders<CompletedEventDocument>.Filter.Eq("_id", completedEvent.VideoId), 
                document, 
                new ReplaceOptions { IsUpsert = true });

            // Update status with completion message
            var uniqueQrCodes = completedEvent.QrCodeDetections?
                .GroupBy(qr => qr.Content)
                .Count() ?? 0;

            string message;
            if (uniqueQrCodes == 0)
            {
                message = "Processing completed with no QR codes detected";
            }
            else
            {
                message = $"Processing completed with {uniqueQrCodes} unique QR codes detected";
            }

            await _statusService.UpdateStatusAsync(completedEvent.VideoId, "Completed", message);
        }

        public async Task<CompletedEvent?> GetCompletedResultsAsync(string videoId)
        {
            // Since VideoId is now the _id, we can find by _id directly
            var document = await _resultsCollection.Find(
                Builders<CompletedEventDocument>.Filter.Eq("_id", videoId))
                .FirstOrDefaultAsync();

            if (document == null)
                return null;

            return new CompletedEvent
            {
                VideoId = document.VideoId,
                CompletedAt = document.CompletedAt,
                QrCodeDetections = document.QrCodeDetections?.Select(qr => new QrCodeDetection
                {
                    Content = qr.Content,
                    FrameNumber = qr.FrameNumber,
                    TimestampSeconds = qr.TimestampSeconds,
                    FramePath = qr.FramePath
                }).ToList() ?? new List<QrCodeDetection>()
            };
        }
    }
}
