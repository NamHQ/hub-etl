using Etl.Core.Extraction;
using Etl.Core.Transformation.Modification;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class DataField<T> : TransformField<T>
    {
        [XmlElement("Modify")]
        public ModificationActionBase ModifyAction { get; set; }

        public ExtractedResult GetExtractedResult(IDictionary<string, object> record)
            => string.IsNullOrWhiteSpace(LazyParserField.Value) || !record.TryGetValue(LazyParserField.Value, out object value)
                ? null
                : value as ExtractedResult;

        protected override T Start(IDictionary<string, object> record, IEtlContext context)
        {
            var extractedResult = GetExtractedResult(record);

            var text = Modify(extractedResult, record, context);

            var value = Convert(text, extractedResult, context);

            if (Required && value == null)
                throw Stop(extractedResult, "Required");

            Validate(value, extractedResult, context);

            return value;
        }

        protected virtual string Modify(ExtractedResult extractedResult, IDictionary<string, object> record, IEtlContext context)
            => ModifyAction == null
                ? extractedResult?.Value
                : ModifyAction.Execute(this, extractedResult?.Value, record);

        protected abstract T Convert(string text, ExtractedResult extractedResult, IEtlContext context);

        protected virtual void Validate(T value, ExtractedResult extractedResult, IEtlContext context) { }

        protected Exception Stop(ExtractedResult extractedResult, string reason)
            => new(
                $" Invalid: '{reason}', " +
                $" {GetType().Name}:'{LazyDbField.Value}'," +
                $" Data: '{extractedResult?.Value ?? "NULL"}'," +
                $" {extractedResult.Position}," +
                $" Text: {string.Join('\n', extractedResult.Lines)}");
    }
}
