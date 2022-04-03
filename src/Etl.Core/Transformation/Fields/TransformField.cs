using Etl.Core.Extraction;
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

        protected internal abstract Type InstanceType { get; }
    }

    public abstract class TransformField<TInst> : TransformField
        where TInst : ITransformFieldInst
    {
        override sealed internal protected Type InstanceType => typeof(TInst);
    }

    public interface ITransformFieldInst : IInitialization
    {
        string DataField { get; }
        string ParserField { get; }

        bool Required { get; }

        object Transform(ExtractedRecord record);
    }

    public abstract class TransformFieldInst<TDef, TOutput> : ITransformFieldInst
        where TDef : TransformField
    {
        public string DataField { get; set; }
        public string ParserField { get; set; }
        public bool Required { get; set; }

        public virtual void Initialize(TDef definition, IServiceProvider sp) { }
        void IInitialization.Initialize(object args, IServiceProvider sp)
        {
            var definition = (TDef)args;

            DataField = String.IsNullOrWhiteSpace(definition.Field) ? definition.ParserField : definition.Field;
            ParserField = String.IsNullOrWhiteSpace(definition.ParserField) ? definition.Field : definition.ParserField;
            Required = definition.Required;
            Initialize(definition, sp);
        }

        public abstract TOutput Transform(ExtractedRecord record);
        object ITransformFieldInst.Transform(ExtractedRecord record)
            => Transform(record);

        
       
    }
}