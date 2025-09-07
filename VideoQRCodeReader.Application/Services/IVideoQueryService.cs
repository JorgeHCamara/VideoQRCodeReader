using VideoQRCodeReader.Contracts.DTOs;

namespace VideoQRCodeReader.Application.Services
{
    /// <summary>
    /// Application service interface for video status and results retrieval
    /// Follows Clean Architecture by keeping business logic separate from presentation
    /// </summary>
    public interface IVideoQueryService
    {
        /// <summary>
        /// Gets the current processing status for a video
        /// </summary>
        Task<VideoStatusResponse?> GetVideoStatusAsync(string videoId);

        /// <summary>
        /// Gets the processing results for a video
        /// </summary>
        Task<VideoResultsResponse?> GetVideoResultsAsync(string videoId);
    }
}
