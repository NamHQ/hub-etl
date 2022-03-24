using System.Collections.Generic;

namespace Etl.Core.Scanner
{
    public class TextBlock
    {
        private readonly IReadOnlyList<TextLine> _lines;

        public readonly int Count;
        public TextLine this[int index]
            => _lines[index];

        public TextBlock(List<TextLine> lines)
        {
            _lines = lines;
            Count = _lines.Count;
        }

        public string GetValue(int row, int startColumn, int length)
            => GetValue(_lines[row].Text, startColumn, length);

        public string GetValue((int row, int col) start, (int row, int col) end)
            => GetValue(start.row, start.col, end.row, end.col);
        

        public string GetValue(int startRow, int startColumn, int endRow, int endColumn)
        {
            if (startRow == endRow)
                return GetValue(_lines[startRow].Text, startColumn, endColumn - startColumn);

            var result = new List<string>();

            for (var y = startRow; y < endRow; y++)
            {
                var line = GetValue(_lines[y].Text, startColumn, endColumn - startColumn);
                result.Add(line);
            }

            return string.Join('\n', result);
        }

        private static string GetValue(string text, int startIndex, int length, bool isTrim = true)
        {
            if (text.Length == 0 || length < 1)
                return string.Empty;

            if (startIndex + length > text.Length)
                length = text.Length - startIndex;

            if (isTrim)
            {
                var end = startIndex + length - 1;
                while (end > startIndex && (char.IsWhiteSpace(text[end]) || text[end] == 0))
                    end--;

                while (startIndex < end && (char.IsWhiteSpace(text[startIndex]) || text[startIndex] == 0))
                    startIndex++;

                length = end + 1 - startIndex;
            }

            return startIndex < text.Length ? text.Substring(startIndex, length) : string.Empty;
        }
    }
}
