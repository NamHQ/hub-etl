using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class GroupField : FieldBase
    {
        [XmlArrayItem("Integer", typeof(IntegerField))]
        [XmlArrayItem("Float", typeof(FLoatField))]
        [XmlArrayItem("Boolean", typeof(BooleanField))]
        [XmlArrayItem("Date", typeof(DateField))]
        [XmlArrayItem("String", typeof(StringField))]
        [XmlArrayItem("Hash", typeof(HashField))]
        [XmlArrayItem("Encrypt", typeof(EncryptField))]
        [XmlArrayItem("Group", typeof(GroupField))]
        [XmlArrayItem("Array", typeof(ArrayField))]
        public virtual List<FieldBase> Fields { get; set; } = new();

        protected readonly Lazy<ArrayField> LazyFlatArray;

        public GroupField()
        {
            LazyFlatArray = new Lazy<ArrayField>(() =>
            {
                ArrayField flatField = null;
                foreach (var e in Fields)
                    if (e is ArrayField array && array.Flat)
                    {
                        if (flatField != null)
                            throw new Exception($"Not except multiple flat {nameof(ArrayField)} in the same hierarchy.");

                        flatField = array;
                    }
                return flatField;
            });
        }

        public override object GetValue(IDictionary<string, object> record, Context context)
            => TranformOneParserdRecord(record, context);

        protected virtual TransformResult TranformOneParserdRecord(IDictionary<string, object> record, Context context)
        {
            IDictionary<string, object> newRecord = null;
            var result = LazyFlatArray.Value?.GetValue(record, context) as TransformResult ?? new TransformResult();
            if (result.Items.Count == 0)
                result.Items.Add(newRecord = new Dictionary<string, object>());

            try
            {
                foreach (var field in Fields)
                {
                    if (field == LazyFlatArray.Value)
                        continue;

                    var val = field.GetValue(record, context);
                    if (val == null)
                        continue;

                    if (newRecord != null)
                        newRecord[field.LazyDbField.Value] = val;
                    else
                        result.Items.ForEach(e => e[field.LazyDbField.Value] = val);
                }
            }
            catch (Exception ex)
            {
                return new TransformResult { TotalErrors = Math.Max(result.TotalRecords, 1) }
                    .AddErorr(ex is TransformException error ? error.ToString() : ex.Message);
            }

            return result;
        }

    }
}
