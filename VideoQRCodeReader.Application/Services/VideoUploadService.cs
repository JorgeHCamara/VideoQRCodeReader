using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Contracts.DTOs;
using Microsoft.AspNetCore.Http;

namespace VideoQRCodeReader.Application.Services
{
    public class VideoUploadService : IVideoUploadService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMessageQueueService _messageQueueService;

        public VideoUploadService(
            IFileStorageService fileStorageService, 
            IMessageQueueService messageQueueService)
        {
            _fileStorageService = fileStorageService;
            _messageQueueService = messageQueueService;
        }

        public async Task<VideoUploadResult> ProcessUploadAsync(IFormFile file)
        {
            // Business validation
            ValidateFile(file);

            // Generate business identifier
            var videoId = GenerateVideoId();
            
            // Store file
            var filePath = await _fileStorageService.SaveFileAsync(file, videoId);

            // Create and publish domain event
            var videoEvent = new VideoUploadedEvent
            {
                VideoId = videoId,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };

            await _messageQueueService.PublishAsync(videoEvent);

            // Return result
            return new VideoUploadResult
            {
                VideoId = videoId,
                Status = "Queued",
                FilePath = filePath
            };
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file == null)
                throw new ArgumentException("File is required");

            if (file.ContentType != "video/mp4" && file.ContentType != "video/x-msvideo")
                throw new ArgumentException("Invalid format. Use .mp4 or .avi");

            // Future: Add file size validation, codec validation, etc.
        }

        private static string GenerateVideoId()
        {
            // Future: Could implement custom ID generation logic
            return Guid.NewGuid().ToString();
        }
    }
}
