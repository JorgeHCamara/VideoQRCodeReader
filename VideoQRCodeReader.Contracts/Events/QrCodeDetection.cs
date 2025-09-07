namespace VideoQRCodeReader.Contracts.Events
{
    public class QrCodeDetection
    {
        public string Content { get; set; } = string.Empty;
        public double TimestampSeconds { get; set; }
        public int FrameNumber { get; set; }
        public string FramePath { get; set; } = string.Empty;
    }
}
