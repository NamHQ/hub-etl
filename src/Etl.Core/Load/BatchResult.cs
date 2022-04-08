using System;
using System.Collections.Generic;
using System.Text;

namespace Etl.Core.Load
{
    public class BatchResult
    {
        public List<IDictionary<string, object>> Batch;
        public IEnumerable<string> Errors;
        public DateTime StartAt;
        public int TotalTransformSuccess;
        public int TotalTransformErrors;
        public bool IsLast;

        public int TotalRecords => TotalTransformSuccess + TotalTransformErrors;

        public override string ToString()
        {
            var t = DateTime.Now.Subtract(StartAt);

            var sb = new StringBuilder($"total: {TotalRecords}, valid: {TotalTransformSuccess}, errors: {TotalTransformErrors}, spend: {t}, batch {Batch.Count}");

            if (Errors != null)
            {
                sb.AppendLine();
                foreach (var e in Errors)
                    sb.AppendLine(e);
            }

            return sb.ToString();
        }
    }
}
