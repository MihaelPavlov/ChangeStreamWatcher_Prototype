namespace ChangeStreamWatcher_Blazor.Services
{
    using ChangeStreamWatcher_Blazor.Data;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILoggerService
    {

        ILogDocumentFactory Factory { get; }

        LogEventLevel MinimumLogLevel { get; }

        /// <summary>
        /// Logs a message with a given <see cref="LogEventLevel"/>.
        /// </summary>
        /// <param name="level">The <see cref="LogEventLevel"/> of the log entry.</param>
        /// <param name="message">The message to log.</param>
        ILogDocument Log(LogEventLevel level, string message);

        /// <summary>
        /// Logs an <see cref="Exception"/> and an optional message with a given <see cref="LogEventLevel"/>.
        /// </summary>
        /// <param name="level">The <see cref="LogEventLevel"/> of the log entry.</param>
        /// <param name="exception">The <see cref="Exception"/> to log in the log entry.</param>
        /// <param name="message">The message to log.</param>
        /// <remarks>Internally, this will log the exception's .ToString() method.</remarks>
        void Log(LogEventLevel level, Exception exception, string message = default);

        void LogError(Exception exception, string message = default);

        void Debug(string message);

        Task EmitAllLogsAsync(CancellationToken cancellationToken);
    }
}