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

        public override string Execute(FieldBase field, IDictionary<string, object> record)
        {
            if (!record.TryGetValue(field.LazyParserField.Value, out object raw))
                return null;

            var text = raw as string;

            var start = 0;
            if (_regexStart.Value != null)
            {
                var match = _regexStart.Value.Match(text);
                if (match.Success)
                    start = _regexStart.Value.Match(text).Index;
                else
                    return null;
            }

            start += StartOffset;
            if (start >= text.Length)
                return null;

            var length = 0;
            if (_regexEnd.Value != null)
            {
                var match = start >= text.Length ? null : _regexEnd.Value.Match(text, start);
                if (match != null && match.Success)
                    length = match.Index - start;
                else
                    throw new Exception($"{nameof(SubStringAction)} can't find {nameof(End)}: {End}.");
            }

            length += EndOffset;

            return (_regexEnd.Value == null && length == 0) || (start + length) >= text.Length ? text[start..] : text.Substring(start, length);
        }
    }
}
