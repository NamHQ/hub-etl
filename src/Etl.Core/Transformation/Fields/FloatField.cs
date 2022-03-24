using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class FLoatField : NumberField<double?>
    {
        [XmlAttribute]
        public double Max { get; set; } = double.MaxValue;

        [XmlAttribute]
        public double Min { get; set; } = double.MinValue;

        protected override double? Convert(string text, Context context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDouble(text);
            }
            catch
            {
                throw NewException("Value", "Value", text);
            }
        }

        protected override void Validate(double? value, IDictionary<string, object> record, Context context)
        {
            if (Min != int.MinValue && value < Min)
                throw NewException(nameof(Min), Min, value);

            if (Max != int.MaxValue && value > Max)
                throw NewException(nameof(Max), Max, value);
        }
    }
}
