using MongoDB.Driver;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Data.Documents;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class VideoStatusService : IVideoStatusService
    {
        private readonly IMongoCollection<ProcessingStatusDocument> _statusCollection;

        public VideoStatusService(IMongoDatabase database)
        {
            _statusCollection = database.GetCollection<ProcessingStatusDocument>("processing_status");
        }

        public async Task UpdateStatusAsync(string videoId, string status, string? message = null)
        {
            var document = new ProcessingStatusDocument
            {
                VideoId = videoId,
                Status = status,
                UpdatedAt = DateTime.UtcNow,
                Message = message
            };

            // Since VideoId is now the _id, we can use it directly for upsert
            await _statusCollection.ReplaceOneAsync(
                Builders<ProcessingStatusDocument>.Filter.Eq("_id", videoId), 
                document, 
                new ReplaceOptions { IsUpsert = true });
        }

        public async Task<ProcessingStatusEvent?> GetStatusAsync(string videoId)
        {
            // Since VideoId is now the _id, we can find by _id directly
            var document = await _statusCollection.Find(
                Builders<ProcessingStatusDocument>.Filter.Eq("_id", videoId))
                .FirstOrDefaultAsync();

            if (document == null)
                return null;

            return new ProcessingStatusEvent
            {
                VideoId = document.VideoId,
                Status = document.Status,
                UpdatedAt = document.UpdatedAt,
                Message = document.Message
            };
        }
    }
}
