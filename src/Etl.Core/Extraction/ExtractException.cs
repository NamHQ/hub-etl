using Etl.Core.Scanner;
using System;
using System.Text;

namespace Etl.Core.Extraction
{
    public class ExtractException : Exception
    {
        public LayoutDirection Direction { get; private set; }
        public (int row, int column) From { get; private set; }
        public (int row, int column) To { get; private set; }
        public int ToRow { get; private set; }
        public int ToColumn { get; private set; }

        public LayoutDef Layout { get; private set; }
        public TextBlock Block { get; private set; }

        public ExtractException(string message, LayoutDirection direction, (int row, int column) from, (int row, int column) to, LayoutDef layout, TextBlock block)
            : base(BuildMessage(message, layout, block, from, to))
        {
            Direction = direction;
            From = from;
            To = to;
            Layout = layout;
            Block = block;
        }

        private static string BuildMessage(string message, LayoutDef layout, TextBlock block, (int row, int column) from, (int row, int column) to)
        {
            var sb = new StringBuilder();

            sb.Append(message)
                .Append(':')
                .Append($"LayoutId={layout.Id}");

            var line = "R." + (from.row < block.Count ? block[from.row].Row.ToString() : block[block.Count - 1].Row.ToString() + "...");
            sb.Append($", From({line},{from.column})");

            line = "R." + (to.row < block.Count ? block[to.row].Row.ToString() : block[block.Count - 1].Row.ToString() + "...");
            sb.Append($", To({line},{to.column})");
            sb.AppendLine();

            for (var i = 0; i < block.Count; i++)
                sb.AppendLine(block[i].ToString());

            return sb.ToString();
        }
    }
}
