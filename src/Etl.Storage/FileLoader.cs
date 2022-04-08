using Etl.Core.Load;
using Etl.Core.Utils;
using System.IO;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public abstract class FileLoader<TDef, TInst> : Loader<TInst>
        where TDef : FileLoader<TDef, TInst>
        where TInst : FileLoaderInst<TDef, TInst>
    {
        [XmlAttribute]
        public string OutPath { get; set; } = "$path/$name.result";
    }

    public abstract class FileLoaderInst<TDef, TInst> : LoaderInst<TDef>
        where TDef : FileLoader<TDef, TInst>
        where TInst : FileLoaderInst<TDef, TInst>

    {
        protected StreamWriter _stream;

        protected override void Initalize(TDef definition, LoaderArgs args)
        {
            var file = new FileInfo(FilePath.GetFullPath(args.InputFile));
            var path = definition.OutPath.Replace("$path", file.DirectoryName)
                .Replace("$name", file.Name);

            _stream = new StreamWriter(path);
        }

        public sealed override void ProcessBatch(BatchResult result)
        {
            OnProcessBatch(result);

            if (result.IsLast)
                OnCompleted();
        }

        public abstract void OnProcessBatch(BatchResult result);

        protected virtual void OnCompleted()
            => _stream?.Dispose();
    }
}
