using Etl.Core.Extraction;

namespace Etl.Core.Transformation.Fields
{
    public interface ITransformFieldInst
    {
        string DataField { get; set; }
        string ParserField { get; set; }

        bool Required { get; set; }
        object Transform(ExtractedRecord record);
    }

    public abstract class TransformFieldInst : ITransformFieldInst
    {
        public string DataField { get; set; }
        public string ParserField { get; set; }
        public bool Required { get; set; }
        public abstract object Transform(ExtractedRecord record);
    }

    public abstract class TransformFieldInst<TOutput> : ITransformFieldInst
    {
        public string DataField { get; set; }
        public string ParserField { get; set; }
        public bool Required { get; set; }

        object ITransformFieldInst.Transform(ExtractedRecord record)
            => Transform(record);
        protected abstract TOutput Transform(ExtractedRecord record);
    }
}
