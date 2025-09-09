using Microsoft.AspNetCore.SignalR;
using VideoQRCodeReader.Hubs;

namespace VideoQRCodeReader.Services
{
    public interface ISignalRNotificationService
    {
        Task SendStatusUpdate(string videoId, string status, string? message = null);
        Task SendProcessingComplete(string videoId, object results);
        Task SendError(string videoId, string error);
    }

    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<VideoProcessingHub> _hubContext;

        public SignalRNotificationService(IHubContext<VideoProcessingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendStatusUpdate(string videoId, string status, string? message = null)
        {
            await _hubContext.Clients.Group($"video_{videoId}").SendAsync("StatusUpdate", new
            {
                VideoId = videoId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendProcessingComplete(string videoId, object results)
        {
            await _hubContext.Clients.Group($"video_{videoId}").SendAsync("ProcessingComplete", new
            {
                VideoId = videoId,
                Status = "Completed",
                Results = results,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendError(string videoId, string error)
        {
            await _hubContext.Clients.Group($"video_{videoId}").SendAsync("ProcessingError", new
            {
                VideoId = videoId,
                Status = "Failed",
                Error = error,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
