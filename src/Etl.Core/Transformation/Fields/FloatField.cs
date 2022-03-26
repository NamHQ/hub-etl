using Etl.Core.Extraction;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class FLoatField : NumberField<double?>
    {
        [XmlAttribute]
        public double Max { get; set; } = double.MaxValue;

        [XmlAttribute]
        public double Min { get; set; } = double.MinValue;

        protected override double? Convert(string text,  ExtractedResult extractedResult, Context context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDouble(text);
            }
            catch
            {
                throw Stop(extractedResult, "Value");
            }
        }

        protected override void Validate(double? value, ExtractedResult extractedResult, Context context)
        {
            if (Min != double.MinValue && value < Min)
                throw Stop(extractedResult, nameof(Min));

            if (Max != double.MaxValue && value > Max)
                throw Stop(extractedResult, nameof(Max));
        }
    }
}
