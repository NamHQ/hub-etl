using Etl.Core.Extraction;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class IntegerField : NumberField<int?>
    {
        [XmlAttribute]
        public int Max { get; set; } = int.MaxValue;

        [XmlAttribute]
        public int Min { get; set; } = int.MinValue;

        protected override int? Convert(string text, ExtractedResult extractedResult, IEtlContext context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToInt32(text);
            }
            catch
            {
                throw Stop(extractedResult, "Value");
            }
        }

        protected override void Validate(int? value, ExtractedResult extractedResult, IEtlContext context)
        {
            if (Min != int.MinValue && value < Min)
                throw Stop(extractedResult, nameof(Min));

            if (Max != int.MaxValue && value > Max)
                throw Stop(extractedResult, nameof(Max));
        }
    }
}
