using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Core.Transformation.Fields
{
    public class BooleanActionInst : TransformActionInst<bool?>
    {
        protected override bool? Execute(object input, ActionArgs args)
        {
            var text = input?.ToString();

            return string.IsNullOrEmpty(input?.ToString())
                ?  null
                : "on".Equals(text, StringComparison.OrdinalIgnoreCase)
                   || "true".Equals(text, StringComparison.OrdinalIgnoreCase)
                   || System.Convert.ToBoolean(input);
        }
    }

    public class BooleanField : PipeLineField
    {
        protected override TransformAction MainAction => new TransformAction<BooleanActionInst>();
    }
}
