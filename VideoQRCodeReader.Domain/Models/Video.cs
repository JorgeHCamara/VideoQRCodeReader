namespace VideoQRCodeReader.Domain.Models
{
    /// <summary>
    /// Core domain entity representing a video file in the system
    /// Contains both business metadata and technical specifications
    /// </summary>
    public class Video
    {
        // Business Properties (Identity & Tracking)
        public string VideoId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        
        // Technical Properties (Media Information)
        public TimeSpan Duration { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        
        // Computed Business Properties
        public string FileExtension => Path.GetExtension(FileName);
        public bool FileExists => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);
        public TimeSpan Age => DateTime.UtcNow - UploadedAt;
        public string Resolution => $"{Width}x{Height}";
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
        
        // Business Logic Methods
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(VideoId) 
                && !string.IsNullOrEmpty(FileName) 
                && !string.IsNullOrEmpty(FilePath)
                && Duration > TimeSpan.Zero
                && Width > 0 
                && Height > 0;
        }
        
        public bool IsProcessable()
        {
            return IsValid() && FileExists && IsVideoFormat();
        }
        
        public bool IsVideoFormat()
        {
            var validExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };
            return validExtensions.Contains(FileExtension.ToLowerInvariant());
        }
        
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}
