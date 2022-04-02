using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class TransformFieldDef
    {
        [XmlAttribute]
        public string Field { get; set; }

        [XmlAttribute]
        public string ParserField { get; set; }

        [XmlAttribute]
        public bool Required { get; set; }

        public ITransformField CreateInstance(IServiceProvider sp)
        {
            var instance = OnCreateInstance(sp);
            instance.DataField = String.IsNullOrWhiteSpace(Field) ? ParserField : Field;
            instance.ParserField = String.IsNullOrWhiteSpace(ParserField) ? Field : ParserField;
            instance.Required = Required;
            return instance;
        }

        protected abstract ITransformField OnCreateInstance(IServiceProvider sp);
    }
}
