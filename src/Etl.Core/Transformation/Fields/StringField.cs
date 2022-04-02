﻿using Etl.Core.Transformation.Actions;

namespace Etl.Core.Transformation.Fields
{
    public class StringActionInst : TransformActionInst<string>
    {
        protected override string Execute(object input, ActionArgs args)
            => input?.ToString();
    }

    public class StringField : PipeLineField
    {
        protected override TransformAction MainAction => new TransformAction<StringActionInst>();
    }

    //public class StringField : DataField<string>
    //{
    //    //[^0-9A-Za-z\s,;.'":<>?~½+-_=!@#$%^&*(){}[\]\\\/]+
    //    private static readonly Regex _blackListChars = new("[^0-9A-Za-z\\s,;.'\":<>?~½+-_=!@#$%^&*(){}[\\]\\\\\\/]+", RegexOptions.Compiled);
    //    private readonly Lazy<Regex> _regexPattern;

    //    [XmlAttribute]
    //    public int MaxLength { get; set; } = int.MaxValue;

    //    [XmlAttribute]
    //    public int MinLength { get; set; } = 0;

    //    [XmlAttribute]
    //    public string Pattern { get; set; }

    //    public StringField()
    //    {
    //        _regexPattern = new Lazy<Regex>(() => string.IsNullOrWhiteSpace(Pattern) ? null : new Regex(Pattern));
    //    }

    //    protected override string Convert(string text, ExtractedResult extractedResult)
    //    {
    //        if (string.IsNullOrWhiteSpace(text))
    //            return null;

    //        if (_regexPattern.Value != null)
    //        {
    //            if (!_regexPattern.Value.IsMatch(text))
    //                Stop(extractedResult, nameof(Pattern));
    //            return text;
    //        }

    //        return _blackListChars.Replace(text, " ");
    //    }

    //    protected override void Validate(string value, ExtractedResult extractedResult)
    //    {
    //        if (MinLength > 0 && (value == null || value.Length < MinLength))
    //            Stop(extractedResult, nameof(MinLength));

    //        if (MaxLength != int.MaxValue && (value == null || value.Length > MaxLength))
    //            Stop(extractedResult, nameof(MaxLength));
    //    }
    //}
}
