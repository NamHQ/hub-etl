using System.Collections.Generic;

namespace Etl.Core.Settings
{
    public class TransformationSetting
    {
        public List<string> References { get; set; } = new();
        public CryptorSetting Cryptor { get; set; } = new();
    }
}
