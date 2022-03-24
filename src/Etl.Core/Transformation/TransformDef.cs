using Etl.Core.Transformation.Fields;
using Etl.Core.Load;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Transformation
{
    public class TransformDef 
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
        public List<FieldBase> Fields { get; set; } = new();

        public MassageDataCSharpCode Massage { get; set; } = new();
    }
}
