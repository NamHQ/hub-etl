using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class CsvLoader : FileLoader<CsvLoaderInst, CsvLoader>
    {
        [XmlAttribute]
        public string Delimiter { get; set; } = "|";

        public List<string> Fields { get; set; } = new();
    }

    public class CsvLoaderInst : FileLoaderInst<CsvLoaderInst, CsvLoader>
    {
        private Dictionary<string, int> _fieldOrders;
        private string _delimiter;

        protected override void Initalize(CsvLoader args, string inputFile, IReadOnlyCollection<TransformField> fields)
        {
            //args must be immutable, it is singleton.
            base.Initalize(args, inputFile, fields);
            var selectedFields = new List<string>();

            if (args.Fields.Count == 0)
                selectedFields.AddRange(fields.Select(e => e.Field ?? e.ParserField));
            else
                selectedFields.AddRange(args.Fields);

            _delimiter = args.Delimiter;
            _stream.WriteLine(string.Join('|', selectedFields));

            if (_fieldOrders == null)
                _fieldOrders = new Dictionary<string, int>();

            for (var i = 0; i < selectedFields.Count; i++)
                _fieldOrders[selectedFields[i]] = i;

            
        }

        protected override void OnProcessBatch(BatchResult result)
        {
            for (var i = 0; i < result.Batch.Count; i++)
                OnSaveRecord(result.Batch[i]);

            if (result.IsLast)
                OnCompleted();
        }

        protected virtual void OnSaveRecord(IDictionary<string, object> record)
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
