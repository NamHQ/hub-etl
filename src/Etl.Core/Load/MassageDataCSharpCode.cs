using System.Xml.Serialization;

namespace Etl.Core.Load
{
    public class MassageDataCSharpCode
    {
        [XmlAttribute]
        public string GlobalVariables { get; set; }

        public string Code { get; set; }
    }
}
