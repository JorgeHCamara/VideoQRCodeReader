using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    /// <summary>
    /// Service for tracking and retrieving video processing status
    /// </summary>
    public interface IVideoStatusService
    {
        /// <summary>
        /// Updates the processing status for a video
        /// </summary>
        Task UpdateStatusAsync(string videoId, string status, string? message = null);

        /// <summary>
        /// Gets the current processing status for a video
        /// </summary>
        Task<ProcessingStatusEvent?> GetStatusAsync(string videoId);

        /// <summary>
        /// Stores the completed results for a video
        /// </summary>
        Task StoreCompletedResultsAsync(CompletedEvent completedEvent);

        /// <summary>
        /// Gets the completed results for a video
        /// </summary>
        Task<CompletedEvent?> GetCompletedResultsAsync(string videoId);
    }
}
