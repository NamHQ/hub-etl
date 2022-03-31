using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class ArrayField : GroupField
    {
        [XmlAttribute]
        public bool Flat { get; set; }

        public override object Transform(IDictionary<string, object> record, IEtlContext context)
        {
            if (!record.ContainsKey(LazyParserField.Value))
                return null;

            var nestedRecords = record[LazyParserField.Value] as List<IDictionary<string, object>>;
            if (nestedRecords == null)
                throw new Exception($"Expected {typeof(List<IDictionary<string, object>>).Name} instead of {record[LazyParserField.Value]?.GetType().Name}");

            var result = new TransformResult();
            foreach (var nextRecord in nestedRecords)
            {
                var oneResult = base.Start(nextRecord, context);
                result.Items.AddRange(oneResult.Items);
                result.TotalErrors += oneResult.TotalErrors;
                result.AddErorrs(oneResult.Errors);
            }

            return result;
        }
    }
}
