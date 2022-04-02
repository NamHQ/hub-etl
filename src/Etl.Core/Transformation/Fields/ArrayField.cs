using Etl.Core.Extraction;
using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class ArrayField : GroupField
    {
        [XmlAttribute]
        public bool Flat { get; set; }
    }

    public class ArrayFieldInst : GroupFieldInst
    {
        protected override TransformResult Transform(ExtractedRecord record)
        {
            if (!record.ContainsKey(ParserField))
                return null;

            var nestedRecords = record[ParserField] as ExtractedArray;
            if (nestedRecords == null)
                throw new Exception($"Expected {typeof(ExtractedArray).Name} instead of {record[ParserField]?.GetType().Name}");

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
