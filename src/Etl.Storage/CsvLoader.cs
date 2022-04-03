using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class CsvLoader : FileLoader<CsvLoader, CsvLoaderInst>
    {
        [XmlAttribute]
        public string Delimiter { get; set; } = "|";

        public List<string> Fields { get; set; } = new();
    }

    public class CsvLoaderInst : FileLoaderInst<CsvLoader, CsvLoaderInst>
    {
        private Dictionary<string, int> _fieldOrders;
        private string _delimiter;

        protected override void Initalize(CsvLoader defintion, LoaderArgs args)
        {
            //args must be immutable, it is singleton.
            base.Initalize(defintion, args);
            var selectedFields = new List<string>();

            if (defintion.Fields.Count == 0)
                selectedFields.AddRange(args.Fields.Select(e => e.Field ?? e.ParserField));
            else
                selectedFields.AddRange(defintion.Fields);

            _delimiter = defintion.Delimiter;
            _stream.WriteLine(string.Join('|', selectedFields));

            if (_fieldOrders == null)
                _fieldOrders = new Dictionary<string, int>();

            for (var i = 0; i < selectedFields.Count; i++)
                _fieldOrders[selectedFields[i]] = i;

            
        }

        public override void OnProcessBatch(BatchResult result)
        {
            for (var i = 0; i < result.Batch.Count; i++)
                OnSaveRecord(result.Batch[i]);

            if (result.IsLast)
                OnCompleted();
        }

        public virtual void OnSaveRecord(IDictionary<string, object> record)
        {
            var eles = new object[_fieldOrders.Count];
            foreach (var e in record)
            {
                if (_fieldOrders.TryGetValue(e.Key, out int order))
                    eles[order] = e.Value;
            }

            var text = string.Join(_delimiter, eles);
            _stream?.WriteLine(text);
        }
    }
}
