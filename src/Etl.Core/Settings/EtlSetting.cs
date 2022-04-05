namespace Etl.Core.Settings
{
    public class EtlSetting
    {
        public ConfigFilesSetting ConfigFiles { get; set; } = new();

        public ExtractionSetting Extraction { get; set; } = new();

        public TransformationSetting Transformation { get; set; } = new();

        public LoadSetting Load { get; set; } = new();
    }
}
