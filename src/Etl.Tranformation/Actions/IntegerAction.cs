using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Tranformation.Actions
{
    public class IntegerAction : TransformAction<IntegerActionInst> { }

    public class IntegerActionInst : TransformActionInst<int?>
    {
        protected override int? Execute(object input, ActionArgs args)
        {
            var text = input as string;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (text[^1] == '-')
                text = $"-{text[0..^1]}";

            return Convert.ToInt32(text);
        }
    }
}
