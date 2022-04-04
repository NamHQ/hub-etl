using Etl.Core.Extraction;
using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class ArrayField : RecordField<ArrayField, ArrayFieldInst>
    {
        [XmlAttribute]
        public bool Flat { get; set; }
    }

    public class ArrayFieldInst : RecordFieldInst<ArrayField, ArrayFieldInst>
    {
        public override TransformResult Transform(ExtractedRecord record)
        {
            if (!record.ContainsKey(DataField))
                return null;

            var nestedRecords = record[DataField] as ExtractedArray;
            if (nestedRecords == null)
                throw new Exception($"Expected {typeof(ExtractedArray).Name} instead of {record[DataField]?.GetType().Name}");

            var result = new TransformResult();
            foreach (var nextRecord in nestedRecords)
            {
                var oneResult = base.Transform(nextRecord);
                result.Items.AddRange(oneResult.Items);
                result.TotalErrors += oneResult.TotalErrors;
                result.AddErorrs(oneResult.Errors);
            }

            return result;
        }
    }
}
