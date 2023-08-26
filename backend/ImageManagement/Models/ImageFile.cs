using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SixLabors.ImageSharp.Formats;
using System;

namespace ImageManagement.Models
{
    public class MetaData
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelTypeInfo PixelType { get; internal set; }
    }
    public class ImageFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public double? Size { get; set; }

        public DateTime CreatedAt { get; set; } = new DateTime();

        public DateTime UpdatedAt { get; set; } = new DateTime();

        public string? UploadedBy { get; set; }

        public MetaData? Metadata { get; set; }

        public string? Description { get; set; }
    }
}
