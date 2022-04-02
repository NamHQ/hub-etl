using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core.Extraction
{
    public class Extractor
    {
        [XmlAttribute]
        public string LayoutStart { get; set; }

        [XmlAttribute]
        public int LayoutStartOffset { get; set; }

        [XmlArrayItem("Layout", typeof(Layout))]
        public List<Layout> Comments { get; set; } = new();

        public Layout Layout { get; set; }
    }
}
