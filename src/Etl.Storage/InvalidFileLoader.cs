using Etl.Core.Load;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public class InvalidFileLoader : FileLoader<InvalidFileLoader, InvalidFileLoaderInst>
    {
        [XmlAttribute]
        public override string OutPath { get; set; } = "$path/$name.invalid";
    }

    public class InvalidFileLoaderInst : FileLoaderInst<InvalidFileLoader, InvalidFileLoaderInst>
    {
        public override void OnProcessBatch(BatchResult result)
        {
            if (result.Errors != null)
                foreach (var error in result.Errors)
                    _stream.WriteLine(error);
        }
    }
}
