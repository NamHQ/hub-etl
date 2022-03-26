using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Extraction
{
    [XmlRoot("Layout")]
    public class LayoutDef
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public LayoutDirection Direction { get; set; }

        [XmlAttribute]
        public string Start { get; set; }

        [XmlAttribute]
        public int StartOffset { get; set; }

        [XmlAttribute]
        public int MaxStart { get; set; }

        [XmlAttribute]
        public string End { get; set; }

        [XmlAttribute]
        public int EndOffset { get; set; }

        [XmlAttribute]
        public string DataField { get; set; }

        [XmlAttribute]
        public bool Repeat { get; set; }

        [XmlElement("Layout")]
        public List<LayoutDef> Children { get; set; } = new List<LayoutDef>();
    }
}
