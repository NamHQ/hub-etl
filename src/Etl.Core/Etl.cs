﻿using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Scanner;
using Etl.Core.Extraction;
using Etl.Core.Transformation;
using System;
using Etl.Core.Transformation.Fields;
using System.IO;

namespace Etl.Core
{
    public class Etl
    {
        private readonly Extractor _extractor;
        private readonly Transformer _transformer;
        public IReadOnlyCollection<TransformFieldDef> AllFields => _transformer.AllFields;

        public Etl(EtlDef etfDef)
        {
            _extractor = new Extractor(etfDef.Extraction);
            _transformer = new Transformer(etfDef.Transformation, etfDef.Extraction.Layout);
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
