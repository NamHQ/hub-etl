using Etl.Core.Events;
using Etl.Core.Scanner;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Etl.Core.Extraction
{
    public class Layout
    {
        private static (int row, int column) STOP = (int.MaxValue, int.MaxValue);

        private readonly LayoutDef _layout;
        private readonly int _hierarchy;
        private readonly LayoutDirection _parentDirection;
        private readonly Regex _regexStart;
        private readonly Regex _regexEnd;
        private readonly List<Layout> _children;
        private readonly bool _isLast;

        public Layout(LayoutDef layout, LayoutDirection parentDirection = LayoutDirection.Row, bool isLast = false, int hierarchy = 0)
        {
            _layout = layout;
            _parentDirection = parentDirection;
            _isLast = isLast;
            _hierarchy = hierarchy;
            _regexStart = string.IsNullOrWhiteSpace(layout.Start) ? null : new Regex(layout.Start, RegexOptions.Compiled);
            _regexEnd = string.IsNullOrWhiteSpace(layout.End) ? null : new Regex(layout.End, RegexOptions.Compiled);

            if (layout.Children != null && layout.Children.Count > 0)
            {
                _children = new();
                var lastIndex = layout.Children.Count - 1;
                for (var i = 0; i <= lastIndex; i++)
                {
                    var child = layout.Children[i];
                    isLast = i == lastIndex && (child.Children == null || child.Children.Count == 0);
                    _children.Add(new Layout(child, layout.Direction, isLast, hierarchy + 1));
                }
            }
        }

        public int DetectComments(List<TextLine> textLines, int row)
        {
            if (_layout.MaxStart == 1 && (row >= textLines.Count || _regexStart == null || !_regexStart.IsMatch(textLines[row].Text)))
                return row;

            var next = row;
            if (_regexStart != null)
            {
                var count = _layout.MaxStart == 0 ? int.MaxValue : _layout.MaxStart;
                var isMatch = false;
                while (next < textLines.Count && count-- > 0 && !(isMatch = _regexStart.IsMatch(textLines[next].Text)))
                    next++;

                if (!isMatch)
                    return row;
            }
            next += _layout.StartOffset;

            if (_regexEnd != null)
            {
                var isMatch = false;
                var temp = next;
                while (++temp < textLines.Count && !(isMatch = _regexEnd.IsMatch(textLines[temp].Text))) { }

                if (isMatch)
                    next = temp;
            }
            else
                _children?.ForEach(e => next = e.DetectComments(textLines, next));

            next += _layout.EndOffset;

            return next;
        }

        public IDictionary<string, object> Parse(List<TextLine> textLines, ICompilerEvent events)
        {
            var result = new Dictionary<string, object>();
            var textBlock = new TextBlock(textLines);

            DetectChildrenLayouts((0, 0), (textBlock.Count, int.MaxValue), textBlock, events, result);

            return result;
        }

        private (int row, int column) DetectChildrenLayouts(
            (int row, int column) start,
            (int row, int column) end,
            TextBlock block,
            ICompilerEvent events,
            IDictionary<string, object> result)
        {
            var line = start.row;
            var column = start.column;

            if (_layout.Direction == LayoutDirection.Row)
                _children?.ForEach(child =>
                {
                    var next = child.DetectDataField((line, start.column), end, block, events, result);
                    line = next.row;
                });
            else
                _children?.ForEach(child =>
                {
                    var next = child.DetectDataField((start.row, column), end, block, events, result);
                    column = next.column;
                    if (line < next.row)
                        line = next.row;
                });

            return (line, column);
        }

        private (int row, int column) DetectDataField(
            (int row, int column) start,
            (int row, int column) end,
            TextBlock block,
            ICompilerEvent events,
            IDictionary<string, object> result)
        {
            var records = _layout.Repeat ? new List<IDictionary<string, object>>() : null;
            var cursor = start;

            do
            {
                events?.OnExtracting?.Invoke(_layout, _hierarchy, block, cursor, null, null, null, null);

                var record = _layout.Repeat ? new Dictionary<string, object>() : result;

                var newStart = GetFromPosition(cursor, end, block);
                if (newStart == STOP)
                {
                    events?.OnExtracting?.Invoke(_layout, _hierarchy, block, null, null, newStart, null, null);
                    break;
                }

                var (to, newEnd) = GetToPosition(newStart, end, block);

                ExtractedResult value = default;
                if (!_layout.Repeat && _children == null && !string.IsNullOrEmpty(_layout.DataField))
                {
                    value = GetValue(block, newStart, to, end);
                    record[_layout.DataField] = value;
                }

                events?.OnExtracting?.Invoke(_layout, _hierarchy, block, null, newStart, newEnd, _layout.DataField, value);

                var (line, column) = _children == null ? newStart : DetectChildrenLayouts(newStart, newEnd, block, events, record);

                if (records != null && record.Count > 0)
                    records.Add(record);

                cursor = (
                    Math.Max(to.row, line),
                    _parentDirection == LayoutDirection.Column ? Math.Max(to.column, column) : to.column);

                events?.OnExtracting?.Invoke(_layout, _hierarchy, block, null, null, cursor, null, null);
            }
            while (_layout.Repeat);

            if (records?.Count > 0)
            {
                if (string.IsNullOrEmpty(_layout.DataField))
                    throw new Exception($"Repeat Layout expects {nameof(_layout.DataField)}");
                result[_layout.DataField] = records;
            }

            return cursor;
        }

        private (int row, int column) GetFromPosition((int row, int column) from, (int row, int column) end, TextBlock block)
        {
            var row = from.row;
            var column = from.column;
            var endRow = Math.Min(end.row, block.Count);

            if (_parentDirection == LayoutDirection.Row)
            {
                if (_regexStart != null)
                {
                    var count = _layout.MaxStart == 0 ? int.MaxValue : _layout.MaxStart;

                    var isMatch = true;
                    while (row < endRow
                        && count-- > 0
                        && (column >= block[row].Text.Length || !(isMatch = _regexStart.IsMatch(block[row].Text, column))))
                        row++;

                    if (!isMatch)
                        return STOP;
                }

                row += _layout.StartOffset;
                if (row >= endRow)
                    return STOP;
            }
            else
            {
                var textLine = row < block.Count ? block[row] : null;
                if (_regexStart != null && textLine != null)
                {
                    var match = column >= textLine.Text.Length ? null : _regexStart.Match(textLine.Text, column);

                    if (match != null && match.Success)
                        column = match.Index;
                    else
                        return STOP;
                }

                column += _layout.StartOffset;
                if (column < 0 || column >= textLine?.Text.Length)
                    return STOP;
            }

            return (row, column);
        }

        private ((int row, int column) to, (int row, int column) newEnd) GetToPosition((int row, int column) from, (int row, int column) end, TextBlock block)
        {
            if (_regexEnd == null && _layout.EndOffset == 0)
                return (from, end);

            var row = from.row;
            var column = from.column;
            var newEnd = end;

            if (_parentDirection == LayoutDirection.Row)
            {
                if (_regexEnd != null)
                {
                    var stop = Math.Min(end.row, block.Count);
                    row++;
                    while (row < stop
                         && (column >= block[row].Text.Length || !_regexEnd.IsMatch(block[row].Text, column)))
                        row++;
                }

                newEnd.row = row += _layout.EndOffset;
            }
            else
            {
                if (_regexEnd != null)
                {
                    if (_layout.Start != null && _layout.StartOffset == 0)
                        column += 1;

                    var match = row >= block.Count || column >= block[row].Text.Length ? null : _regexEnd.Match(block[row].Text, column);

                    if (match != null && match.Success)
                        column = match.Index;
                    else
                        return ((row, end.column), end);
                }

                newEnd.column = column += _layout.EndOffset;
            }

            return ((row, column), newEnd);
        }

        private ExtractedResult GetValue(TextBlock block, (int row, int col) from, (int row, int col) to, (int row, int col) end)
        {
            if (_isLast && from == to)
            {
                if (_parentDirection == LayoutDirection.Row)
                    to.row = end.row;
                else
                    to.col = end.col;
            }

            return new ExtractedResult(block, from, to);
        }
    }
}
