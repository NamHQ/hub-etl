using Etl.Core.Extraction;
using System;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class BooleanField : FieldBase<bool?>
    {
        public BooleanField()
        {
        }

        protected override string Modify(ExtractedResult extractedResult, IDictionary<string, object> record, Context context)
        {
            var text = base.Modify(extractedResult, record, context);

            if (ModifyAction == null && !string.IsNullOrWhiteSpace(text))
                return text == "1"
                    || "on".Equals(text, StringComparison.OrdinalIgnoreCase)
                    || "true".Equals(text, StringComparison.OrdinalIgnoreCase) ? "True" : "False";

            return text;
        }

        protected override bool? Convert(string text, ExtractedResult extractedResult, Context context)
            => string.IsNullOrEmpty(text) ? null : System.Convert.ToBoolean(text);
    }
}
