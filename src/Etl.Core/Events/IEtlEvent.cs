using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Scanner;
using Etl.Core.Transformation;
using System;
using System.Collections.Generic;

namespace Etl.Core.Events
{
    public interface IEtlEvent
    {
        Action<IReadOnlyCollection<TextLine>> OnScanned { get; set; }

        Action<Layout, int, TextBlock, (int row, int column)?, (int row, int column)?, (int row, int column)?, string, IExtractedInfo> OnExtracting { get; set; }

        Action<ExtractedRecord> OnExtracted { get; set; }

        Action<TransformResult> OnTransformed { get; set; }

        Action<BatchResult> OnTransformedBatch { get; set; }

        Action<string, Exception> OnError { get; set; }

        (int seconds, Action<IEtlStatus> onStatus) OnStatusInterval { get; set; }
    }
}
