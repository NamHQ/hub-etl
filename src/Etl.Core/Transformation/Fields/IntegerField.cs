using Etl.Core.Transformation.Actions;
using System;

namespace Etl.Core.Transformation.Fields
{

    public class IntegerAction : TransformAction<int?>
    {
        protected override int? Execute(object input, ActionArgs args)
        {
            var text = input as string;

            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            if (text[^1] == '-')
                text =$"-{text[0..^1]}";

            return Convert.ToInt32(text);
        }
    }

    public class IntegerFieldDef : PipeLineFieldDef
    {
        protected override TransformActionDef MainActionDef => new TransformActionDef<IntegerAction>();
    }
}
