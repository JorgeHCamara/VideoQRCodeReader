using MassTransit;
using VideoQRCodeReader.Infrastructure.Interfaces;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class MassTransitQueueService : IMessageQueueService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitQueueService(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T message) where T : class
        {
            await _publishEndpoint.Publish(message);
        }
    }
}
