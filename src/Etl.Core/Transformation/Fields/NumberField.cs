using Etl.Core.Extraction;
using Etl.Core.Transformation.Actions;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class FormatNumberActionDef : TransformActionDef<FormatNumberAction, FormatNumberActionDef>
    {
    }

    public class FormatNumberAction : TransformAction<FormatNumberAction, FormatNumberActionDef>
    {
        public override object Execute(object input, ActionArgs args)
        {
            var text = input?.ToString();

            if (text == null)
                return null;

            if (text[^1] == '-')
                text = $"-{text[0..^1]}";
            return text;
        }
    }

    public class FormatNumberField : DataField<double?>
    {
        protected override TransformActionDef MainActionDef => new FormatNumberActionDef();
    }
}
