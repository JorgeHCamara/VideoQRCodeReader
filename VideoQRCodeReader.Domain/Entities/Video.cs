namespace VideoQRCodeReader.Domain.Entities
{
    public class Video
    {
        public string VideoId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
