namespace VideoQRCodeReader.Contracts.DTOs
{
    public class QrCodeDetectionResponse
    {
        public string Content { get; set; } = string.Empty;
        public int FrameNumber { get; set; }
        public double TimestampSeconds { get; set; }
        public string? FramePath { get; set; }
    }
}
