using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class TransformField
    {
        [XmlAttribute]
        public string Field { get; set; }

        [XmlAttribute]
        public string ParserField { get; set; }

        [XmlAttribute]
        public bool Required { get; set; }

        public ITransformFieldInst CreateInstance(IServiceProvider sp)
        {
            var instance = OnCreateInstance(sp);
            instance.DataField = String.IsNullOrWhiteSpace(Field) ? ParserField : Field;
            instance.ParserField = String.IsNullOrWhiteSpace(ParserField) ? Field : ParserField;
            instance.Required = Required;
            return instance;
        }

        protected abstract ITransformFieldInst OnCreateInstance(IServiceProvider sp);
    }
}
