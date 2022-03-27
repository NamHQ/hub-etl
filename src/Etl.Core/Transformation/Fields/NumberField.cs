using Etl.Core.Extraction;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public abstract class NumberField<T> : FieldBase<T>
    {
        protected override string Modify(ExtractedResult extractedResult, IDictionary<string, object> record, IEtlContext context)
        {
            var raw = base.Modify(extractedResult, record, context);

            return base.ModifyAction == null && !string.IsNullOrEmpty(raw) && raw[^1] == '-'
                ? $"-{raw.Substring(0, raw.Length -1)}"
                : raw;
        }
    }
}
