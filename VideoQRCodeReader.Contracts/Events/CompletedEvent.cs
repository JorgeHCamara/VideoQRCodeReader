namespace VideoQRCodeReader.Contracts.Events
{
    public class CompletedEvent
    {
        public string VideoId { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public List<QrCodeDetection> QrCodeDetections { get; set; } = new();
    }
}
