using Etl.Core.Extraction;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation.Fields
{
    public class GroupFieldDef : TransformFieldDef
    {
        [XmlArrayItem("Integer", typeof(IntegerFieldDef))]
        //[XmlArrayItem("Float", typeof(FLoatField))]
        //[XmlArrayItem("Boolean", typeof(BooleanField))]
        //[XmlArrayItem("Date", typeof(DateField))]
        //[XmlArrayItem("String", typeof(StringField))]
        //[XmlArrayItem("Hash", typeof(HashField))]
        //[XmlArrayItem("Encrypt", typeof(EncryptField))]
        //[XmlArrayItem("Group", typeof(GroupField))]
        //[XmlArrayItem("Array", typeof(ArrayField))]
        public virtual List<TransformFieldDef> Fields { get; set; } = new();

        public HashSet<string> IgnoreParserFields { get; set; } = new();

        protected override ITransformField OnCreateInstance(IServiceProvider sp)
        {
            var result = new GroupField();
            foreach (var e in Fields)
            {
                var item = e.CreateInstance(sp);
                if (e is ArrayFieldDef array && array.Flat)
                {
                    if (result.FlatArray != null)
                        throw new Exception($"Not except multiple flat {nameof(ArrayFieldDef)} in the same hierarchy.");

                    result.FlatArray = (ArrayField)item;
                }
                else
                    result.Fields.Add(item);
            }

            return result;
        }
    }

    public class GroupField : TransformField<TransformResult>
    {
        public ArrayField FlatArray;
        public List<ITransformField> Fields = new();

        protected override TransformResult Transform(ExtractedRecord record)
        {
            IDictionary<string, object> newRecord = null;
            var result = FlatArray?.Transform(record) ?? new TransformResult();
            if (result.Items.Count == 0)
                result.Items.Add(newRecord = new Dictionary<string, object>());

            try
            {
                foreach (var field in Fields)
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
