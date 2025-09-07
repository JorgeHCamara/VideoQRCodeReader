using VideoQRCodeReader.Infrastructure.Interfaces;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class FFMpegVideoProcessingService : IVideoProcessingService
    {
        public async Task<List<string>> ExtractFramesAsync(string videoFilePath, string outputDirectory)
        {
            // TODO: Implement FFMpeg frame extraction
            // This is a placeholder for the video processing requirement
            await Task.CompletedTask;
            
            // Return empty list for now - will be implemented with FFMpegCore later
            return new List<string>();
        }

        public async Task<string?> DetectQrCodeAsync(string imagePath)
        {
            // TODO: Implement QR code detection using a library like ZXing.Net
            // This is a placeholder for the QR code detection requirement
            await Task.CompletedTask;
            
            // Return null for now - will be implemented with QR detection library later
            return null;
        }
    }
}
