using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Tranformation.Actions
{
    public class DateAction : TransformAction<DateActionInst>
    {
    }

    public class DateActionInst : TransformActionInst<DateTime?>
    {
        protected override DateTime? Execute(object input, ActionArgs args)
        {
            var text = input as string;
            return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDateTime(input);
        }
    }
}
