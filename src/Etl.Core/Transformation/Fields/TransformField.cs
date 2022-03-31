using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class TransformField
    {
        [XmlAttribute]
        public string Field { get; set; }
        internal readonly Lazy<string> LazyDbField;

        [XmlAttribute]
        public string ParserField { get; set; }
        internal readonly Lazy<string> LazyParserField;

        [XmlAttribute]
        public bool Required { get; set; }

        protected TransformField()
        {
            LazyDbField = new Lazy<string>(() => string.IsNullOrWhiteSpace(Field) ? ParserField : Field);
            LazyParserField = new Lazy<string>(() => string.IsNullOrWhiteSpace(ParserField) ? Field : ParserField);
        }

        public abstract object Transform(IDictionary<string, object> record, IEtlContext context);
    }

    public abstract class TransformField<T> : TransformField
    {
        public override object Transform(IDictionary<string, object> record, IEtlContext context)
           => Start(record, context);

        protected abstract T Start(IDictionary<string, object> record, IEtlContext context);
    }
}
