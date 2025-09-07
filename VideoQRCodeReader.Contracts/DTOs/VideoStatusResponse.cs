namespace VideoQRCodeReader.Contracts.DTOs
{
    /// <summary>
    /// Response DTO for video processing status endpoint
    /// </summary>
    public class VideoStatusResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
