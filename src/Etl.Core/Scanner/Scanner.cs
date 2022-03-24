using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Etl.Core.Scanner
{
    public class Scanner : IDisposable
    {
        private readonly (Regex regex, int offset) _startLayout;
        private readonly (Regex regex, int offset) _startRecord;
        private readonly (Regex regex, int offset) _endRecord;
        private readonly StreamReader _streamReader;

        private int _currentLine = 0;
        private bool _disposedValue;

        private readonly Action<List<TextLine>> _onFlush;

        public Scanner(
            StreamReader streamReader,
            (Regex regex, int offset) startLayout,
            (Regex regex, int offset) startRecord,
            (Regex regex, int offset) endRecord,
            Action<List<TextLine>> onFlush)
        {
            _streamReader = streamReader;
            _startLayout = startLayout;
            _startRecord = startRecord;
            _endRecord = endRecord;
            _onFlush = onFlush;
        }

        public void Start(int? take = null, int? skip = null)
        {
            var reader = GetEnumerator();
            reader.MoveNext();

            MoveTo(_startLayout.regex, _startLayout.offset, reader);

            while (reader.Current != TextLine.End)
            {
                MoveTo(_startRecord.regex, _startRecord.offset, reader);

                if (reader.Current == TextLine.End)
                    break;

                var items = GetTo(_endRecord.regex, _endRecord.offset, reader);

                if (skip == null || --skip < 0)
                {
                    if (take != null && --take < 0)
                        break;

                    _onFlush?.Invoke(items);
                }
            }

            _onFlush?.Invoke(null);
        }

        private static void MoveTo(Regex regex, int offset, IEnumerator<TextLine> reader)
        {
            while (reader.Current != TextLine.End && regex != null && !regex.IsMatch(reader.Current.Text))
                reader.MoveNext();

            while (reader.Current != TextLine.End && offset-- > 0)
                reader.MoveNext();
        }

        private static List<TextLine> GetTo(Regex regex, int offset, IEnumerator<TextLine> reader)
        {
            var items = new List<TextLine>();

            if (regex != null)
            {
                items.Add(reader.Current);
                reader.MoveNext();
                while (reader.Current != TextLine.End && !regex.IsMatch(reader.Current.Text))
                {
                    items.Add(reader.Current);
                    reader.MoveNext();
                }
            }

            while (reader.Current != TextLine.End && offset-- > 0)
            {
                items.Add(reader.Current);
                reader.MoveNext();
            }

            return items;
        }

        private IEnumerator<TextLine> GetEnumerator()
        {
            IEnumerable<TextLine> GetIEnumerable()
            {
                while (!_streamReader.EndOfStream)
                    yield return new TextLine(_streamReader.ReadLine(), ++_currentLine);

                yield return TextLine.End;
            }

            return GetIEnumerable().GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _streamReader?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
