using System;

namespace Etl.Core
{
    public interface IEtlStatus
    {
        string FilePath { get; }
        DateTime Start { get; }
        float ScannerProgress { get; }

        int TotalRecords { get; }
        int TotalTransformSuccess { get; }
        int TotalTransformErrors { get; }

        float LoadBufferBatches { get; }

        int TotalLoadSuccess { get; }
        int TotalLoadErrors { get; }

        bool IsCompleted { get; }

    }

    class EtlStatus : IEtlStatus
    {
        public string FilePath { get; }

        public DateTime Start { get; set; } = DateTime.MinValue;

        public float ScannerProgress { get; set; }

        public int TotalTransformSuccess { get; set; }
        public int TotalTransformErrors { get; set; }
        public int TotalRecords => TotalTransformErrors + TotalTransformSuccess;
        public int TransformerWorkers { get; set; }

        public float LoadBufferBatches { get; set; }
        public int TotalLoadSuccess => TotalRecords - TotalLoadErrors;
        public int TotalLoadErrors { get; set; }

        public bool IsCompleted { get; set; }

        public EtlStatus(string filePath)
        {
            FilePath = filePath;
        }

        public override string ToString()
            => $"{DateTime.Now.Subtract(Start)}" +
                $", Scan: {ScannerProgress}% " +
                $", Extract: ({TransformerWorkers} workers - {TotalRecords} total records) " +
                $", Transform: ({TotalTransformSuccess} success - {TotalTransformErrors} errors)" +
                $", Load: ({LoadBufferBatches} bufferBatches - {TotalLoadSuccess} success, {TotalLoadErrors} errors)";
    }
}
