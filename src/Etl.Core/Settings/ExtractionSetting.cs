namespace Etl.Core.Settings
{
    public class ExtractionSetting
    {
        public int MaxThread { get; set; } = 2;
        public int MaxBatchBuffer { get; set; } = 100;
    }
}
