﻿using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Transformation;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Etl.Core
{
    public class EtlDef
    {
        [XmlAttribute]
        public int ScanBatch { get; set; } = 10;

        [XmlAttribute]
        public int FlushBatch { get; set; } = 10 * 1000;

        public ExtractionDef Extraction { get; set; } = new();

        public TransformDef Transformation { get; set; } = new();

        public List<Loader> Loaders { get; set; } = new();
    }
}
