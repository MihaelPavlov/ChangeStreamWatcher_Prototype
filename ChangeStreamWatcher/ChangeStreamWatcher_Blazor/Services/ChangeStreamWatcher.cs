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

        public async Task Start()
        {
            if (this._initialized)
            {
                return;
                throw new InvalidOperationException("This watcher has already been initialized!");
            }

            this._counter++;
            this._initialized = true;
            await StartWatching();
        }

        public async Task StartWatching()
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/TestDatabase");

            var database = dbClient.GetDatabase("TestDatabase");
            var collection = database.GetCollection<BsonDocument>("TestData");
            CreateData(collection);
            using (var cursor = await collection.WatchAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    if (cursor.Current.Any())
                    {

                        if (cursor.Current.First().OperationType != ChangeStreamOperationType.Invalidate)
                        {
                            CreateLogInTheDatabase(cursor.Current.First(), database);
                            Console.WriteLine(cursor.Current.First().OperationType);
                            Console.WriteLine(cursor.Current.First().FullDocument);
                            Console.WriteLine(cursor);

                        }
                        else
                        {
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
                { "date-time", $"{DateTime.UtcNow}"} ,
                { "operation_type", $"{cursor.OperationType}"},
                {"document_key", $"{cursor.DocumentKey}"} ,
                {"collection_namespace", $"{cursor.CollectionNamespace}"} ,
                { "clusture_time", $"{cursor.ClusterTime}"},
                { "backing_document", $"{cursor.BackingDocument}"} ,
                { "full_document", $"{cursor.FullDocument}"} ,
                { "update_description", $"{cursor.UpdateDescription}"} ,
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
                    new BsonDocument{ {"type", "exam"}, {"score", 88.12334193287023 } },
                    new BsonDocument{ {"type", "quiz"}, {"score", 74.92381029342834 } },
                    new BsonDocument{ {"type", "homework"}, {"score", 89.97929384290324 } },
                    new BsonDocument{ {"type", "homework"}, {"score", 82.12931030513218 } }
                    }
                },
                { "class_id", 480}
            };

            await collection.InsertOneAsync(document);
        }
        public  List<BsonDocument> GetCollectionByName(string collectionName)
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/TestDatabase");
            var db = dbClient.GetDatabase("TestDatabase");

            IMongoCollection<BsonDocument> dbCollection = db.GetCollection<BsonDocument>(collectionName);
            var documents =  dbCollection.Find(new BsonDocument()).ToList();
            return documents;

        }
    }

}

