using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class GroupField : TransformField<TransformResult>
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
        public virtual List<TransformField> Fields { get; set; } = new();

        public HashSet<string> IgnoreParserFields { get; set; } = new();

        protected ArrayField FlatArray;

        public override void Initialize(IServiceProvider sp)
        {
            foreach (var e in Fields)
            {
                e.Initialize(sp);
                if (e is ArrayField array && array.Flat)
                {
                    if (FlatArray != null)
                        throw new Exception($"Not except multiple flat {nameof(ArrayField)} in the same hierarchy.");

                    FlatArray = array;
                }
            }
        }

        protected override TransformResult Start(IDictionary<string, object> record, IEtlContext context)
        {
            IDictionary<string, object> newRecord = null;
            var result = FlatArray?.Transform(record, context) as TransformResult ?? new TransformResult();
            if (result.Items.Count == 0)
                result.Items.Add(newRecord = new Dictionary<string, object>());

            try
            {
                foreach (var field in Fields)
                {
                    if (field == FlatArray)
                        continue;

                    var val = field.Transform(record, context);
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
                    .AddErorr(ex.Message);
            }

            return result;
        }

    }
}
