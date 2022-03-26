namespace Etl.Core.Events
{
    public class LogOptions
    {
        public bool OnScanned { get; set; }

        public bool OnExtracting { get; set; }

        public bool OnExtracted { get; set; } 

        public bool OnTransformed { get; set; } 

        public bool OnTransformedBatch { get; set; }

        public bool OnError { get; set; } 
    }
}
