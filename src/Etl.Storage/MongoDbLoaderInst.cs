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
    public class MongoDbLoader : Loader<MongoDbLoaderInst, MongoDbLoader>
    {
        [XmlAttribute]
        public string ConnectionName { get; set; }

        [XmlAttribute]
        public int MaxConcurency { get; set; } = 10;

        [XmlAttribute]
        public string CollectionName { get; set; }
    }

    public class MongoDbLoaderInst : LoaderInst<MongoDbLoaderInst, MongoDbLoader>
    {
        private readonly IConfiguration _configuration;
        private readonly List<Task> _tasks = new();
        
        private MongoDbLoader _args;
        private Lazy<IMongoDatabase> _lazyDb;

        public MongoDbLoaderInst(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Initalize(MongoDbLoader args, string inputFile, IReadOnlyCollection<TransformField> fields)
        {
            _args = args;
            _lazyDb = new Lazy<IMongoDatabase>(() =>
            {
                var cn = _configuration.GetSection($"MongoDb:{args.ConnectionName ?? "Default"}").Get<MongoDbConnection>();
                var client = new MongoClient(cn.GetConnectionString());
                return client.GetDatabase(cn.DbName);
            });
        }

        protected override void ProcessBatch(BatchResult parseResult)
        {
            int sleep = 0;
            while (_tasks.Count >= _args.MaxConcurency)
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

        protected Task OnSave(List<IDictionary<string, object>> batch)
        {
            if (batch == null || batch.Count == 0)
                return Task.CompletedTask;

            var collection = _lazyDb.Value.GetCollection<BsonDocument>(_args.CollectionName);
            return collection.InsertManyAsync(batch.Select(e => new BsonDocument(e)));
        }

        protected override void WaitToComplete()
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
