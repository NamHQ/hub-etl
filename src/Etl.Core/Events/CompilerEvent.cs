using Etl.Core.Extraction;
using Etl.Core.Load;
using Etl.Core.Scanner;
using System;
using System.Collections.Generic;

namespace Etl.Core.Events
{
    public class CompilerEvent : ICompilerEvent
    {
        public Action<int> OnStart { get; set; }

        public Action<IReadOnlyCollection<TextLine>> OnScanned { get; set; }

        public Action<LayoutDef, int, TextBlock, (int row, int column)?, (int row, int column)?, (int row, int column)?, string, string> OnParsing { get; set; }

        public Action<IDictionary<string, object>> OnExtracted { get; set; }

        public Action<object> OnTransformed { get; set; }

        public Action<BatchResult> OnTransformedBatch { get; set; }

        public Action<string, Exception> OnError { get; set; }
    }
}
