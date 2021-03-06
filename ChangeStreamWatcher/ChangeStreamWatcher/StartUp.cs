namespace ChangeStreamWatcher
{
    using System;
    using System.Linq;
    using System.Threading;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class StartUp
    {
        static void Main(string[] args)
        {
            var streamer = new Streamer();
            streamer.StartWatching();

        }
    }

    public class Streamer
    {
        public ChangeStreamOptions _options;
        public bool _resume;
        public CancellationToken _cancelToken;
        public bool _initialized;
        public void StartWatching()
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/TestDatabase");

            var database = dbClient.GetDatabase("TestDatabase");
            var collection = database.GetCollection<BsonDocument>("TestData");
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
            collection.InsertOne(document);
            //var firstDocument = collection.Find(new BsonDocument()).FirstOrDefault();
            //Console.WriteLine(firstDocument.ToString()); 
            //Console.WriteLine($"Database name -> {database.DatabaseNamespace}");
            //Console.WriteLine(collection);
            //Console.WriteLine(collection.CollectionNamespace);


            using (var cursor = collection.Watch())
            {
                while (cursor.MoveNext())
                {
                    if (cursor.Current.Any())
                    {
                        Console.WriteLine("Cursor  " +cursor);
                        Console.WriteLine("operation Type   " +cursor.Current.First().OperationType);
                        Console.WriteLine();
                        Console.WriteLine("bson  " + cursor.ToBson());
                        Console.WriteLine();
                        Console.WriteLine("string  " +cursor.ToString());
                        Console.WriteLine();

                        Console.WriteLine("backing doc   "+cursor.Current.First().BackingDocument.ToJson());
                        Console.WriteLine();

                        Console.WriteLine("clusterTime   "+cursor.Current.First().ClusterTime);
                        Console.WriteLine();

                        Console.WriteLine("collection namespace  " + cursor.Current.First().CollectionNamespace);
                        Console.WriteLine();

                        Console.WriteLine("doc key   "+cursor.Current.First().DocumentKey);
                        Console.WriteLine();

                        Console.WriteLine("fulldoc  "+ cursor.Current.First().FullDocument);
                        Console.WriteLine();

                        Console.WriteLine("Rename to  " + cursor.Current.First().RenameTo);
                        Console.WriteLine();

                        Console.WriteLine("update Description   "+cursor.Current.First().UpdateDescription);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine();


                        //if (cursor.Current.First().OperationType != ChangeStreamOperationType.Invalidate)
                        //{
                        //    Console.WriteLine(cursor.Current.First().OperationType);
                        //    Console.WriteLine(cursor.Current.First().FullDocument);

                        //}
                        //else
                        //{
                        //    Console.WriteLine(cursor.Current.First().OperationType);
                        //    Console.WriteLine(cursor.Current.First().FullDocument);
                        //    Console.WriteLine(cursor);
                        //}
                    }
                }

            }



        }
    }
}
