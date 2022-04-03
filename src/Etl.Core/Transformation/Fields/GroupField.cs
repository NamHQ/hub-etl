using Etl.Core.Extraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class GroupField : TransformField<GroupFieldInst>
    {
        public virtual List<TransformField> Fields { get; set; } = new();

        public HashSet<string> IgnoreParserFields { get; set; } = new();
    }

    public class GroupFieldInst : TransformFieldInst<GroupField, TransformResult>
    {
        private readonly List<ITransformFieldInst> _fields = new();
        private ArrayFieldInst _flatArray;

        public override void Initialize(GroupField definition, IServiceProvider sp)
        {
            foreach (var e in definition.Fields)
            {
                var item = (ITransformFieldInst) sp.GetRequiredService(e.InstanceType);

                if (e is ArrayField array && array.Flat)
                {
                    if (_flatArray != null)
                        throw new Exception($"Not except multiple flat {nameof(ArrayField)} in the same hierarchy.");

                    _flatArray = (ArrayFieldInst)item;
                }
                else
                    _fields.Add(item);

                item.Initialize(e, sp);
            }
        }

        public override TransformResult Transform(ExtractedRecord record)
        {
            IDictionary<string, object> newRecord = null;
            var result = _flatArray?.Transform(record) ?? new TransformResult();
            if (result.Items.Count == 0)
                result.Items.Add(newRecord = new Dictionary<string, object>());

            try
            {
                foreach (var field in _fields)
                {
                    var val = field.Transform(record);
                    if (val == null)
                        continue;

                    if (newRecord != null)
                        newRecord[field.DataField] = val;
                    else
                        result.Items.ForEach(e => e[field.DataField] = val);
                }
            }
            catch (Exception ex)
            {
                return new TransformResult { TotalErrors = Math.Max(result.TotalRecords, 1) }
                    .AddErorr(ex.Message);
            }

            return result;
        }
    }

}
