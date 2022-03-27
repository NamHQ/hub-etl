using System.Collections.Generic;

namespace Etl.Core.Settings
{
    public class ConfigFilesSetting
    {
        public string Folder { get; set; } = "./EtlConfigs";

        public Dictionary<string, string> Matches { get; set; } = new();
    }
}
