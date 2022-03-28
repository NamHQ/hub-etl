using Etl.Core.Extraction;
using System;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class DateField : FieldBase<DateTime?>
    {
        [XmlAttribute]
        public DateTime Max { get; set; } = DateTime.MaxValue;

        [XmlAttribute]
        public DateTime Min { get; set; } = DateTime.MinValue;

        protected override DateTime? Convert(string text, ExtractedResult extractedResult, IEtlContext context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDateTime(text); 
            }
            catch
            {
                throw Stop(extractedResult, $"Value");
            }
        }

        protected override void Validate(DateTime? value, ExtractedResult extractedResult, IEtlContext context)
        {
            if (Min != DateTime.MinValue && value < Min)
                throw Stop(extractedResult, nameof(Min));

            if (Max != DateTime.MaxValue && value > Max)
                throw Stop(extractedResult, nameof(Max));
        }
    }
}
