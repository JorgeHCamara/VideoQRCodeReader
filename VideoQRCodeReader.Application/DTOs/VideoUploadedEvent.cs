namespace VideoQRCodeReader.Application.DTOs
{
    public class VideoUploadedEvent
    {
        public string VideoId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
