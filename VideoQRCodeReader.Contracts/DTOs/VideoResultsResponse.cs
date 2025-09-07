namespace VideoQRCodeReader.Contracts.DTOs
{
    /// <summary>
    /// Response DTO for video processing results endpoint
    /// </summary>
    public class VideoResultsResponse
    {
        public string VideoId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? Message { get; set; }
        public List<QrCodeDetectionResponse> QrCodeDetections { get; set; } = new();
    }
}
