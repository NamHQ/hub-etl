using Etl.Core.Transformation.Actions;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Tranformation.Actions
{
    public class CheckPatternAction : TransformAction<CheckPatternActionInst>
    {
        internal readonly Lazy<Regex> _regexPattern;

        [XmlAttribute]
        public string Pattern { get; set; }


        public CheckPatternAction()
        {
            _regexPattern = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(Pattern) ? null : new Regex(Pattern, RegexOptions.Compiled));
        }

    }

    public class CheckPatternActionInst : ValidateActionInst<CheckPatternAction>
    {
        private Lazy<Regex> _regexPattern;
        protected override void Initialize(CheckPatternAction definition, IServiceProvider sp)
        {
            _regexPattern = definition._regexPattern;
        }

        protected override void Execute(object input, ActionArgs args)
        {
            if (_regexPattern.Value == null)
                return;

            var text = input?.ToString()??"";

            if (!_regexPattern.Value.IsMatch(text))
                throw new Exception($"Not match pattern '{_regexPattern.Value}'");
        }
    }
}
