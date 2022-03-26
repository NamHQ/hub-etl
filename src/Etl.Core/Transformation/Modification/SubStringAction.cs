using Etl.Core.Transformation.Fields;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Modification
{
    public class SubStringAction : ModificationActionBase
    {
        [XmlAttribute]
        public string Start { get; set; }

        [XmlAttribute]
        public int StartOffset { get; set; }

        [XmlAttribute]
        public string End { get; set; }

        [XmlAttribute]
        public int EndOffset { get; set; }

        private readonly Lazy<Regex> _regexStart;
        private readonly Lazy<Regex> _regexEnd;

        public SubStringAction()
        {
            _regexStart = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(Start) ? null : new Regex(Start, RegexOptions.Compiled));
            _regexEnd = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(End) ? null : new Regex(End, RegexOptions.Compiled));
        }

        public override string Execute(FieldBase field, string rawValue, IDictionary<string, object> record)
        {
            if (rawValue == null)
                return null;

            var start = 0;
            if (_regexStart.Value != null)
            {
                var match = _regexStart.Value.Match(rawValue);
                if (match.Success)
                    start = _regexStart.Value.Match(rawValue).Index;
                else
                    return null;
            }

            start += StartOffset;
            if (start >= rawValue.Length)
                return null;

            var length = 0;
            if (_regexEnd.Value != null)
            {
                var match = start >= rawValue.Length ? null : _regexEnd.Value.Match(rawValue, start);
                if (match != null && match.Success)
                    length = match.Index - start;
                else
                    throw new Exception($"{nameof(SubStringAction)} can't find {nameof(End)}: {End}.");
            }

            length += EndOffset;

            return (_regexEnd.Value == null && length == 0) || (start + length) >= rawValue.Length ? rawValue[start..] : rawValue.Substring(start, length);
        }
    }
}
