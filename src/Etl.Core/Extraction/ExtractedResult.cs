using Etl.Core.Scanner;
using System;
using System.Text;

namespace Etl.Core.Extraction
{
    public class ExtractedResult
    {
        private readonly Lazy<string> _lazyValue;
        private readonly Lazy<string> _layzPosition;
        public string Value => _lazyValue.Value;
        public string Position => _layzPosition.Value;

        public ExtractedResult(TextBlock block, (int row, int col) from, (int row, int col) to)
        {
            _lazyValue = new Lazy<string>(() => block.GetValue(from, to));

            _layzPosition = new Lazy<string>(() =>
            {
                var sb = new StringBuilder();
                sb.Append($"From(R.{block[Math.Min(block.Count - 1, from.row)].Row}, {WriteColumn(from)})");
                sb.Append($", To(R.{block[Math.Min(block.Count - 1, to.row)].Row}, {WriteColumn(to)})");
                return sb.ToString();
            });
        }

        private static string WriteColumn((int row, int column) pos)
                => pos.column == int.MaxValue ? "~" : pos.column.ToString();
    }
}
