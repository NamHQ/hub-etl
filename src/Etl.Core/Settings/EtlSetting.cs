namespace Etl.Core.Settings
{
    public class EtlSetting
    {
        public ConfigFilesSetting ConfigFiles = new();

        public ReferencesSetting References { get; set; } = new();

        public ExtractionSetting Extraction { get; set; } = new();

        public CryptorSetting Cryptor { get; set; } = new();
    }
}
