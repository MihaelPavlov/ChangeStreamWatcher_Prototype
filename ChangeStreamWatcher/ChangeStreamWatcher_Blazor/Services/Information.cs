using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Services
{
    public class Information<TKey>
          where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Gets or sets the tenant identifier this information relates to.
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMongoClient"/>, used in all <see cref="IMongoDatabase"/> among the <see cref="TenantInformation{TKey}"/>.
        /// </summary>
        public IMongoClient MongoClient { get; set; }

        /// <summary>
        /// Gets or sets a connection to the main database.
        /// </summary>
        public IMongoDatabase MainDatabase { get; set; }

        /// <summary>
        /// Gets or sets a connection to the Serilog history database.
        /// </summary>
        public IMongoDatabase LogsDatabase { get; set; }

        /// <summary>
        /// Gets or sets the minimum logging level that should be used upon configuring the logger.
        /// </summary>
        public LogEventLevel MinimumLoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether item operations should be registered into the 'Journal'.
        /// </summary>
        public bool EnableJournal { get; set; }
    }
}
