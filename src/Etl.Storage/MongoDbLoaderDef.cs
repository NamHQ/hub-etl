using Etl.Core.Load;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class MongoDbLoaderDef : LoaderDef<MongoDbLoader, MongoDbLoaderDef>
    {
        [XmlAttribute]
        public string ConnectionName { get; set; }

        [XmlAttribute]
        public int MaxConcurency { get; set; } = 10;

        [XmlAttribute]
        public string CollectionName { get; set; }
    }
}
