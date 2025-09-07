using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    public interface IVideoStatusService
    {
        Task UpdateStatusAsync(string videoId, string status, string? message = null);

        Task<ProcessingStatusEvent?> GetStatusAsync(string videoId);
    }
}
