namespace ChangeStreamWatcher_Blazor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        public async Task Start(EventTypes eventTypes, FilterDefinition<ChangeStreamDocument<BsonDocument>> filter)
        {
            if (this._initialized)
            {
                return;
                throw new InvalidOperationException("This watcher has already been initialized!");
            }

            this._counter++;
            this._initialized = true;

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
            var changeStreamOptions = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            var filters = Builders<ChangeStreamDocument<BsonDocument>>.Filter.Where(x => databaseOperationTypes.Contains(x.OperationType));

            if (filter != null)
                filters &= filter;
            this._pipeline= new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match("{ operationType: { $in: [ 'replace', 'insert', 'update' ] } }");

            //this._pipeline = new IPipelineStageDefinition[] {


            //    PipelineStageDefinitionBuilder.Match(filters),

            //    PipelineStageDefinitionBuilder.Project<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>>(@"
            //    {
            //        _id: 1,
            //        operationType: 1,
            //        fullDocument: { $ifNull: ['$fullDocument', '$documentKey'] }
            //    }")
            //};
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
                            var full = cursor.Current.Select(x => x.FullDocument);
                            await CreateLogInTheDatabase(cursor.Current.First(), database);
                        }

                        this._operationType = cursor.Current.First().OperationType.ToString();
                    }

                }

            }

        }
        public async Task CreateLogInTheDatabase(ChangeStreamDocument<BsonDocument> cursor, IMongoDatabase database)
        {
            var collection = database.GetCollection<BsonDocument>("Logs");
            var document = new BsonDocument
            {
                { "_id", Guid.NewGuid().ToString() },
                { "operationType", $"{cursor.OperationType}"},
                { "fullDocument", $"{cursor.FullDocument}"},
            };

            await collection.InsertOneAsync(document);
        }
        public async Task CreateData(IMongoCollection<BsonDocument> collection)
        {
            var document = new BsonDocument
            {
                { "student_id", 10000 },
                { "scores", new BsonArray
                    {
                    new BsonDocument{ {"type", "exam"}, {"score", 88.12334193287023 } }
                    }
                }
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

