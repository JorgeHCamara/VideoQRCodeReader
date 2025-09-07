namespace VideoQRCodeReader.Contracts.Events
{
    public class VideoUploadedEvent
    {
        public string VideoId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
