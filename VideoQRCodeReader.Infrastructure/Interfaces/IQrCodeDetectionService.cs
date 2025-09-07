namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    /// <summary>
    /// Service responsible for QR code detection in images
    /// </summary>
    public interface IQrCodeDetectionService
    {
        /// <summary>
        /// Detects QR codes in an image file
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        /// <returns>QR code content if found, null otherwise</returns>
        Task<string?> DetectQrCodeAsync(string imagePath);
        
        /// <summary>
        /// Detects multiple QR codes in an image file
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        /// <returns>List of detected QR code contents</returns>
        Task<List<string>> DetectMultipleQrCodesAsync(string imagePath);
    }
}
