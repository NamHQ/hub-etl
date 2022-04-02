using Etl.Core.Extraction;

namespace Etl.Core.Transformation.Fields
{
    public interface ITransformField
    {
        string DataField { get; set; }
        string ParserField { get; set; }

        bool Required { get; set; }
        object Transform(ExtractedRecord record);
    }

    public abstract class TransformField : ITransformField
    {
        public string DataField { get; set; }
        public string ParserField { get; set; }
        public bool Required { get; set; }
        public abstract object Transform(ExtractedRecord record);
    }

    public abstract class TransformField<T> : ITransformField
    {
        public string DataField { get; set; }
        public string ParserField { get; set; }
        public bool Required { get; set; }

        object ITransformField.Transform(ExtractedRecord record)
            => Transform(record);
        protected abstract T Transform(ExtractedRecord record);
    }
}
