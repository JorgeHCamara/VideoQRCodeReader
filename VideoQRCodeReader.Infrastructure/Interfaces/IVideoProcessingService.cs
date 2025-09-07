using VideoQRCodeReader.Domain.Models;

namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    /// <summary>
    /// Service responsible for video frame extraction using FFMpeg
    /// </summary>
    public interface IVideoProcessingService
    {
        /// <summary>
        /// Extracts frames from a video file at specified intervals
        /// </summary>
        /// <param name="videoFilePath">Path to the video file</param>
        /// <param name="outputDirectory">Directory to save extracted frames</param>
        /// <param name="frameInterval">Interval between frames in seconds (default: 1 second)</param>
        /// <returns>List of paths to extracted frame images</returns>
        Task<List<string>> ExtractFramesAsync(string videoFilePath, string outputDirectory, double frameInterval = 1.0);
        
        /// <summary>
        /// Gets video information like duration, resolution, etc.
        /// </summary>
        /// <param name="videoFilePath">Path to the video file</param>
        /// <returns>Video metadata information</returns>
        Task<Video> GetVideoInfoAsync(string videoFilePath);
    }
}
