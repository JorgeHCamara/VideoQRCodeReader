namespace VideoQRCodeReader.Services.Interfaces
{
    public interface IMessageQueueService
    {
        Task PublishAsync<T>(T message) where T : class;
    }
}
