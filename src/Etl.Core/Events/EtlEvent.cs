using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Scanner;
using Etl.Core.Transformation;
using System;
using System.Collections.Generic;

namespace Etl.Core.Events
{
    class EtlEvent : IEtlEvent
    {
        public Action<IReadOnlyCollection<TextLine>> OnScanned { get; set; }

        public Action<Layout, int, TextBlock, (int row, int column)?, (int row, int column)?, (int row, int column)?, string, IExtractedInfo> OnExtracting { get; set; }

        public Action<ExtractedRecord> OnExtracted { get; set; }

        public Action<TransformResult> OnTransformed { get; set; }

        public Action<BatchResult> OnTransformedBatch { get; set; }

        public Action<string, Exception> OnError { get; set; }

        public (int seconds, Action<IEtlStatus> onStatus) OnStatusInterval { get; set; }
    }
}
