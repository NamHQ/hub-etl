using System;
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
                throw new TransformException($"Invalid Value", text);
            }
        }

        protected override void Validate(double? value, IDictionary<string, object> record, Context context)
        {
            if (Min != double.MinValue && value < Min)
                throw new TransformException($"Invalid {nameof(Min)}", value);

            if (Max != double.MaxValue && value > Max)
                throw new TransformException($"Invalid {nameof(Max)}", value);
        }
    }
}
