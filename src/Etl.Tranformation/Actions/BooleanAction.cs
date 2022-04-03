using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Tranformation.Actions
{
    public class BooleanAction : TransformAction<BooleanActionInst>
    {
    }

    public class BooleanActionInst : TransformActionInst<bool?>
    {
        protected override bool? Execute(object input, ActionArgs args)
        {
            var text = input?.ToString();

            return string.IsNullOrEmpty(text)
                ? null
                : "on".Equals(text, StringComparison.OrdinalIgnoreCase)
                   || "true".Equals(text, StringComparison.OrdinalIgnoreCase)
                   || System.Convert.ToBoolean(input);
        }
    }
}
