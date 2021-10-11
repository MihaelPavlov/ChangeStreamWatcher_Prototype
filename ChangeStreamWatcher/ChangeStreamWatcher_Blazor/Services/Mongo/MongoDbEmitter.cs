using ChangeStreamWatcher_Blazor.Services.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Services.Mongo
{
    /// <summary>
    /// A class responsible for emitting a batches of logs. 
    /// If not initialized correctly, it will use the console logger to log any logs, matched by severity.
    /// </summary>
    /// <typeparam name="TKey">The type of the unique identifier of the contained entities within our databases.</typeparam>
    public class MongoDbEmitter<TKey> : ILogEmitter, IInformationConsumer<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        private Information<TKey> _information;

        private IMongoCollection<ILogDocument> _logsCollection;

        private readonly IDateTimeServer _dateTimeServer;
        private readonly IQuantumConsoleLogger _quantumConsoleLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbEmitter{TKey}"/> class.
        /// </summary>
        /// <param name="tenantInformationDistributor">The <see cref="ITenantInformationDistributor{TKey}"/> used to distribute the current <see cref="TenantInformation{TKey}"/> configuration parameters.</param>
        /// <param name="dateTimeServer">The <see cref="IDateTimeServer"/> used to construct the collection name by the current <see cref="DateTimeOffset"/>.</param>
        /// <param name="quantumConsoleLogger">The <see cref="IQuantumConsoleLogger"/> that should be used if the emitter is not configured properly.</param>
        public MongoDbEmitter(InformationDistributor<TKey> informationDistributor, IDateTimeServer dateTimeServer, IQuantumConsoleLogger quantumConsoleLogger)
        {
            if (informationDistributor is null)
                throw new ArgumentNullException(nameof(informationDistributor));

            informationDistributor.DistributeTo(this);

            this._dateTimeServer = dateTimeServer ?? throw new ArgumentNullException(nameof(dateTimeServer));
            this._quantumConsoleLogger = quantumConsoleLogger ?? throw new ArgumentNullException(nameof(quantumConsoleLogger));
        }

        /// <summary>
        /// Use this method to retrieve the logs from the database for the current collection.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        // This method is used in API's that collect logs, like the Admin Portal.
        public async Task<IEnumerable<ILogDocument>> GetLogs()
        {
            if (this.LogsDatabaseProvided())
            {
                var emptyFilter = Builders<ILogDocument>.Filter.Empty;

                var result = this._logsCollection?.FindAsync(emptyFilter);
                using var asyncCursor = await result.ConfigureAwait(false);
                return await asyncCursor.ToListAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
            }

            return Enumerable.Empty<ILogDocument>();
        }

        /// <inheritdoc />
        public Task EmitBatchAsync(IEnumerable<ILogDocument> documents, CancellationToken cancellationToken)
        {
            if (documents is null || documents.Any() == false)
                return Task.CompletedTask;

            if (cancellationToken.IsCancellationRequested)
                return Task.CompletedTask;

            // We need to verify that the per-tenant logger is initialised. If it is not,
            // We log to the console.
            if (this.LogsDatabaseProvided())
            {
                this.SetTenantDailyLogsCollection();

                // Insert into the database the previously created documents.
                // This method is not awaited on purpose, to allow the application to continue without waiting.
                // RA 20210410 - This method was awaited and I removed the await.
                return this._logsCollection.InsertManyAsync(documents, cancellationToken: cancellationToken);
            }

            // if _tenantInformation or LogsDatabase is null, log to console
            foreach (var item in documents)
            {
                // We need to enhance this by formatting the console output better.
                this._quantumConsoleLogger.Log(item.Severity, item.Message);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Use this method to verify that the logs database exists.
        /// </summary>
        /// <returns>True, if the _tenantInformation object or the set Logs database is not null.</returns>
        private bool LogsDatabaseProvided()
            => this._information?.LogsDatabase != null;

        /// <inheritdoc />
        public void Consume(Information<TKey> information)
            => this._information = information;

        /// <summary>
        /// Use this method to set the correct <see cref="IMongoCollection{TDocument}"/> for logging.
        /// </summary>
        private void SetTenantDailyLogsCollection()
        {
            // At this stage, tenant information can be null.
            var collectionName = $"{this._dateTimeServer.Now:yyyyMMdd}-{this._information.Id}";
            this._logsCollection = this._information.LogsDatabase.GetCollection<ILogDocument>(collectionName);
        }
    }
}
