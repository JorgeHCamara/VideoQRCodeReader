using Microsoft.AspNetCore.Mvc;
using VideoQRCodeReader.Contracts.Events;
using VideoQRCodeReader.Infrastructure.Interfaces;

namespace VideoQRCodeReader.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMessageQueueService _messageQueueService;

        public VideoController(IFileStorageService fileStorageService, IMessageQueueService messageQueueService)
        {
            _fileStorageService = fileStorageService;
            _messageQueueService = messageQueueService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || (file.ContentType != "video/mp4" && file.ContentType != "video/x-msvideo"))
                return BadRequest("Formato inválido. Use .mp4 ou .avi.");

            var videoId = Guid.NewGuid().ToString();
            var filePath = await _fileStorageService.SaveFileAsync(file, videoId);

            var videoEvent = new VideoUploadedEvent
            {
                VideoId = videoId,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };

            await _messageQueueService.PublishAsync(videoEvent);

            return Ok(new
            {
                VideoId = videoId,
                Status = "Na Fila",
                FilePath = filePath
            });
        }
    }
}
