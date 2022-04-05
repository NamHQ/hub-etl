using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Etl.Core.Settings
{
    public class LoadSetting
    {
        public List<string> References { get; set; } = new ();

        public IConfigurationSection Loaders { get; set; }
    }
}
