using Etl.Core.Load;
using Etl.Core.Transformation.Fields;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Etl.Storage
{
    public abstract class FileLoader<TInst, TDef> : Loader<TInst, TDef>
        where TInst : FileLoaderInst<TInst, TDef>
        where TDef : FileLoader<TInst, TDef>
    {
        [XmlAttribute]
        public string OutPath { get; set; } = "$path/$name.result";
    }

    public abstract class FileLoaderInst<TInst, TDef> : LoaderInst<TInst, TDef>
        where TInst : FileLoaderInst<TInst, TDef>
        where TDef : FileLoader<TInst, TDef>
    {
        protected StreamWriter _stream;

        protected override void Initalize(TDef definition, string inputFile, IReadOnlyCollection<TransformField> fields)
        {
            var file = new FileInfo(inputFile);
            var path = definition.OutPath.Replace("$path", file.DirectoryName)
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
