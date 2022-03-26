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
        public int TotalRecords;
        public int TotalValidRecords;
        public int TotalErrors;
        public bool IsLast;

        public override string ToString()
        {
            var t = DateTime.Now.Subtract(StartAt);

            var sb = new StringBuilder($"total: {TotalRecords}, valid: {TotalValidRecords}, errors: {TotalErrors}, spend: {t}, batch {Batch.Count}");

            if (Errors != null)
            {
                sb.AppendLine();
                foreach (var e in Errors)
                    sb.AppendLine($"\t{e}");
            }

            return sb.ToString();
        }
    }
}
