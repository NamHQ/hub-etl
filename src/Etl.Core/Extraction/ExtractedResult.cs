using Etl.Core.Scanner;
using System;
using System.Collections.Generic;
using System.Text;

namespace Etl.Core.Extraction
{
    public class ExtractedValue
    {
        private readonly Lazy<string> _lazyValue;
        private readonly Lazy<(string position, IEnumerable<string> lines)> _lazyInfo;
        public string Value => _lazyValue.Value;
        public string Position => _lazyInfo.Value.position;
        public IEnumerable<string> Lines => _lazyInfo.Value.lines;

        public ExtractedValue(TextBlock block, (int row, int col) from, (int row, int col) to)
        {
            _lazyValue = new Lazy<string>(() => block.GetValue(from, to));

            _lazyInfo = new Lazy<(string position, IEnumerable<string> lines)>(() =>
            {
                var startIndex = Math.Min(block.Count - 1, from.row);
                var endIndex = Math.Min(block.Count - 1, to.row);

                var sb = new StringBuilder();
                sb.Append($"From(R.{block[startIndex].Row}, {WriteColumn(from)})");
                sb.Append($"-To(R.{block[endIndex].Row}, {WriteColumn(to)})");

                var lines = new List<string>();

                for (var i = startIndex; i <= endIndex; i++)
                    lines.Add(block[i].Text);

                return (sb.ToString(), lines);
            });
        }

        private static string WriteColumn((int row, int column) pos)
                => pos.column == int.MaxValue ? "~" : pos.column.ToString();
    }
}
