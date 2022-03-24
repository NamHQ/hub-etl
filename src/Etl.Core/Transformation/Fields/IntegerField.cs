using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class IntegerField : NumberField<int?>
    {
        [XmlAttribute]
        public int Max { get; set; } = int.MaxValue;

        [XmlAttribute]
        public int Min { get; set; } = int.MinValue;

        protected override int? Convert(string text, Context context)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToInt32(text);
            }
            catch
            {
                throw NewException("Value", "Value", text);
            }
        }

        protected override void Validate(int? value, IDictionary<string, object> record, Context context)
        {
            if (Min != int.MinValue && value < Min)
                throw NewException(nameof(Min), Min, value);

            if (Max != int.MaxValue && value > Max)
                throw NewException(nameof(Max), Max, value);
        }
    }
}
