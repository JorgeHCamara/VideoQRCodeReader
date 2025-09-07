namespace VideoQRCodeReader.Contracts.Events
{
    public class ProcessingStatusEvent
    {
        public string VideoId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string? Message { get; set; }
    }
}
