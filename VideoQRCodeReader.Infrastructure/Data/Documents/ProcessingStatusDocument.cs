using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VideoQRCodeReader.Infrastructure.Data.Documents
{
    public class ProcessingStatusDocument
    {
        [BsonId]
        [BsonElement("_id")]
        public string VideoId { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("message")]
        public string? Message { get; set; }
    }
}
