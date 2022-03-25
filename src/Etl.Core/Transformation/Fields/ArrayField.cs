using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class ArrayField : CollectionField
    {
        [XmlAttribute]
        public bool Flat { get; set; }

        public override object GetValue(IDictionary<string, object> record, Context context)
        {
            if (!record.ContainsKey(LazyParserField.Value))
                return null;

            if (record[LazyParserField.Value] is not List<IDictionary<string, object>>)
                throw new Exception($"Expected {typeof(List<IDictionary<string, object>>).Name} instead of {record[LazyParserField.Value]?.GetType().Name}");
            
            var nestedRecords = record[LazyParserField.Value] as List<IDictionary<string, object>>;

            var result = new List<IDictionary<string, object>>();
            foreach (var nextRecord in nestedRecords)
            {
                var newRecord = new Dictionary<string, object>();

                var flatResult = LazyFlatArray.Value?.GetValue(nextRecord, context) as List<IDictionary<string, object>>;

                bool hasFlat = flatResult != null && flatResult.Count > 0;

                if (hasFlat)
                    result.AddRange(flatResult);
                else
                    result.Add(newRecord);

                foreach (var field in Fields)
                {
                    if (field == LazyFlatArray.Value)
                        continue;

                    var val = field.GetValue(nextRecord, context);
                    if (val == null)
                        continue;

                    if (hasFlat)
                        flatResult.ForEach(e => e[field.LazyDbField.Value] = val);
                    else
                        newRecord[field.LazyDbField.Value] = val;
                }
            }

            return result;
        }
    }
}
