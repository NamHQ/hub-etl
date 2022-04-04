using Etl.Core.Extraction;
using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public abstract class TransformField
    {
        private string _alias;
        [XmlAttribute]
        public string Alias
        {
            get { return _alias ?? _dataField; }
            set { _alias = string.IsNullOrWhiteSpace(value) ? null : value; }
        }


        private string _dataField;
        [XmlAttribute]
        public string DataField
        {
            get { return _dataField ?? _alias; }
            set { _dataField = string.IsNullOrWhiteSpace(value) ? null : value; }
        }

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
        string Alias { get; }
        string DataField { get; }

        bool Required { get; }

        object Transform(ExtractedRecord record);
    }

    public abstract class TransformFieldInst<TDef, TOutput> : ITransformFieldInst
        where TDef : TransformField
    {
        public string Alias { get; set; }
        public string DataField { get; set; }
        public bool Required { get; set; }

        public virtual void Initialize(TDef definition, IServiceProvider sp) { }
        void IInitialization.Initialize(object args, IServiceProvider sp)
        {
            var definition = (TDef)args;

            Alias = definition.Alias;
            DataField = definition.DataField;
            Required = definition.Required;
            Initialize(definition, sp);
        }

        public abstract TOutput Transform(ExtractedRecord record);
        object ITransformFieldInst.Transform(ExtractedRecord record)
            => Transform(record);
    }
}