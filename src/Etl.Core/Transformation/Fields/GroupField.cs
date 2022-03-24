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

        public override object GetValue(IDictionary<string, object> record, Context context)
        {
            var result = new Dictionary<string, object>();

            foreach (var field in Fields)
            {
                var val = field.GetValue(record, context);
                if (val != null)
                    result[field.LazyDbField.Value] = val;
            }

            return result;
        }
    }
}
