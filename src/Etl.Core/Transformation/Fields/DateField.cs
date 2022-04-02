using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Core.Transformation.Fields
{
    public class DateActionInst : TransformActionInst<DateTime?>
    {
        protected override DateTime? Execute(object input, ActionArgs args)
        {
            var text = input as string;
            return string.IsNullOrWhiteSpace(text) ? null : System.Convert.ToDateTime(text);
        }
    }
    public class DateField : PipeLineField
    {
        protected override TransformAction MainAction => new TransformAction<DateActionInst>();
    }
}
