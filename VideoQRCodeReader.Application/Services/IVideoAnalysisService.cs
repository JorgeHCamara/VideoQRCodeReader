using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Application.Services
{
    /// <summary>
    /// Application service that orchestrates video analysis workflow
    /// Combines video processing and QR code detection following SOLID principles
    /// </summary>
    public interface IVideoAnalysisService
    {
        /// <summary>
        /// Analyzes a video file by extracting frames and detecting QR codes
        /// </summary>
        /// <param name="videoFilePath">Path to the video file</param>
        /// <param name="videoId">Unique identifier for the video</param>
        /// <returns>List of detected QR codes with their positions</returns>
        Task<List<QrCodeDetection>> AnalyzeVideoAsync(string videoFilePath, string videoId);
    }
}
