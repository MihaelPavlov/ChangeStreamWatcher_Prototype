namespace ChangeStreamWatcher_Blazor.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using ChangeStreamWatcher_Blazor.Services;
    using MongoDB.Bson.Serialization.Attributes;

    public class MongoLogDocument : ILogDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("t")]
        public DateTime Timestamp { get; set; }

        [BsonElement("s")]
        public LogEventLevel Severity { get; set; }

        [BsonElement("mn")]
        [BsonIgnoreIfNull]
        public string MachineName { get; set; }

        [BsonElement("ip")]
        [BsonIgnoreIfNull]
        public string IpAddress { get; set; }

        [BsonElement("u")]
        [BsonIgnoreIfNull]
        public string Username { get; set; }

        [BsonElement("m")]
        public string Message { get; set; }

        [BsonElement("e")]
        [BsonIgnoreIfNull]
        public string Exception { get; set; }

        [BsonElement("ie")]
        [BsonIgnoreIfNull]
        public string InnerException { get; set; }

        [BsonElement("btid")]
        [BsonIgnoreIfNull]
        public string BackgroundTaskId { get; set; }
    }
}
