using Microsoft.AspNetCore.Mvc;
using VideoQRCodeReader.Application.Services;
using VideoQRCodeReader.Contracts.DTOs;

namespace VideoQRCodeReader.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoUploadService _videoUploadService;

        public VideoController(IVideoUploadService videoUploadService)
        {
            _videoUploadService = videoUploadService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            try
            {
                var result = await _videoUploadService.ProcessUploadAsync(file);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log error here - using ex for logging
                // TODO: Add proper logging
                return StatusCode(500, $"Internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("{videoId}/status")]
        public async Task<IActionResult> GetProcessingStatus(string videoId)
        {
            // TODO: Implement status retrieval from repository/cache
            // This will be implemented when we add the repository pattern
            await Task.CompletedTask;
            
            return Ok(new
            {
                VideoId = videoId,
                Status = "Processing", // Placeholder - will be retrieved from storage
                Message = "Video is being processed"
            });
        }

        [HttpGet("{videoId}/results")]
        public async Task<IActionResult> GetResults(string videoId)
        {
            // TODO: Implement results retrieval from repository
            // This will be implemented when we add the repository pattern
            await Task.CompletedTask;
            
            return Ok(new
            {
                VideoId = videoId,
                Status = "Completed", // Placeholder - will be retrieved from storage
                QrCodeDetections = new[]
                {
                    new { Content = "Sample QR Code", TimestampSeconds = 1.5 }
                }
            });
        }
    }
}
