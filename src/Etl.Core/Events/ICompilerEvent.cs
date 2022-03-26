using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Scanner;
using Etl.Core.Transformation;
using System;
using System.Collections.Generic;

namespace Etl.Core.Events
{
    public interface ICompilerEvent
    {
        Action<IReadOnlyCollection<TextLine>> OnScanned { get; }

        Action<LayoutDef, int, TextBlock, (int row, int column)?, (int row, int column)?, (int row, int column)?, string, ExtractedResult> OnExtracting { get; }

        Action<IDictionary<string, object>> OnExtracted { get; }

        Action<TransformResult> OnTransformed { get; }

        Action<BatchResult> OnTransformedBatch { get; }

        Action<string, Exception> OnError { get; }
    }
}
