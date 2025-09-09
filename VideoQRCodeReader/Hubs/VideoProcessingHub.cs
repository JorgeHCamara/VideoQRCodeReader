using Microsoft.AspNetCore.SignalR;

namespace VideoQRCodeReader.Hubs
{
    public class VideoProcessingHub : Hub
    {
        public async Task JoinGroup(string videoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"video_{videoId}");
        }

        public async Task LeaveGroup(string videoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video_{videoId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
