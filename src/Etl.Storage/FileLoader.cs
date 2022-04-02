using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public abstract class FileLoaderDef<TLoader, TArgs> : Loader<TLoader, TArgs>
        where TLoader : FileLoaderInst<TLoader, TArgs>
        where TArgs : FileLoaderDef<TLoader, TArgs>
    {
        [XmlAttribute]
        public string OutPath { get; set; } = "$path/$name.result";
    }

    public abstract class FileLoaderInst<TLoader, TDef> : LoaderInst<TLoader, TDef>
        where TLoader : FileLoaderInst<TLoader, TDef>
        where TDef : FileLoaderDef<TLoader, TDef>
    {
        protected StreamWriter _stream;

        protected override void Initalize(TDef args, string inputFile, IReadOnlyCollection<TransformField> fields)
        {
            var file = new FileInfo(inputFile);
            var path = args.OutPath.Replace("$path", file.DirectoryName)
                .Replace("$name", file.Name);

            _stream = new StreamWriter(path);
        }

        protected sealed override void ProcessBatch(BatchResult result)
        {
            OnProcessBatch(result);

            if (result.IsLast)
                OnCompleted();
        }

        protected abstract void OnProcessBatch(BatchResult result);


        protected virtual void OnCompleted()
            => _stream?.Dispose();
    }
}
