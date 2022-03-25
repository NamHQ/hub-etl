using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{

    public class DateField : FieldBase<DateTime?>
    {
        [XmlAttribute]
        public DateTime Max { get; set; } = DateTime.MaxValue;

        [XmlAttribute]
        public DateTime Min { get; set; } = DateTime.MinValue;

        protected override DateTime? Convert(string text, Context context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDateTime(text); 
            }
            catch
            {
                throw new TransformException($"Invalid Value", text);
            }
        }

        protected override void Validate(DateTime? value, IDictionary<string, object> record, Context context)
        {
            if (Min != DateTime.MinValue && value < Min)
                throw new TransformException($"Invalid {nameof(Min)}", value);

            if (Max != DateTime.MaxValue && value > Max)
                throw new TransformException($"Invalid {nameof(Max)}", value);
        }
    }
}
