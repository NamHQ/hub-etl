using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Extraction
{
    public class ExtractionDef
    {
        [XmlAttribute]
        public string LayoutStart { get; set; }

        [XmlAttribute]
        public int LayoutStartOffset { get; set; }

        [XmlArrayItem("Layout", typeof(LayoutDef))]
        public List<LayoutDef> Comments { get; set; } = new();

        public LayoutDef Layout { get; set; }
    }
}
