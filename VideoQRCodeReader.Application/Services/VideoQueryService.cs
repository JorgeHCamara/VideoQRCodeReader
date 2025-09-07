using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.DTOs;

namespace VideoQRCodeReader.Application.Services
{
    /// <summary>
    /// Application service for video status and results retrieval
    /// Handles mapping between domain entities and response DTOs
    /// Follows Clean Architecture principles
    /// </summary>
    public class VideoQueryService : IVideoQueryService
    {
        private readonly IVideoStatusService _videoStatusService;
        private readonly IVideoResultsService _videoResultsService;

        public VideoQueryService(IVideoStatusService videoStatusService, IVideoResultsService videoResultsService)
        {
            _videoStatusService = videoStatusService;
            _videoResultsService = videoResultsService;
        }

        public async Task<VideoStatusResponse?> GetVideoStatusAsync(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return null;

            var status = await _videoStatusService.GetStatusAsync(videoId);
            if (status == null)
                return null;

            return new VideoStatusResponse
            {
                VideoId = status.VideoId,
                Status = status.Status,
                Message = status.Message,
                UpdatedAt = status.UpdatedAt
            };
        }

        public async Task<VideoResultsResponse?> GetVideoResultsAsync(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return null;

            var results = await _videoResultsService.GetCompletedResultsAsync(videoId);
            
            if (results != null)
            {
                // Video is completed - return full results
                return new VideoResultsResponse
                {
                    VideoId = results.VideoId,
                    Status = "Completed",
                    CompletedAt = results.CompletedAt,
                    QrCodeDetections = results.QrCodeDetections.Select(qr => new QrCodeDetectionResponse
                    {
                        Content = qr.Content,
                        FrameNumber = qr.FrameNumber,
                        TimestampSeconds = qr.TimestampSeconds,
                        FramePath = qr.FramePath
                    }).ToList()
                };
            }

            // Check if video exists but not completed yet
            var status = await _videoStatusService.GetStatusAsync(videoId);
            if (status == null)
                return null; // Video doesn't exist

            // Video exists but not completed yet
            return new VideoResultsResponse
            {
                VideoId = videoId,
                Status = status.Status,
                Message = "Video processing not yet completed",
                QrCodeDetections = new List<QrCodeDetectionResponse>()
            };
        }
    }
}
