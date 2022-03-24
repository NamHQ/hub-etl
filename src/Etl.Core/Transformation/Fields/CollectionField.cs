using System;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class CollectionField : GroupField
    {
        public HashSet<string> IgnoreParserFields = new();

        protected readonly Lazy<ArrayField> LazyFlatArray;

        public CollectionField()
        {
            LazyFlatArray = new Lazy<ArrayField>(() =>
            {
                ArrayField flatField = null;
                foreach (var e in Fields)
                    if (e is ArrayField array && array.Flat )
                    {
                        if (flatField != null)
                            throw new Exception($"Not except multiple flat {nameof(ArrayField)} in the same hierarchy.");

                        flatField = array;
                    }
                return flatField;
            });
        }

        public override object GetValue(IDictionary<string, object> record, Context context)
        {
            var result = new Dictionary<string, object>();
            var flatResult = LazyFlatArray.Value?.GetValue(record, context) as List<IDictionary<string, object>>;
            bool hasFlat = flatResult != null && flatResult.Count > 0;

            foreach (var field in Fields)
            {
                if (field == LazyFlatArray.Value)
                    continue;

                var val = field.GetValue(record, context);
                if (val == null)
                    continue;

                if (hasFlat)
                    flatResult.ForEach(e => e[field.LazyDbField.Value] = val);
                else
                    result[field.LazyDbField.Value] = val;
            }

            return hasFlat ? flatResult : result;
        }
    }
}
