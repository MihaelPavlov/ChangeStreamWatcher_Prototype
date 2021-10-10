namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.Serializers;

    public interface ILogDocument
    {
        public DateTime Timestamp { get; set; }

        public LogEventLevel Severity { get; set; }

        public string MachineName { get; set; }

        public string IpAddress { get; set; }

        public string Username { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

        public string InnerException { get; set; }

        public string BackgroundTaskId { get; set; }
    }
}