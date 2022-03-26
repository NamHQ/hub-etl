using System.Collections.Generic;

namespace Etl.Core.Transformation
{
    public class TransformResult
    {
        public int TotalErrors;

        private List<string> _errorMessages;
        public IEnumerable<string> Errors => _errorMessages;

        public List<IDictionary<string, object>> Items;

        public int TotalRecords => TotalErrors + Items.Count;

        public TransformResult(int? size = null)
        {
            Items = size == null ? new() : new(size.Value);
        }

        public TransformResult AddErorr(string message)
        {
            if (_errorMessages == null)
                _errorMessages = new List<string>();

            _errorMessages.Add(message);
            return this;
        }

        public TransformResult AddErorrs(IEnumerable<string> messages)
        {
            if (messages != null)
            {
                if (_errorMessages == null)
                    _errorMessages = new List<string>();

                _errorMessages.AddRange(messages);
            }
            return this;
        }

        public void Append(TransformResult result)
        {
            if (result == null)
                return;

            if (result.Items != null)
                Items.AddRange(result.Items);

            TotalErrors += result.TotalErrors;

            AddErorrs(result.Errors);
        }
    }
}
