using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    public interface IVideoResultsService
    {
        Task StoreCompletedResultsAsync(CompletedEvent completedEvent);

        Task<CompletedEvent?> GetCompletedResultsAsync(string videoId);
    }
}
