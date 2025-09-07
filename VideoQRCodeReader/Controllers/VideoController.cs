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
        private readonly IVideoQueryService _videoQueryService;

        public VideoController(
            IVideoUploadService videoUploadService,
            IVideoQueryService videoQueryService)
        {
            _videoUploadService = videoUploadService;
            _videoQueryService = videoQueryService;
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
            try
            {
                if (string.IsNullOrWhiteSpace(videoId))
                {
                    return BadRequest("Video ID is required");
                }

                var status = await _videoQueryService.GetVideoStatusAsync(videoId);
                
                if (status == null)
                {
                    return NotFound($"No processing status found for video ID: {videoId}");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error occurred: {ex.Message}");
            }
        }

        [HttpGet("{videoId}/results")]
        public async Task<IActionResult> GetResults(string videoId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(videoId))
                {
                    return BadRequest("Video ID is required");
                }

                var results = await _videoQueryService.GetVideoResultsAsync(videoId);
                
                if (results == null)
                {
                    return NotFound($"No video found with ID: {videoId}");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                // TODO: Add proper logging
                return StatusCode(500, $"Internal server error occurred: {ex.Message}");
            }
        }
    }
}
