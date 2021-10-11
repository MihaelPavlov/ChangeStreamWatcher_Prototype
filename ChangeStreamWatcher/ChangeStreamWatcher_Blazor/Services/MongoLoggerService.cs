namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ChangeStreamWatcher_Blazor.Services.Interfaces;
    using ChangeStreamWatcher_Blazor.Services.Internal;

    public class MongoLoggerService<TKey> : ILoggerService, IInformationConsumer<TKey>, IDisposable, IAsyncDisposable
      where TKey : struct, IEquatable<TKey>
    {
        private const int DEFAULT_TIMER_PERIOD = 1_000 * 10;
        private const int DEFAULT_BATCH_LIMIT = 100;

        private bool _isInitialized;
        private readonly Timer _timer;
        private readonly MultiThreadedQueue<ILogDocument> _logs;
        private readonly IQuantumConsoleLogger consoleLogger;

        public ILogEnricher Enricher { get; }
        public ILogDocumentFactory Factory { get; }
        public ILogEmitter Emitter { get; }
        public LogEventLevel MinimumLogLevel { get; private set; }

        // The `timerPeriod` and `internalBatchLimit` arguments should be nullable integers, because the Dependency Injection Container passes random integers for some reason if they are non-nullable with default value.
        public MongoLoggerService(
            ILogEnricher enricher,
            ILogDocumentFactory factory,
            ILogEmitter emitter,
            IInformationDistributor<TKey> informationDistributor,
            IQuantumConsoleLogger consoleLogger,
            int? timerPeriod = null,
            int? internalBatchLimit = null)
        {
            this.Enricher = enricher ?? throw new ArgumentNullException(nameof(enricher));
            this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.Emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            this.consoleLogger = consoleLogger ?? throw new ArgumentNullException(nameof(consoleLogger));

            if (informationDistributor is null)
                throw new ArgumentNullException(nameof(informationDistributor));
            informationDistributor.DistributeTo(this);

            var period = TimeSpan.FromMilliseconds(timerPeriod ?? DEFAULT_TIMER_PERIOD);
            this._timer = new Timer(this.OnTimerTick, null, period, period);

            this._logs = new MultiThreadedQueue<ILogDocument>(internalBatchLimit ?? DEFAULT_BATCH_LIMIT);
        }

        public void Log(LogEventLevel level, string message)
        {
            // This if is important, it will skip log messages with a lower-than-minimum set log level.
            if (level < this.MinimumLogLevel || string.IsNullOrWhiteSpace(message))
                return;

            if (this._isInitialized == false)
                this.LogToSystemLogger(level, message);

            var logEvent = this.Factory.FromMessage(level, message);

            this.EnrichAndLog(logEvent);
        }

        public void Log(LogEventLevel level, Exception exception, string message = default)
        {
            // This if is important, it will skip log messages with a lower-than-minimum set log level.
            if (level < this.MinimumLogLevel || exception is null)
                return;

            if (this._isInitialized == false)
                this.LogToSystemLogger(level, message);

            var logEvent = this.Factory.FromException(level, exception, message);

            this.EnrichAndLog(logEvent);
        }

        private void LogToSystemLogger(LogEventLevel level, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            this.consoleLogger.Log(level, message);
        }

        public void LogError(Exception exception, string message = default)
            => this.Log(LogEventLevel.Error, exception, message);

        public void Debug(string message)
            => this.Log(LogEventLevel.Debug, message);

        public void Consume(Information<TKey> information)
        {
            // We should not care about any threading issues in this method because nothing should happen until the service is not initialized yet.
            if (information is null)
                return;

            this.MinimumLogLevel = information.MinimumLoggingLevel;
            this._isInitialized = true;
        }

        public async Task EmitAllLogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var allLogs = this._logs.FlushAllData();

                if (allLogs is null || allLogs.Any() == false)
                    return;

                await this.Emitter.EmitBatchAsync(allLogs, cancellationToken).ConfigureAwait(false);
            }
            // We are suppressing this catch so it captures every exception in a fire-and-forget design.
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // Logging exceptions should always be captured to avoid possible issues related to bad logging design and error.
                this.consoleLogger.LogError(ex, "MongoLoggerService EmitBatchAsync logging exception.");
            }
        }

        private void EmitAndForgetAllLogs(CancellationToken cancellationToken)
        {
#pragma warning disable 4014
            this.EmitAllLogsAsync(cancellationToken);
#pragma warning restore 4014
        }

        private void OnTimerTick(object state)
        {
            this.EmitAndForgetAllLogs(CancellationToken.None);
        }

        private void EnrichAndLog(ILogDocument logDocument)
        {
            // Add system details to the message before enqueuing it
            this.Enricher.Enrich(logDocument);

            this._logs.Enqueue(logDocument);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._logs.Clear();
                this._timer.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (this._timer != null)
                await this._timer.DisposeAsync().ConfigureAwait(false);


            // Dispose of unmanaged resources.
            this.Dispose(true);

            this.EmitAndForgetAllLogs(CancellationToken.None);

            GC.SuppressFinalize(this);
        }
    }
}
