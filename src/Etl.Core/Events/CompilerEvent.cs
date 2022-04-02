using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Scanner;
using Etl.Core.Transformation;
using System;
using System.Collections.Generic;

namespace Etl.Core.Events
{
    public class CompilerEvent : ICompilerEvent
    {
        public Action<IReadOnlyCollection<TextLine>> OnScanned { get; set; }

        public Action<LayoutDef, int, TextBlock, (int row, int column)?, (int row, int column)?, (int row, int column)?, string, IExtractedInfo> OnExtracting { get; set; }

        public Action<ExtractedRecord> OnExtracted { get; set; }

        public Action<TransformResult> OnTransformed { get; set; }

        public Action<BatchResult> OnTransformedBatch { get; set; }

        public Action<string, Exception> OnError { get; set; }
    }
}
