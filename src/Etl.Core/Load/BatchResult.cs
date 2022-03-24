using System;
using System.Collections.Generic;

namespace Etl.Core.Load
{
    public class BatchResult
    {
        public List<IDictionary<string, object>> Batch;
        public DateTime StartAt;
        public int TotalScannedRecords;
        public int TotalRecords;
        public int TotalValidRecords;
        public int TotalErrors;
        public bool IsLast;

        public override string ToString()
        {
            var t = DateTime.Now.Subtract(StartAt);
            return $"scaned: {TotalScannedRecords}, total: {TotalRecords}, valid: {TotalValidRecords}, errors: {TotalErrors}, spend: {t}, batch {Batch.Count}";
        }
    }
}
