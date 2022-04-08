using System;
using System.IO;

namespace Etl.Core.Utils
{
    public class FilePath
    {
        public static string GetFullPath(string path)
        {
            path = path.StartsWith(".") ? $"{AppContext.BaseDirectory}{path}" : path;
            return Path.GetFullPath(path);
        }
    }
}
