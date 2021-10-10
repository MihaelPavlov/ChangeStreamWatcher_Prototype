namespace ChangeStreamWatcher_Blazor.Services
{
    using System;

    public interface ILogDocumentFactory
    {
        ILogDocument FromMessage(LogEventLevel severity, string message);
        ILogDocument FromException(LogEventLevel severity, Exception exception, string message = default);
    }
}