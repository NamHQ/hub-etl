﻿using Etl.Core.Events;
using Etl.Core.Scanner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etl.Core.Extraction
{
    public class Extractor
    {
        private readonly Layout _layout;

        private readonly (Regex regex, int offset) _startLayout;
        private readonly (Regex regex, int offset) _startRecord;
        private readonly (Regex regex, int offset) _endRecord;
        private readonly List<Layout> _layoutComments = new();

        public Extractor(ExtractorDef definition)
        {
            _layout = new Layout(definition.Layout);

            _startLayout = (string.IsNullOrWhiteSpace(definition.LayoutStart) ? null : new Regex(definition.LayoutStart, RegexOptions.Compiled), definition.LayoutStartOffset);
            _startRecord = (string.IsNullOrWhiteSpace(definition.Layout.Start) ? null : new Regex(definition.Layout.Start, RegexOptions.Compiled), definition.Layout.StartOffset);
            _endRecord = (string.IsNullOrWhiteSpace(definition.Layout.End) ? null : new Regex(definition.Layout.End, RegexOptions.Compiled), definition.Layout.EndOffset);

            _layoutComments = definition.Comments?.Select(e => new Layout(e)).ToList();
        }

        public Scanner.Scanner CreateScanner(Func<StreamReader> getStreamReader, Action<List<TextLine>> flush)
        {
            var proxy = _layoutComments == null || _layoutComments.Count == 0
                ? flush
                : textLines =>
                {
                    if (textLines != null)
                        RemoveComments(textLines);
                    flush(textLines);
                };

            return new(getStreamReader(), _startLayout, _startRecord, _endRecord, proxy);
        }

        private void RemoveComments(List<TextLine> textLines)
        {
            var i = 0;
            var current = i;
            while (i++ < textLines.Count)
            {
                current = i;
                _layoutComments.ForEach(e => i = Math.Max(e.DetectComments(textLines, current), i));

                if (i > current)
                    textLines.RemoveRange(current, Math.Min(i, textLines.Count) - current);
                i = current;
            }
        }

        public ExtractedRecord Execute(List<TextLine> textLines, ICompilerEvent events)
            => _layout.ParseOneRecord(textLines, events);
    }
}
