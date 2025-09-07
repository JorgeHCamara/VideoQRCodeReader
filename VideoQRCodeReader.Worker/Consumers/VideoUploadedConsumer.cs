using MassTransit;
using VideoQRCodeReader.Contracts.Events;

namespace VideoQRCodeReader.Worker.Consumers
{
    public class VideoUploadedConsumer : IConsumer<VideoUploadedEvent>
    {
        public async Task Consume(ConsumeContext<VideoUploadedEvent> context)
        {
            var message = context.Message;
            Console.WriteLine($"Vídeo recebido para processamento: {message.VideoId} - {message.FilePath}");
            await Task.CompletedTask;
        }
    }
}
