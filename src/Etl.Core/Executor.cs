using System.Collections.Generic;
using Etl.Core.Events;
using Etl.Core.Scanner;
using Etl.Core.Extraction;
using Etl.Core.Transformation;
using System.Text.RegularExpressions;
using System;
using Etl.Core.Load;
using System.Linq;
using Etl.Core.Transformation.Fields;
using System.IO;

namespace Etl.Core
{
    public class Executor
    {
        private readonly Extractor _extractor;
        private readonly Transformer _transformer;

        private readonly (Regex regex, int offset) _startLayout;
        private readonly (Regex regex, int offset) _startRecord;
        private readonly (Regex regex, int offset) _endRecord;
        private readonly List<Extractor> _layoutComments = new();

        private Func<List<IDictionary<string, object>>, List<IDictionary<string, object>>> _massageInstance;


        public IReadOnlyCollection<FieldBase> AllFields => _transformer.AllFields;

        public Executor(EtlDef etfDef)
        {
            var extraction = etfDef.Extraction;

            _extractor = new Extractor(extraction.Layout);
            _transformer = new Transformer(etfDef.Transformation, extraction.Layout);

            _startLayout = (string.IsNullOrWhiteSpace(extraction.LayoutStart) ? null : new Regex(extraction.LayoutStart, RegexOptions.Compiled), extraction.LayoutStartOffset);
            _startRecord = (string.IsNullOrWhiteSpace(extraction.Layout.Start) ? null : new Regex(extraction.Layout.Start, RegexOptions.Compiled), extraction.Layout.StartOffset);
            _endRecord = (string.IsNullOrWhiteSpace(extraction.Layout.End) ? null : new Regex(extraction.Layout.End, RegexOptions.Compiled), extraction.Layout.EndOffset);

            _layoutComments = extraction.Comments?.Select(e => new Extractor(e)).ToList();
        }

        public Scanner.Scanner CreateScanner(Func<StreamReader> getStreamReader, Action<List<TextLine>> flush)
        {
            _massageInstance = _transformer.CreateTranformInstance();

            return new(getStreamReader(), _startLayout, _startRecord, _endRecord, flush);
        }

        public void RemoveComments(List<TextLine> textLines)
        {
            if (_layoutComments == null || _layoutComments.Count == 0 || textLines == null)
                return;

            var i = 0;
            var current = i;
            while(i++ < textLines.Count)
            {
                current = i;
                _layoutComments.ForEach(e => i = Math.Max(e.DetectComments(textLines, current), i));

                if (i > current)
                    textLines.RemoveRange(current, Math.Min(i, textLines.Count) - current);
                i = current;
            }
        }

        public IDictionary<string, object> Extract(List<TextLine> textLines, ICompilerEvent events)
            => _extractor.Parse(textLines, events);

        public TransformResult Transform(IDictionary<string, object> record, Context context)
            => _transformer.ExtractFields(record, context);

        public List<IDictionary<string, object>> ApplyMassage(List<IDictionary<string, object>> batch)
            => _massageInstance == null ? batch : _massageInstance(batch);
    }
}
