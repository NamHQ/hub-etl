using Etl.Core.Load;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public abstract class FileLoaderDef<TLoader, TArgs> : LoaderDef<TLoader, TArgs>
        where TLoader : FileLoader<TLoader, TArgs>
        where TArgs : FileLoaderDef<TLoader, TArgs>
    {
        [XmlAttribute]
        public string OutPath { get; set; } = "$path/$name.result";
    }
}
