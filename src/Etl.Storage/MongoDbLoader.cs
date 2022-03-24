using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class MongoDbLoader : Loader
    {
        [XmlAttribute]
        public string ConnectionName { get; set; }

        [XmlAttribute]
        public int MaxConcurency { get; set; } = 10;

        [XmlAttribute]
        public string CollectionName { get; set; }

        private readonly List<Task> _tasks = new();
        private Lazy<IMongoDatabase> _lazyDb;

        public override void Initialize(IConfiguration appSetting, string inputFile, IReadOnlyCollection<FieldBase> fields)
        {
            _lazyDb = new Lazy<IMongoDatabase>(() =>
            {
                var dbConnection = appSetting.GetSection($"MongoDb:{ConnectionName ?? "Default"}").Get<MongoDbConnection>();
                var client = new MongoClient(dbConnection.GetConnectionString());
                return client.GetDatabase(dbConnection.DbName);
            });
        }

        public override void ProcessBatch(BatchResult parseResult)
        {
            int sleep = 0;
            while (_tasks.Count >= MaxConcurency)
            {
                sleep += 200;
                Thread.Sleep(200);
            }
            if (sleep > 0)
                Console.WriteLine($"Sleep...{sleep} ms");

            Task t = null;
            lock (this)
            {
                _tasks.Add(t = new(async () =>
               {
                   try { await OnSave(parseResult.Batch); }
                   finally { lock (this) { _tasks.Remove(t); } }
               }));
            }

            t.Start();
        }

        protected virtual Task OnSave(List<IDictionary<string, object>> batch)
        {
            if (batch == null || batch.Count == 0)
                return Task.CompletedTask;

            var collection = _lazyDb.Value.GetCollection<BsonDocument>(CollectionName);
            return collection.InsertManyAsync(batch.Select(e => new BsonDocument(e)));
        }

        public override void WaitToComplete()
        {
            Task[] tasks = null;
            lock (this)
            {
                tasks = _tasks.ToArray();
            }

            Task.WaitAll(tasks);
        }
    }
}
