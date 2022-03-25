using Etl.Core.Transformation.Modification;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class FieldBase
    {
        [XmlAttribute]
        public string Field { get; set; }
        internal readonly Lazy<string> LazyDbField;

        [XmlAttribute]
        public string ParserField { get; set; }
        internal readonly Lazy<string> LazyParserField;

        [XmlAttribute]
        public bool Require { get; set; }

        protected FieldBase()
        {
            LazyDbField = new Lazy<string>(() => string.IsNullOrWhiteSpace(Field) ? ParserField : Field);
            LazyParserField = new Lazy<string>(() => string.IsNullOrWhiteSpace(ParserField) ? Field: ParserField);
        }

        public abstract object GetValue(IDictionary<string, object> record, Context context);

    }

    public abstract class FieldBase<T> : FieldBase
    {
        [XmlElement("Modify")]
        public ModificationActionBase ModifyAction { get; set; }

        public override object GetValue(IDictionary<string, object> record, Context context)
            => Start(record, context);
        protected virtual T Start(IDictionary<string, object> record, Context context)
        {
            var text = Modify(record, context);

            var value = Convert(text, context);

            if (Require && value == null)
                throw new Exception("required");

            Validate(value, record, context);

            return value;
        }

        protected virtual string Modify(IDictionary<string, object> record, Context context)
        {
            if (ModifyAction == null)
            {
                record.TryGetValue(LazyParserField.Value, out object raw);
                return raw as string;
            }

            return ModifyAction.Execute(this, record);
        }
        protected abstract T Convert(string text, Context context);
        protected virtual void Validate(T value, IDictionary<string, object> record, Context context) { }
    }
}
