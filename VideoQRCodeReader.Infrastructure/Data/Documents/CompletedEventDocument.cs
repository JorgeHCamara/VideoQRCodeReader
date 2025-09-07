using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VideoQRCodeReader.Infrastructure.Data.Documents
{
    public class CompletedEventDocument
    {
        [BsonId]
        [BsonElement("_id")]
        public string VideoId { get; set; } = string.Empty;

        [BsonElement("completed_at")]
        public DateTime CompletedAt { get; set; }

        [BsonElement("qr_code_detections")]
        public List<QrCodeDetectionDocument> QrCodeDetections { get; set; } = new();
    }

    public class QrCodeDetectionDocument
    {
        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("frame_number")]
        public int FrameNumber { get; set; }

        [BsonElement("timestamp_seconds")]
        public double TimestampSeconds { get; set; }

        [BsonElement("frame_path")]
        public string FramePath { get; set; } = string.Empty;
    }
}
