using Etl.Core.Extraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Etl.Core.Transformation.Fields
{
    public class RecordField : TransformField
    {
        public List<TransformField> Fields { get; set; } = new();

        public HashSet<string> IgnoreParserFields { get; set; } = new();

        protected internal override Type InstanceType => typeof(RecordFieldInst);
    }

    public class RecordFieldInst : ITransformFieldInst
    {
        private readonly List<ITransformFieldInst> _fields = new();
        private ArrayFieldInst _flatArray;

        public string Alias { get; private set; }
        public string DataField { get; private set; }
        public bool Required { get; private set; }

        void IInitialization.Initialize(object args, IServiceProvider sp)
            => Initialize((RecordField)args, sp);

        public void Initialize(RecordField definition, IServiceProvider sp)
        {
            Alias = definition.Alias;
            DataField = definition.DataField;
            Required = definition.Required;

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

        object ITransformFieldInst.Transform(ExtractedRecord record)
            => Transform(record);
        public virtual  TransformResult Transform(ExtractedRecord record)
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
                        newRecord[field.Alias] = val;
                    else
                        result.Items.ForEach(e => e[field.Alias] = val);
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
