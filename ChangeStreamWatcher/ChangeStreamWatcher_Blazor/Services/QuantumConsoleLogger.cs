namespace Quantum.DMS.Logging
{
    using ChangeStreamWatcher_Blazor.Data;
    using ChangeStreamWatcher_Blazor.Services;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class QuantumConsoleLogger : IQuantumConsoleLogger
    {
        public ILogEnricher Enricher { get; }
        public ILogDocumentFactory Factory { get; } = new MongoLogDocumentFactory();
        public LogEventLevel MinimumLogLevel { get; } = LogEventLevel.Verbose;

        public ILogDocument Log(LogEventLevel level, string message) => this.Factory.FromMessage(level, message);

        public void Log(LogEventLevel level, Exception exception, string message = default)
        {
            Console.WriteLine($"{level} {message} {exception?.ToString()}");
        }

        public void LogError(Exception exception, string message = default)
            => this.Log(LogEventLevel.Error, exception, message);

        public void Debug(string message) => this.Log(LogEventLevel.Debug, message);

        public Task EmitAllLogsAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}