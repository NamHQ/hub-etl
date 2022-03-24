using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class StringField : FieldBase<string>
    {
        //[^0-9A-Za-z\s,;.'":<>?~½+-_=!@#$%^&*(){}[\]\\\/]+
        private static readonly Regex _blackListChars = new("[^0-9A-Za-z\\s,;.'\":<>?~½+-_=!@#$%^&*(){}[\\]\\\\\\/]+", RegexOptions.Compiled);
        private readonly Lazy<Regex> _validatePattern;

        [XmlAttribute]
        public int MaxLength { get; set; } = int.MaxValue;

        [XmlAttribute]
        public int MinLength { get; set; } = 0;

        [XmlAttribute]
        public string Validation { get; set; }

        public StringField()
        {
            _validatePattern = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(Validation) ? null : new Regex(Validation));
        }

        protected override string Convert(string text, Context context)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (_validatePattern.Value != null)
            {
                if (!_validatePattern.Value.IsMatch(text))
                    throw NewException(nameof(Validation), Validation, text);
                return text;
            }

            return _blackListChars.Replace(text, " ");
        }

        protected override void Validate(string value, IDictionary<string, object> record, Context context)
        {
            if (MinLength > 0 && (value == null || value.Length < MinLength))
                throw NewException(nameof(MinLength), MinLength, value);

            if (MaxLength != int.MaxValue && (value == null || value.Length > MaxLength))
                throw NewException(nameof(MaxLength), MaxLength, value);
        }
    }
}
