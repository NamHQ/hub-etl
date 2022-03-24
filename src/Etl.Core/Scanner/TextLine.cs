namespace Etl.Core.Scanner
{
    public class TextLine
    {
        public static readonly TextLine End = new(string.Empty, -1);

        public readonly string Text;
        public readonly int Row;

        public TextLine(string text, int row)
        {
            Text = text;
            Row = row;
        }

        public override string ToString()
        {
            return $"R.{Row}:{Text}";
        }
    }
}
