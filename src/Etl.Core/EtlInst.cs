using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Scanner;
using Etl.Core.Extraction;
using Etl.Core.Transformation;
using System;
using Etl.Core.Transformation.Fields;
using System.IO;

namespace Etl.Core
{
    public class EtlInst
    {
        private readonly ExtractorInst _extractor;
        private readonly TransformerInst _transformer;
        public IReadOnlyCollection<TransformField> AllFields => _transformer.AllFields;

        public EtlInst(Etl etl)
        {
            _extractor = new ExtractorInst(etl.Extraction);
            _transformer = new TransformerInst(etl.Transformation, etl.Extraction.Layout);
        }

        public (
            Scanner.Scanner scaner,
            Func<ExtractedRecord, TransformResult> transformInstance)
            Start(Func<StreamReader> getStreamReader, IServiceProvider sp, Action<List<TextLine>> flush)
        {
            return (
                _extractor.CreateScanner(getStreamReader, flush),
                _transformer.CreateInstance(sp));
        }

        public ExtractedRecord Extract(List<TextLine> textLines, ICompilerEvent events)
            => _extractor.Execute(textLines, events);

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _transformer.ApplyMassage(batch);
    }
}
