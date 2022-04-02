using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Actions
{
    public class SubStringAction : TransformAction
    {
        [XmlAttribute]
        public string Start { get; set; }

        [XmlAttribute]
        public int StartOffset { get; set; }

        [XmlAttribute]
        public string End { get; set; }

        [XmlAttribute]
        public int EndOffset { get; set; }

        internal readonly Lazy<Regex> _regexStart;
        internal readonly Lazy<Regex> _regexEnd;

        public SubStringAction()
        {
            _regexStart = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(Start) ? null : new Regex(Start, RegexOptions.Compiled));
            _regexEnd = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(End) ? null : new Regex(End, RegexOptions.Compiled));
        }

        public override ITransformActionInst CreateInstance(IServiceProvider sp)
            => new SubStringActionInst(this);
    }
    public class SubStringActionInst : TransformActionInst<string>
    {
        private readonly SubStringAction _definition;

        public SubStringActionInst(SubStringAction definition)
        {
            _definition = definition;
        }

        protected override string Execute(object input, ActionArgs args)
        {
            var text = input as string;

            if (text == null)
                return null;

            var regexStart = _definition._regexStart;
            var regexEnd = _definition._regexEnd;

            var start = 0;
            if (regexStart.Value != null)
            {
                var match = regexStart.Value.Match(text);
                if (match.Success)
                    start = regexStart.Value.Match(text).Index;
                else
                    return null;
            }

            start += _definition.StartOffset;
            if (start >= text.Length)
                return null;

            var length = 0;
            if (regexEnd.Value != null)
            {
                var match = start >= text.Length ? null : regexEnd.Value.Match(text, start);
                if (match != null && match.Success)
                    length = match.Index - start;
                else
                    throw new Exception($"{nameof(SubStringAction)} can't find {nameof(_definition.End)}: {_definition.End}.");
            }

            length += _definition.EndOffset;

            return regexEnd.Value == null && length == 0 || start + length >= text.Length ? text[start..] : text.Substring(start, length);
        }


    }
}
