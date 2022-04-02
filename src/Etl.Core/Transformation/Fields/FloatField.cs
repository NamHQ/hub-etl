using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Core.Transformation.Fields
{
    public class FloatActionInst : TransformActionInst<double?>
    {
        protected override double? Execute(object input, ActionArgs args)
        {
            var text = input?.ToString();

            if (text == null)
                return null;

            if (text[^1] == '-')
                text = $"-{text[0..^1]}";

            return Convert.ToDouble(text);
        }
    }

    public class FloatField : PipeLineField
    {
        protected override TransformAction MainAction => new TransformAction<FloatActionInst>();
    }
}
