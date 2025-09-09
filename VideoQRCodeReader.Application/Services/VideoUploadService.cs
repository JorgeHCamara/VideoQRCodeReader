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
        private readonly IVideoStatusService _videoStatusService;

        public VideoUploadService(
            IFileStorageService fileStorageService, 
            IMessageQueueService messageQueueService,
            IVideoStatusService videoStatusService)
        {
            _fileStorageService = fileStorageService;
            _messageQueueService = messageQueueService;
            _videoStatusService = videoStatusService;
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

            // Set initial status
            await _videoStatusService.UpdateStatusAsync(videoId, "Queued", "Video uploaded and queued for processing");

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

            if (file.Length == 0)
                throw new ArgumentException("File is empty");

            // Check file size (100MB limit to match frontend)
            var maxSize = 100 * 1024 * 1024; // 100MB
            if (file.Length > maxSize)
                throw new ArgumentException("File size must be less than 100MB");

            // Check file type - be more permissive to match frontend expectations
            var allowedTypes = new[] { 
                "video/mp4", 
                "video/x-msvideo", 
                "video/avi",
                "video/mov",
                "video/quicktime"
            };

            if (!allowedTypes.Contains(file.ContentType?.ToLower()))
                throw new ArgumentException("Invalid format. Supported formats: MP4, AVI, MOV");

            // Future: Add codec validation, etc.
        }

        private static string GenerateVideoId()
        {
            // Future: Could implement custom ID generation logic
            return Guid.NewGuid().ToString();
        }
    }
}
