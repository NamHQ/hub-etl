using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class CsvLoader : FileLoader
    {
        [XmlAttribute]
        public string Delemiter { get; set; } = "|";

        public List<string> Fields { get; set; } = new();

        private Dictionary<string, int> _fieldOrders;

        public override void Initialize(IConfiguration appSetting, string inputFile, IReadOnlyCollection<FieldBase> fields)
        {
            base.Initialize(appSetting, inputFile, fields);

            if (Fields.Count == 0)
                Fields.AddRange(fields.Select(e => e.Field ?? e.ParserField));

            _stream.WriteLine(string.Join('|', Fields));

            if (_fieldOrders == null)
                _fieldOrders = new Dictionary<string, int>();

            for (var i = 0; i < Fields.Count; i++)
                _fieldOrders[Fields[i]] = i;
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

            var text = string.Join(Delemiter, eles);
            _stream?.WriteLine(text);
        }
    }
}
