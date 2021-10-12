namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using ChangeStreamWatcher_Blazor.Data;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class ChangeStreamWatcher
    {
        public ChangeStreamOptions _options;
        public CancellationToken _cancelToken;

        /// <summary>
        /// True, if this instance is initialised and watching.
        /// </summary>
        public bool Initialized => this._initialized;
        public int Counter => this._counter;
        public string OperationType => this._operationType;

        private bool _initialized;
        private int _counter;
        private string _operationType;
        private PipelineDefinition<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>> _pipeline;
        public async Task Start(EventTypes eventTypes, FilterDefinition<ChangeStreamDocument<BsonDocument>> filter = null)
        {
            if (this._initialized)
            {
                return;
                throw new InvalidOperationException("This watcher has already been initialized!");
            }
            this._counter++;

            var databaseOperationTypes = new HashSet<ChangeStreamOperationType>() { ChangeStreamOperationType.Invalidate };

            if ((eventTypes & EventTypes.Created) != 0)
                databaseOperationTypes.Add(ChangeStreamOperationType.Insert);

            if ((eventTypes & EventTypes.Updated) != 0)
            {
                databaseOperationTypes.Add(ChangeStreamOperationType.Update);
                databaseOperationTypes.Add(ChangeStreamOperationType.Replace);
            }

            if ((eventTypes & EventTypes.Deleted) != 0)
                databaseOperationTypes.Add(ChangeStreamOperationType.Delete);

            //Test all
            if (eventTypes == 0)
            {
                databaseOperationTypes.Add(ChangeStreamOperationType.Insert);
                databaseOperationTypes.Add(ChangeStreamOperationType.Update);
                databaseOperationTypes.Add(ChangeStreamOperationType.Replace);
                databaseOperationTypes.Add(ChangeStreamOperationType.Delete);
            }
            var filters = Builders<ChangeStreamDocument<BsonDocument>>.Filter.Where(x => databaseOperationTypes.Contains(x.OperationType));

            if (filter != null)
                filters &= filter;

            /// <summary>
            ///  For example, if you are only interested in monitoring inserted documents,
            ///  you could use a pipeline to filter the change stream to only include insert operations.
            /// </summary>
            /// <example>
            ///      var pipeline = 
            ///      new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
            ///      .Match(x => x.OperationType == ChangeStreamOperationType.Insert);
            /// </example>
            this._pipeline = new IPipelineStageDefinition[] {
                PipelineStageDefinitionBuilder.Match(filters),

                PipelineStageDefinitionBuilder.Project<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>>(@"
                {
                    _id: 1,
                    operationType: 1,
                    fullDocument: { $ifNull: ['$fullDocument', '$documentKey'] }
                }"),
            };

            /// <sumary>
            ///     FullDocument can be set to ChangeStreamFullDocumentOption.UpdateLookup if you want the change stream 
            ///     event for Update operations to include a copy of the full document (the full document might include additional changes 
            ///     that are the result of subsequent change events, see the server documentation here).
            /// </sumary>
            var changeStreamOptions = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            this._initialized = true;

            await StartWatching(this._pipeline, changeStreamOptions);
        }

        public async Task StartWatching(PipelineDefinition<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>> pipeline, ChangeStreamOptions changeStreamOptions)
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/TestDatabase");

            var database = dbClient.GetDatabase("TestDatabase");
            var collection = database.GetCollection<BsonDocument>("TestData");
            await CreateData(collection);
            using (var cursor = await collection.WatchAsync(pipeline, changeStreamOptions).ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync().ConfigureAwait(false))
                {
                    if (cursor.Current.Any())
                    {
                        if (cursor.Current.First().OperationType != ChangeStreamOperationType.Invalidate)
                        {
                            cursor.Current.First().FullDocument.Remove("_id");
                            var dotNetObj = BsonTypeMapper.MapToDotNetValue(cursor.Current.First().FullDocument);
                            var json = JsonSerializer.Serialize(dotNetObj);

                            var serializeObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                            await CreateLogInDB(cursor.Current.First(), database, serializeObject);

                        }
                    }

                }

            }

        }
        public async Task CreateLogInDB(ChangeStreamDocument<BsonDocument> cursor, IMongoDatabase database, Dictionary<string, object> keyValuePairs)
        {
            var collection = database.GetCollection<Log>("Logs");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var kvp in keyValuePairs)
            {
                dic.Add(kvp.Key, kvp.Value.ToString());
            }
            var document = new Log
            {
                Id = Guid.NewGuid().ToString(),
                OperationType = cursor.OperationType.ToString(),
                FullDocument = cursor.FullDocument.ToString(),
                KeyValuePairs = dic,
            };

            await collection.InsertOneAsync(document);
        }
        public async Task CreateData(IMongoCollection<BsonDocument> collection)
        {
            var document = new BsonDocument
            {
                { "student_id", 10000 },
                { "FirstName" , "Gosho"},
                { "LastName" , "Soc"}
            };

            await collection.InsertOneAsync(document);
        }
        public List<BsonDocument> GetCollectionByName(string collectionName)
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/TestDatabase");
            var db = dbClient.GetDatabase("TestDatabase");

            IMongoCollection<BsonDocument> dbCollection = db.GetCollection<BsonDocument>(collectionName);
            var documents = dbCollection.Find(new BsonDocument()).ToList();
            return documents;

        }
    }
    [Flags]
    public enum EventTypes
    {
        /// <summary>
        /// Created, fired on a document created event.
        /// </summary>
        Created = 1 << 1,

        /// <summary>
        /// Updated, fired on a document updated event.
        /// </summary>
        Updated = 1 << 2,

        /// <summary>
        /// Deleted, fired on a document deleted event.
        /// </summary>
        Deleted = 1 << 3
    }


}

