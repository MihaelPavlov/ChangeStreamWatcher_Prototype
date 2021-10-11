namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using MongoDB.Bson;
    using ChangeStreamWatcher_Blazor.Services.Mongo;

    public class MongoLogDocumentFactory : ILogDocumentFactory
    {
        public ILogDocument FromMessage(LogEventLevel severity, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            return new MongoLogDocument
            {
                Id = ObjectId.GenerateNewId(),
                Severity = severity,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public ILogDocument FromException(LogEventLevel severity, Exception exception, string message = default)
        {
            if (exception is null)
                return null;

            return new MongoLogDocument
            {
                Id = ObjectId.GenerateNewId(),
                Severity = severity,
                Message = string.IsNullOrWhiteSpace(message) ? exception.Message : message,
                Timestamp = DateTime.UtcNow,
                Exception = exception.ToString(),
                InnerException = exception.InnerException?.Message
            };
        }
    }
}
