using Etl.Core.Transformation.Actions;
using System.Text.RegularExpressions;

namespace Etl.Core.Transformation.Fields
{
    public class StringAction : TransformAction<StringActionInst> { }
    public class StringActionInst : TransformActionInst<string>
    {
        //[^0-9A-Za-z\s,;.'":<>?~½+-_=!@#$%^&*(){}[\]\\\/]+
        private static readonly Regex _blackListChars = new("[^0-9A-Za-z\\s,;.'\":<>?~½+-_=!@#$%^&*(){}[\\]\\\\\\/]+", RegexOptions.Compiled);
        
        protected override string Execute(object input, ActionArgs args)
        {
            var text = input?.ToString();
            return string.IsNullOrWhiteSpace(text) ? null : _blackListChars.Replace(text, " ");
        }
    }

    public class StringField : PipeLineField
    {
        public override TransformAction DefaultAction => new StringAction();
    }
}
