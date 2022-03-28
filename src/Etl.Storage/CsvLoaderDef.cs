using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class CsvLoaderDef : FileLoaderDef<CsvLoader, CsvLoaderDef>
    {
        [XmlAttribute]
        public string Delimiter { get; set; } = "|";

        public List<string> Fields { get; set; } = new();
    }
}
