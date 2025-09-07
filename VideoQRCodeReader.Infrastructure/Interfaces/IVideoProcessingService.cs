namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    public interface IVideoProcessingService
    {
        Task<List<string>> ExtractFramesAsync(string videoFilePath, string outputDirectory);
        Task<string?> DetectQrCodeAsync(string imagePath);
    }
}
